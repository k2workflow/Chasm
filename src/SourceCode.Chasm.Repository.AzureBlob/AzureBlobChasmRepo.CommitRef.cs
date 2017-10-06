#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SourceCode.Clay;
using System;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.AzureBlob
{
    partial class AzureBlobChasmRepo // .CommitRef
    {
        #region Read

        private async ValueTask<(CommitId, AccessCondition)> ReadCommitRefImplAsync(CloudBlob blobRef, CancellationToken cancellationToken)
        {
            var commitId = CommitId.Empty;
            AccessCondition accessCondition = null;
            try
            {
                // CommitRefs are not compressed

                using (var output = new MemoryStream())
                {
                    // Perf: Use a stream instead of a preceding call to fetch the buffer length
                    await blobRef.DownloadToStreamAsync(output, default, default, default, cancellationToken).ConfigureAwait(false);

                    // Grab the etag - we need it for optimistic concurrency control
                    var etag = blobRef.Properties.ETag;
                    accessCondition = AccessCondition.GenerateIfMatchCondition(etag);

                    if (output.Length > 0)
                    {
                        var sha1 = Serializer.DeserializeSha1(output.ToArray()); // TODO: Perf
                        commitId = new CommitId(sha1);
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

        public async ValueTask<CommitId> ReadCommitRefAsync(string branch, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refsContainer = _refsContainer.Value;

            var blobName = DeriveCommitRefBlobName(branch, name);
            var blobRef = refsContainer.GetAppendBlobReference(blobName);

            var (commitId, _) = await ReadCommitRefImplAsync(blobRef, cancellationToken).ConfigureAwait(false);
            return commitId;
        }

        #endregion

        #region Write

        public async Task WriteCommitRefAsync(CommitId? previousCommitId, string branch, string name, CommitId newCommitId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refsContainer = _refsContainer.Value;

            var blobName = DeriveCommitRefBlobName(branch, name);
            var blobRef = refsContainer.GetAppendBlobReference(blobName);

            // Load existing commit ref in order to use its ETAG
            var (existingCommitId, accessCondition) = await ReadCommitRefImplAsync(blobRef, cancellationToken).ConfigureAwait(false);

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

                // CommitRefs are not compressed

                using (var session = Serializer.Serialize(newCommitId.Sha1))
                using (var output = new MemoryStream())
                {
                    var seg = session.Result;
                    output.Write(seg.Array, seg.Offset, seg.Count);
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

        private static ChasmConcurrencyException BuildConcurrencyException(string branch, string name, Exception innerException)
            => new ChasmConcurrencyException($"Concurrent write detected on {nameof(CommitRef)} {branch}/{name}", innerException);

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
