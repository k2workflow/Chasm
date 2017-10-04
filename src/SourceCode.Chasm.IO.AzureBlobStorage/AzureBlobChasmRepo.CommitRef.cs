using Microsoft.WindowsAzure.Storage;
using SourceCode.Clay;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.AzureBlobStorage
{
    partial class AzureBlobChasmRepo // .CommitRef
    {
        #region Read

        public override CommitId ReadCommitRef(string branch, string name)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var (commitId, _) = ReadCommitRefImpl(branch, name);
            return commitId;
        }

        public override async ValueTask<CommitId> ReadCommitRefAsync(string branch, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var (commitId, _) = await ReadCommitRefImplAsync(branch, name, cancellationToken);
            return commitId;
        }

        private (CommitId, AccessCondition) ReadCommitRefImpl(string branch, string name)
        {
            var refsContainer = _refsContainer.Value;

            var blobName = DeriveCommitRefBlobName(branch, name);
            var blobRef = refsContainer.GetAppendBlobReference(blobName);

            var commitId = CommitId.Empty;
            AccessCondition accessCondition = null;
            try
            {
                using (var input = new MemoryStream())
                using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
                {
                    // Perf: Use a stream instead of a preceding call to fetch the buffer length
                    blobRef.DownloadToStreamAsync(input).Wait();

                    // Grab the etag - we need it for optimistic concurrency control
                    var etag = blobRef.Properties.ETag;
                    accessCondition = AccessCondition.GenerateIfMatchCondition(etag);

                    using (var output = new MemoryStream())
                    {
                        input.Position = 0; // Else gzip returns []
                        gzip.CopyTo(output);

                        if (output.Length > 0)
                        {
                            var sha1 = Serializer.DeserializeSha1(output.ToArray()); // TODO: Perf
                            commitId = new CommitId(sha1);
                        }
                    }
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                // Try-catch is cheaper than a separate exists check
                se.Suppress();
            }

            return (commitId, accessCondition);
        }

        private async ValueTask<(CommitId, AccessCondition)> ReadCommitRefImplAsync(string branch, string name, CancellationToken cancellationToken)
        {
            var refsContainer = _refsContainer.Value;

            var blobName = DeriveCommitRefBlobName(branch, name);
            var blobRef = refsContainer.GetAppendBlobReference(blobName);

            var commitId = CommitId.Empty;
            AccessCondition accessCondition = null;
            try
            {
                using (var input = new MemoryStream())
                using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
                {
                    // Perf: Use a stream instead of a preceding call to fetch the buffer length
                    await blobRef.DownloadToStreamAsync(input, default, default, default, cancellationToken).ConfigureAwait(false);

                    // Grab the etag - we need it for optimistic concurrency control
                    var etag = blobRef.Properties.ETag;
                    accessCondition = AccessCondition.GenerateIfMatchCondition(etag);

                    using (var output = new MemoryStream())
                    {
                        input.Position = 0; // Else gzip returns []
                        gzip.CopyTo(output);

                        if (output.Length > 0)
                        {
                            var sha1 = Serializer.DeserializeSha1(output.ToArray()); // TODO: Perf
                            commitId = new CommitId(sha1);
                        }
                    }
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                // Try-catch is cheaper than a separate exists check
                se.Suppress();
            }

            return (commitId, accessCondition);
        }

        #endregion

        #region Write

        public override void WriteCommitRef(CommitId? previousCommitId, string branch, string name, CommitId newCommitId)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refsContainer = _refsContainer.Value;

            var blobName = DeriveCommitRefBlobName(branch, name);
            var blobRef = refsContainer.GetAppendBlobReference(blobName);

            // Load existing commit ref in order to use its ETAG
            var (existingCommitId, accessCondition) = ReadCommitRefImpl(branch, name);

            if (existingCommitId != CommitId.Empty)
            {
                // Idempotent
                if (existingCommitId == newCommitId)
                    return;

                // Optimistic concurrency check
                if (previousCommitId.HasValue
                    && existingCommitId != previousCommitId.Value)
                {
                    throw BuildConcurrencyException(branch, name, null);
                }
            }

            try
            {
                // Required to create blob before appending to it
                blobRef.CreateOrReplaceAsync(accessCondition, default, default, default).Wait(); // Note ETAG access condition

                using (var output = new MemoryStream())
                {
                    using (var gz = new GZipStream(output, CompressionLevel, true))
                    using (var session = Serializer.Serialize(newCommitId.Sha1))
                    {
                        var result = session.Result;
                        gz.Write(result.Array, result.Offset, result.Count);
                    }
                    output.Position = 0;

                    // Append blob. Following seems to be the only safe multi-writer method available
                    // http://stackoverflow.com/questions/32530126/azure-cloudappendblob-errors-with-concurrent-access
                    blobRef.AppendBlockAsync(output).Wait();
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
            {
                throw BuildConcurrencyException(branch, name, se);
            }
        }

        public override async Task WriteCommitRefAsync(CommitId? previousCommitId, string branch, string name, CommitId newCommitId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refsContainer = _refsContainer.Value;

            var blobName = DeriveCommitRefBlobName(branch, name);
            var blobRef = refsContainer.GetAppendBlobReference(blobName);

            // Load existing commit ref in order to use its ETAG
            var (existingCommitId, accessCondition) = await ReadCommitRefImplAsync(branch, name, cancellationToken).ConfigureAwait(false);

            if (existingCommitId != CommitId.Empty)
            {
                // Idempotent
                if (existingCommitId == newCommitId)
                    return;

                // Optimistic concurrency check
                if (previousCommitId.HasValue
                    && existingCommitId != previousCommitId.Value)
                {
                    throw BuildConcurrencyException(branch, name, null);
                }
            }

            try
            {
                // Required to create blob before appending to it
                await blobRef.CreateOrReplaceAsync(accessCondition, default, default, cancellationToken).ConfigureAwait(false); // Note ETAG access condition

                using (var output = new MemoryStream())
                {
                    using (var gz = new GZipStream(output, CompressionLevel, true))
                    using (var session = Serializer.Serialize(newCommitId.Sha1))
                    {
                        var result = session.Result;
                        gz.Write(result.Array, result.Offset, result.Count);
                    }
                    output.Position = 0;

                    // Append blob. Following seems to be the only safe multi-writer method available
                    // http://stackoverflow.com/questions/32530126/azure-cloudappendblob-errors-with-concurrent-access
                    await blobRef.AppendBlockAsync(output).ConfigureAwait(false);
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
            {
                throw BuildConcurrencyException(branch, name, se);
            }
        }

        #endregion

        #region Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string DeriveCommitRefBlobName(string branch, string name)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refName = $"{branch}.{name}.commit";
            refName = Uri.EscapeUriString(refName);

            return refName;
        }

        #endregion
    }
}
