using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SourceCode.Chasm.Repository.Disk;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository.AzureBlob
{
    partial class AzureBlobChasmRepo // .Object
    {
        #region Read

        public override async Task<bool> ExistsAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            // Try disk repo first

            bool exists = await _diskRepo.ExistsAsync(objectId, cancellationToken)
                .ConfigureAwait(false);

            // Else go to cloud

            if (!exists)
                exists = await ExistsOnCloudAsync(objectId, cancellationToken)
                    .ConfigureAwait(false);

            return exists;
        }

        private async Task<bool> ExistsOnCloudAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            string blobName = DeriveBlobName(objectId);
            CloudBlobContainer objectsContainer = _objectsContainer.Value;
            CloudAppendBlob blobRef = objectsContainer.GetAppendBlobReference(blobName);

            bool exists = await blobRef.ExistsAsync()
                .ConfigureAwait(false);

            return exists;
        }

        public override async Task<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            // Try disk repo first

            ReadOnlyMemory<byte>? cached = await _diskRepo.ReadObjectAsync(objectId, cancellationToken)
                .ConfigureAwait(false);

            if (cached.HasValue)
                return cached.Value;

            // Else go to cloud

            string blobName = DeriveBlobName(objectId);
            CloudBlobContainer objectsContainer = _objectsContainer.Value;
            CloudAppendBlob blobRef = objectsContainer.GetAppendBlobReference(blobName);

            try
            {
                using (var input = new MemoryStream())
                {
                    // TODO: Perf: Use a stream instead of a preceding call to fetch the buffer length
                    await blobRef.DownloadToStreamAsync(input)
                        .ConfigureAwait(false);

                    byte[] buffer = input.ToArray(); // TODO: Perf
                    return buffer;
                }
            }
            // Try-catch is cheaper than a separate (latent) exists check
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                se.Suppress();
                return default;
            }
        }

        public override async Task<Stream> ReadStreamAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            // Try disk repo first

            Stream cached = await _diskRepo.ReadStreamAsync(objectId, cancellationToken)
                .ConfigureAwait(false);

            if (cached != null)
                return cached;

            // Else go to cloud

            string blobName = DeriveBlobName(objectId);
            CloudBlobContainer objectsContainer = _objectsContainer.Value;
            CloudAppendBlob blobRef = objectsContainer.GetAppendBlobReference(blobName);

            try
            {
                var input = new MemoryStream();

                // TODO: Perf: Use a stream instead of a preceding call to fetch the buffer length
                await blobRef.DownloadToStreamAsync(input)
                    .ConfigureAwait(false);

                return input;
            }
            // Try-catch is cheaper than a separate (latent) exists check
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                se.Suppress();
                return default;
            }
        }

        #endregion

        #region Write

        /// <summary>
        /// Writes a buffer to the destination, returning the content's <see cref="Sha1"/> value.
        /// </summary>
        /// <param name="buffer">The content to hash and write.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override async Task<ChasmResult<Sha1>> WriteObjectAsync(Memory<byte> buffer, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var idempotentSuccess = false;

            async ValueTask UploadAsync(Sha1 sha1, string filePath)
            {
                idempotentSuccess = await IdempotentUploadAsync(filePath, sha1, forceOverwrite, cancellationToken)
                    .ConfigureAwait(false);
            }

            Sha1 objectId = await DiskChasmRepo.WriteFileAsync(buffer, UploadAsync, cancellationToken)
                .ConfigureAwait(false);

            return new ChasmResult<Sha1>(objectId, idempotentSuccess);
        }

        /// <summary>
        /// Writes a stream to the destination, returning the content's <see cref="Sha1"/> value.
        /// </summary>
        /// <param name="stream">The content to hash and write.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override async Task<ChasmResult<Sha1>> WriteObjectAsync(Stream stream, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var idempotentSuccess = false;

            async ValueTask UploadAsync(Sha1 sha1, string filePath)
            {
                idempotentSuccess = await IdempotentUploadAsync(filePath, sha1, forceOverwrite, cancellationToken)
                    .ConfigureAwait(false);
            }

            Sha1 objectId = await DiskChasmRepo.WriteFileAsync(stream, UploadAsync, cancellationToken)
                .ConfigureAwait(false);

            return new ChasmResult<Sha1>(objectId, idempotentSuccess);
        }

        /// <summary>
        /// Writes a stream to the destination, returning the content's <see cref="Sha1"/> value.
        /// The <paramref name="beforeHash"/> function permits a transformation operation
        /// on the source value before calculating the hash and writing to the destination.
        /// For example, the source stream may be encoded as Json.
        /// </summary>
        /// <param name="beforeHash">An action to take on the internal stream, before calculating the hash.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        /// <remarks>Note that the <paramref name="beforeHash"/> function should maintain the integrity
        /// of the source stream: the hash will be taken on the result of this operation.
        /// For example, transforming to Json is appropriate but compression is not since the latter
        /// is not a representative model of the original content, but rather a storage optimization.</remarks>
        public override async Task<ChasmResult<Sha1>> WriteObjectAsync(Func<Stream, ValueTask> beforeHash, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (beforeHash == null) throw new ArgumentNullException(nameof(beforeHash));

            var idempotentSuccess = false;

            async ValueTask UploadAsync(Sha1 sha1, string filePath)
            {
                idempotentSuccess = await IdempotentUploadAsync(filePath, sha1, forceOverwrite, cancellationToken)
                    .ConfigureAwait(false);
            }

            Sha1 objectId = await DiskChasmRepo.WriteFileAsync(beforeHash, UploadAsync, cancellationToken)
                .ConfigureAwait(false);

            return new ChasmResult<Sha1>(objectId, idempotentSuccess);
        }

        private async ValueTask<bool> IdempotentUploadAsync(string filePath, Sha1 objectId, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (!forceOverwrite)
            {
                bool exists = await ExistsOnCloudAsync(objectId, cancellationToken)
                    .ConfigureAwait(false);

                // Idempotent success (already exists)
                if (exists)
                    return true;
            }

            // Objects are immutable
            AccessCondition accessCondition = forceOverwrite ? null : AccessCondition.GenerateIfNotExistsCondition(); // If-None-Match *

            string blobName = DeriveBlobName(objectId);
            CloudBlobContainer objectsContainer = _objectsContainer.Value;
            CloudAppendBlob blobRef = objectsContainer.GetAppendBlobReference(blobName);

            try
            {
                // Required to create blob header before appending to it
                await blobRef.CreateOrReplaceAsync(accessCondition, default, default)
                    .ConfigureAwait(false);
            }
            // Try-catch is cheaper than a separate (latent) exists check
            catch (StorageException se) when (!forceOverwrite && se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                se.Suppress();
            }

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Append blob. Following seems to be the only safe multi-writer method available
                // http://stackoverflow.com/questions/32530126/azure-cloudappendblob-errors-with-concurrent-access
                await blobRef.AppendBlockAsync(fs)
                    .ConfigureAwait(false);
            }

            return false;
        }

        public static string DeriveBlobName(Sha1 sha1)
        {
            System.Collections.Generic.KeyValuePair<string, string> tokens = sha1.Split(2);

            string blobName = $@"{tokens.Key}/{tokens.Value}";
            blobName = Uri.EscapeUriString(blobName);

            return blobName;
        }

        #endregion
    }
}
