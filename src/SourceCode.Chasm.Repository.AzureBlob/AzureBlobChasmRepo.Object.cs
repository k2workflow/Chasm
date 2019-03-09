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
        private const string ContentTypeKey = "contentType";
        private const string FilenameKey = "filename";

        #region Read

        public override async Task<bool> ExistsAsync(Sha1 objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            requestContext = ChasmRequestContext.Ensure(requestContext);

            // Try disk repo first

            bool exists = await _diskRepo.ExistsAsync(objectId, requestContext, cancellationToken)
                .ConfigureAwait(false);

            // Else go to cloud

            if (!exists)
            {
                exists = await ExistsOnCloudAsync(objectId, requestContext, cancellationToken)
                      .ConfigureAwait(false);
            }

            return exists;
        }

        private async Task<bool> ExistsOnCloudAsync(Sha1 objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            requestContext = ChasmRequestContext.Ensure(requestContext);
            var opContext = new OperationContext
            {
                ClientRequestID = requestContext.CorrelationId,
                CustomUserAgent = requestContext.CustomUserAgent
            };

            string blobName = DeriveBlobName(objectId);
            CloudBlobContainer objectsContainer = _objectsContainer.Value;
            CloudAppendBlob blobRef = objectsContainer.GetAppendBlobReference(blobName);

            bool exists = await blobRef.ExistsAsync(default, opContext, cancellationToken)
                .ConfigureAwait(false);

            return exists;
        }

        public override async Task<IChasmBlob> ReadObjectAsync(Sha1 objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            requestContext = ChasmRequestContext.Ensure(requestContext);
            var opContext = new OperationContext
            {
                ClientRequestID = requestContext.CorrelationId,
                CustomUserAgent = requestContext.CustomUserAgent
            };

            // Try disk repo first

            IChasmBlob cached = await _diskRepo.ReadObjectAsync(objectId, requestContext, cancellationToken)
                .ConfigureAwait(false);

            if (cached != null)
                return cached;

            // Else go to cloud

            string blobName = DeriveBlobName(objectId);
            CloudBlobContainer objectsContainer = _objectsContainer.Value;
            CloudAppendBlob blobRef = objectsContainer.GetAppendBlobReference(blobName);

            try
            {
                // Fetch metadata
                await blobRef.FetchAttributesAsync(default, default, opContext, cancellationToken)
                    .ConfigureAwait(false);

                blobRef.Metadata.TryGetValue(FilenameKey, out string filename);
                blobRef.Metadata.TryGetValue(ContentTypeKey, out string contentType);
                var metadata = new ChasmMetadata(contentType, filename);

                using (var output = new MemoryStream())
                {
                    // TODO: Perf: Use a stream instead of a preceding call to fetch the buffer length
                    await blobRef.DownloadToStreamAsync(output, default, default, opContext, cancellationToken)
                        .ConfigureAwait(false);

                    byte[] buffer = output.ToArray(); // TODO: Perf

                    return new ChasmBlob(buffer, metadata);
                }
            }
            // Try-catch is cheaper than a separate (latent) exists check
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                se.Suppress();
                return default;
            }
        }

        public override async Task<IChasmStream> ReadStreamAsync(Sha1 objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            requestContext = ChasmRequestContext.Ensure(requestContext);
            var opContext = new OperationContext
            {
                ClientRequestID = requestContext.CorrelationId,
                CustomUserAgent = requestContext.CustomUserAgent
            };

            // Try disk repo first

            IChasmStream cached = await _diskRepo.ReadStreamAsync(objectId, requestContext, cancellationToken)
                .ConfigureAwait(false);

            if (cached != null)
                return cached;

            // Else go to cloud

            string blobName = DeriveBlobName(objectId);
            CloudBlobContainer objectsContainer = _objectsContainer.Value;
            CloudAppendBlob blobRef = objectsContainer.GetAppendBlobReference(blobName);

            try
            {
                // Fetch metadata
                await blobRef.FetchAttributesAsync()
                    .ConfigureAwait(false);

                blobRef.Metadata.TryGetValue(FilenameKey, out string filename);
                blobRef.Metadata.TryGetValue(ContentTypeKey, out string contentType);
                var metadata = new ChasmMetadata(contentType, filename);

                // TODO: Perf: Use a stream instead of a preceding call to fetch the buffer length
                var output = new MemoryStream();
                await blobRef.DownloadToStreamAsync(output, default, default, opContext, cancellationToken)
                    .ConfigureAwait(false);

                return new ChasmStream(output, metadata);
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
        public override async Task<WriteResult<Sha1>> WriteObjectAsync(ReadOnlyMemory<byte> buffer, ChasmMetadata metadata, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            requestContext = ChasmRequestContext.Ensure(requestContext);

            var created = true;

            async ValueTask AfterWrite(Sha1 sha1, string filePath)
            {
                created = await UploadAsync(sha1, filePath, metadata, forceOverwrite, requestContext, cancellationToken)
                    .ConfigureAwait(false);
            }

            Sha1 objectId = await DiskChasmRepo.WriteFileAsync(buffer, AfterWrite, cancellationToken)
                .ConfigureAwait(false);

            return new WriteResult<Sha1>(objectId, created);
        }

        /// <summary>
        /// Writes a stream to the destination, returning the content's <see cref="Sha1"/> value.
        /// </summary>
        /// <param name="stream">The content to hash and write.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override async Task<WriteResult<Sha1>> WriteObjectAsync(Stream stream, ChasmMetadata metadata, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            requestContext = ChasmRequestContext.Ensure(requestContext);

            var created = true;

            async ValueTask AfterWrite(Sha1 sha1, string filePath)
            {
                created = await UploadAsync(sha1, filePath, metadata, forceOverwrite, requestContext, cancellationToken)
                    .ConfigureAwait(false);
            }

            Sha1 objectId = await DiskChasmRepo.WriteFileAsync(stream, AfterWrite, cancellationToken)
                .ConfigureAwait(false);

            return new WriteResult<Sha1>(objectId, created);
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
        public override async Task<WriteResult<Sha1>> WriteObjectAsync(Func<Stream, ValueTask> beforeHash, ChasmMetadata metadata, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (beforeHash == null) throw new ArgumentNullException(nameof(beforeHash));

            requestContext = ChasmRequestContext.Ensure(requestContext);

            var created = true;

            async ValueTask AfterWrite(Sha1 sha1, string filePath)
            {
                created = await UploadAsync(sha1, filePath, metadata, forceOverwrite, requestContext, cancellationToken)
                    .ConfigureAwait(false);
            }

            Sha1 objectId = await DiskChasmRepo.StageFileAsync(beforeHash, AfterWrite, cancellationToken)
                .ConfigureAwait(false);

            return new WriteResult<Sha1>(objectId, created);
        }

        private async ValueTask<bool> UploadAsync(Sha1 objectId, string filePath, ChasmMetadata metadata, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            requestContext = ChasmRequestContext.Ensure(requestContext);
            var opContext = new OperationContext
            {
                ClientRequestID = requestContext.CorrelationId,
                CustomUserAgent = requestContext.CustomUserAgent
            };

            if (!forceOverwrite)
            {
                bool exists = await ExistsOnCloudAsync(objectId, requestContext, cancellationToken)
                    .ConfigureAwait(false);

                // Not created (already existed)
                if (exists)
                    return false;
            }

            // Objects are immutable
            AccessCondition accessCondition = forceOverwrite ? null : AccessCondition.GenerateIfNotExistsCondition(); // If-None-Match *

            string blobName = DeriveBlobName(objectId);
            CloudBlobContainer objectsContainer = _objectsContainer.Value;
            CloudAppendBlob blobRef = objectsContainer.GetAppendBlobReference(blobName);

            try
            {
                // Required to create blob header before appending to it
                await blobRef.CreateOrReplaceAsync(accessCondition, default, opContext, cancellationToken)
                    .ConfigureAwait(false);

                Task metadataTask = null;
                if (metadata != null)
                {
                    blobRef.Metadata.Add(FilenameKey, metadata.Filename);
                    blobRef.Metadata.Add(ContentTypeKey, metadata.ContentType);

                    metadataTask = blobRef.SetMetadataAsync(default, default, opContext, cancellationToken);
                }

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // Append blob. Following seems to be the only safe multi-writer method available
                    // http://stackoverflow.com/questions/32530126/azure-cloudappendblob-errors-with-concurrent-access
                    await blobRef.AppendBlockAsync(fs, default, default, default, opContext, cancellationToken)
                        .ConfigureAwait(false);

                    if (metadataTask != null)
                    {
                        await metadataTask
                            .ConfigureAwait(false);
                    }
                }
            }
            // Try-catch is cheaper than a separate (latent) exists check
            catch (StorageException se) when (!forceOverwrite && se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                se.Suppress();
            }

            // Created
            return true;
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
