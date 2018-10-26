using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SourceCode.Chasm.Serializer;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository.AzureBlob
{
    partial class AzureBlobChasmRepo // .CommitRef
    {
        #region Fields

        private const string CommitExtension = ".commit";

        #endregion

        #region List

        public override async ValueTask<IReadOnlyList<CommitRef>> GetBranchesAsync(string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            CloudBlobContainer refsContainer = _refsContainer.Value;
            var results = new List<CommitRef>();
            name = DeriveCommitRefBlobName(name, null);

            BlobContinuationToken token = null;

            do
            {
                BlobResultSegment resultSegment = await refsContainer.ListBlobsSegmentedAsync(name, false, BlobListingDetails.None, null, token, null, null).ConfigureAwait(false);

                foreach (IListBlobItem blobItem in resultSegment.Results)
                {
                    if (!(blobItem is CloudBlob blob))
                        continue;

                    if (!blob.Name.EndsWith(CommitExtension, StringComparison.OrdinalIgnoreCase))
                        continue;

                    string branch = blob.Name.Substring(name.Length, blob.Name.Length - CommitExtension.Length - name.Length);
                    branch = Uri.UnescapeDataString(branch);

                    using (var output = new MemoryStream())
                    {
                        await blob.DownloadToStreamAsync(output, default, default, default, cancellationToken).ConfigureAwait(false);

                        if (output.Length < Sha1.ByteLength)
                            throw new SerializationException($"{nameof(CommitRef)} '{name}/{branch}' expected to have byte length {Sha1.ByteLength} but has length {output.Length}");

                        CommitId commitId = Serializer.DeserializeCommitId(output.ToArray());
                        results.Add(new CommitRef(branch, commitId));
                    }
                }

                token = resultSegment.ContinuationToken;
            } while (token != null);

            return results;
        }

        public override async ValueTask<IReadOnlyList<string>> GetNamesAsync(CancellationToken cancellationToken)
        {
            CloudBlobContainer refsContainer = _refsContainer.Value;
            var results = new List<string>();

            BlobContinuationToken token = null;

            do
            {
                BlobResultSegment resultSegment = await refsContainer.ListBlobsSegmentedAsync("", false, BlobListingDetails.None, null, token, null, null).ConfigureAwait(false);

                foreach (IListBlobItem blobItem in resultSegment.Results)
                {
                    if (!(blobItem is CloudBlobDirectory dir))
                        continue;

                    string name = dir.Prefix.Substring(0, dir.Prefix.Length - 1); // Ends with / always.
                    name = Uri.UnescapeDataString(name);
                    results.Add(name);
                }

                token = resultSegment.ContinuationToken;
            } while (token != null);

            return results;
        }

        #endregion

        #region Read

        public override async ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));

            (bool found, CommitId commitId, AccessCondition _, CloudAppendBlob _) = await ReadCommitRefImplAsync(name, branch, cancellationToken).ConfigureAwait(false);

            // NotFound
            if (!found) return null;

            // Found
            var commitRef = new CommitRef(branch, commitId);
            return commitRef;
        }

        #endregion

        #region Write

        public override async Task WriteCommitRefAsync(CommitId? previousCommitId, string name, CommitRef commitRef, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (commitRef == CommitRef.Empty) throw new ArgumentNullException(nameof(commitRef));

            // Load existing commit ref in order to use its etag
            (bool found, CommitId existingCommitId, AccessCondition ifMatchCondition, CloudAppendBlob blobRef) = await ReadCommitRefImplAsync(name, commitRef.Branch, cancellationToken).ConfigureAwait(false);

            // Optimistic concurrency check
            if (found)
            {
                // We found a previous commit but the caller didn't say to expect one
                if (!previousCommitId.HasValue)
                    throw BuildConcurrencyException(name, commitRef.Branch, null); // TODO: May need a different error

                // Semantics follow Interlocked.Exchange (compare then exchange)
                if (existingCommitId != previousCommitId.Value)
                {
                    throw BuildConcurrencyException(name, commitRef.Branch, null);
                }

                // Idempotent
                if (existingCommitId == commitRef.CommitId) // We already know that the name matches
                    return;
            }
            // The caller expected a previous commit, but we didn't find one
            else if (previousCommitId.HasValue)
            {
                throw BuildConcurrencyException(name, commitRef.Branch, null); // TODO: May need a different error
            }

            try
            {
                // Required to create blob before appending to it
                await blobRef.CreateOrReplaceAsync(ifMatchCondition, default, default, cancellationToken).ConfigureAwait(false); // Note etag access condition

                // CommitIds are not compressed
                using (var pool = new SessionMemoryPool<byte>())
                {
                    Memory<byte> mem = Serializer.Serialize(commitRef.CommitId, pool);

                    using (var output = new MemoryStream())
                    {
                        output.Write(mem.Span);
                        output.Position = 0;

                        // Append blob. Following seems to be the only safe multi-writer method available
                        // http://stackoverflow.com/questions/32530126/azure-cloudappendblob-errors-with-concurrent-access
                        await blobRef.AppendBlockAsync(output).ConfigureAwait(false);
                    }
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                throw BuildConcurrencyException(name, commitRef.Branch, se);
            }
        }

        #endregion

        #region Helpers

        private static string DeriveCommitRefBlobName(string name, string branch)
        {
            if (string.IsNullOrEmpty(branch))
                return Uri.EscapeDataString(name) + "/";

            name = Uri.EscapeDataString(name);
            branch = Uri.EscapeDataString(branch);

            string refName = $"{name}/{branch}{CommitExtension}";
            return refName;
        }

        private async ValueTask<(bool found, CommitId, AccessCondition, CloudAppendBlob)> ReadCommitRefImplAsync(string name, string branch, CancellationToken cancellationToken)
        {
            // Callers have already validated parameters

            CloudBlobContainer refsContainer = _refsContainer.Value;
            string blobName = DeriveCommitRefBlobName(name, branch);
            CloudAppendBlob blobRef = refsContainer.GetAppendBlobReference(blobName);

            try
            {
                using (var output = new MemoryStream())
                {
                    // TODO: Perf: Use a stream instead of a preceding call to fetch the buffer length
                    // Or use blobRef.DownloadToByteArrayAsync() since we already know expected length of data (Sha1.ByteLen)
                    // Keep in mind Azure and/or specific IChasmSerializer may add some overhead: it looks like emperical byte length is 40-52 bytes
                    await blobRef.DownloadToStreamAsync(output, default, default, default, cancellationToken).ConfigureAwait(false);

                    // Grab the etag - we need it for optimistic concurrency control
                    var ifMatchCondition = AccessCondition.GenerateIfMatchCondition(blobRef.Properties.ETag);

                    if (output.Length < Sha1.ByteLength)
                        throw new SerializationException($"{nameof(CommitRef)} '{name}/{branch}' expected to have byte length {Sha1.ByteLength} but has length {output.Length}");

                    // CommitIds are not compressed
                    CommitId commitId = Serializer.DeserializeCommitId(output.ToArray()); // TODO: Perf

                    // Found
                    return (true, commitId, ifMatchCondition, blobRef);
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                // Try-catch is cheaper than a separate (latent) exists check
                se.Suppress();
            }

            // NotFound
            return (false, default, default, blobRef);
        }

        #endregion
    }
}
