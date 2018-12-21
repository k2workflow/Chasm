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

        public override async Task<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
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
            string blobName = DeriveBlobName(objectId);
            CloudBlobContainer objectsContainer = _objectsContainer.Value;
            CloudAppendBlob blobRef = objectsContainer.GetAppendBlobReference(blobName);

            try
            {
                var input = new MemoryStream();
                {
                    // TODO: Perf: Use a stream instead of a preceding call to fetch the buffer length
                    await blobRef.DownloadToStreamAsync(input)
                        .ConfigureAwait(false);

                    return input;
                }
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
        public override Task<Sha1> WriteObjectAsync(Memory<byte> buffer, bool forceOverwrite, CancellationToken cancellationToken)
        {
            Task Curry(Sha1 sha1, string tempPath)
                => UploadAsync(tempPath, sha1, forceOverwrite, cancellationToken);

            return DiskChasmRepo.WriteFileAsync(buffer, Curry, true, cancellationToken);
        }

        /// <summary>
        /// Writes a stream to the destination, returning the content's <see cref="Sha1"/> value.
        /// </summary>
        /// <param name="stream">The content to hash and write.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override Task<Sha1> WriteObjectAsync(Stream stream, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            Task Curry(Sha1 sha1, string tempPath)
                => UploadAsync(tempPath, sha1, forceOverwrite, cancellationToken);

            return DiskChasmRepo.WriteFileAsync(stream, Curry, true, cancellationToken);
        }

        /// <summary>
        /// Writes a stream to the destination, returning the content's <see cref="Sha1"/> value.
        /// The <paramref name="beforeWrite"/> function permits a transformation operation
        /// on the source value before calculating the hash and writing to the destination.
        /// For example, the source stream may be encoded as Json.
        /// </summary>
        /// <param name="beforeWrite">An action to take on the internal stream, before calculating the hash.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override Task<Sha1> WriteObjectAsync(Func<Stream, Task> beforeWrite, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (beforeWrite == null) throw new ArgumentNullException(nameof(beforeWrite));

            Task Curry(Sha1 sha1, string tempPath)
                => UploadAsync(tempPath, sha1, forceOverwrite, cancellationToken);

            return DiskChasmRepo.WriteFileAsync(beforeWrite, Curry, true, cancellationToken);
        }

        private async Task UploadAsync(string tempPath, Sha1 sha1, bool forceOverwrite, CancellationToken cancellationToken)
        {
            string blobName = DeriveBlobName(sha1);
            CloudBlobContainer objectsContainer = _objectsContainer.Value;
            CloudAppendBlob blobRef = objectsContainer.GetAppendBlobReference(blobName);

            // Objects are immutable
            AccessCondition accessCondition = forceOverwrite ? null : AccessCondition.GenerateIfNotExistsCondition(); // If-None-Match *

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

            using (var fs = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Append blob. Following seems to be the only safe multi-writer method available
                // http://stackoverflow.com/questions/32530126/azure-cloudappendblob-errors-with-concurrent-access
                await blobRef.AppendBlockAsync(fs)
                    .ConfigureAwait(false);
            }
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
