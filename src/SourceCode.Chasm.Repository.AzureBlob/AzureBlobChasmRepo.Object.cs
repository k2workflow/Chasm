using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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

        public override Task<Sha1> WriteObjectAsync(Memory<byte> buffer, bool forceOverwrite, CancellationToken cancellationToken)
            => WriteFileAsync(buffer, (sha1, tempPath) => Upload(sha1, tempPath, forceOverwrite, cancellationToken), true, cancellationToken);

        public override Task<Sha1> WriteObjectAsync(Stream stream, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            return WriteFileAsync(stream, (sha1, tempPath) => Upload(sha1, tempPath, forceOverwrite, cancellationToken), true, cancellationToken);
        }

        private async Task Upload(Sha1 sha1, string tempPath, bool forceOverwrite, CancellationToken cancellationToken)
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

        #endregion

        #region Helpers

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
