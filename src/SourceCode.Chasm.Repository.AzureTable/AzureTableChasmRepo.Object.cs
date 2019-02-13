using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Chasm.Repository.Disk;
using SourceCode.Clay;
using SourceCode.Clay.Collections.Generic;

namespace SourceCode.Chasm.Repository.AzureTable
{
    partial class AzureTableChasmRepo // .Object
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
            CloudTable objectsTable = _objectsTable.Value;
            TableOperation op = DataEntity.BuildExistsOperation(objectId);

            try
            {
                TableResult result = await objectsTable.ExecuteAsync(op, default, default, cancellationToken)
                    .ConfigureAwait(false);

                if (result.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    return false;

                return true;
            }
            // Try-catch is cheaper than a separate (latent) exists check
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                se.Suppress();
                return default;
            }
        }

        public override async Task<IChasmBlob> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            // Try disk repo first

            IChasmBlob cached = await _diskRepo.ReadObjectAsync(objectId, cancellationToken)
                .ConfigureAwait(false);

            if (cached != null)
                return cached;

            // Else go to cloud

            CloudTable objectsTable = _objectsTable.Value;
            TableOperation op = DataEntity.BuildReadOperation(objectId);

            try
            {
                TableResult result = await objectsTable.ExecuteAsync(op, default, default, cancellationToken)
                    .ConfigureAwait(false);

                if (result.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    return default;

                var entity = (DataEntity)result.Result;
                var metadata = new Metadata(entity.Filename, entity.ContentType);

                return new ChasmBlob(entity.Content, metadata);
            }
            // Try-catch is cheaper than a separate (latent) exists check
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                se.Suppress();
                return default;
            }
        }

        public override async Task<IChasmStream> ReadStreamAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            // Try disk repo first

            IChasmStream cached = await _diskRepo.ReadStreamAsync(objectId, cancellationToken)
                .ConfigureAwait(false);

            if (cached != null)
                return cached;

            // Else go to cloud

            IChasmBlob blob = await ReadObjectAsync(objectId, cancellationToken)
                .ConfigureAwait(false);

            if (blob == null)
                return null;

            // TODO: Use a real stream from source if possible
            var stream = new MemoryStream(blob.Content.ToArray());

            return new ChasmStream(stream, blob.Metadata);
        }

        public override async Task<IReadOnlyDictionary<Sha1, IChasmBlob>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            if (objectIds == null)
                return EmptyMap<Sha1, IChasmBlob>.Empty;

            // Build batches
            IReadOnlyCollection<TableBatchOperation> batches = DataEntity.BuildReadBatches(objectIds);

            // Enumerate batches
            CloudTable objectsTable = _objectsTable.Value;
            var tasks = new List<Task<IList<TableResult>>>(batches.Count);
            foreach (TableBatchOperation batch in batches)
            {
                // Execute batch
                Task<IList<TableResult>> task = objectsTable.ExecuteBatchAsync(batch, default, default, cancellationToken);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks)
                .ConfigureAwait(false);

            // Transform batch results
            var dict = new ConcurrentDictionary<Sha1, IChasmBlob>();
            foreach (Task<IList<TableResult>> task in tasks)
            {
                foreach (TableResult result in task.Result)
                {
                    if (result.HttpStatusCode == (int)HttpStatusCode.NotFound)
                        continue;

                    var entity = (DataEntity)result.Result;
                    Sha1 sha1 = DataEntity.FromPartition(entity);
                    var metadata = new Metadata(entity.Filename, entity.ContentType);

                    dict[sha1] = new ChasmBlob(entity.Content, metadata);
                }
            }

            return dict;
        }

        #endregion

        #region Write

        /// <summary>
        /// Writes a buffer to the destination, returning the content's <see cref="Sha1"/> value.
        /// </summary>
        /// <param name="buffer">The content to hash and write.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override async Task<WriteResult<Sha1>> WriteObjectAsync(ReadOnlyMemory<byte> buffer, Metadata metadata, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var created = true;

            async ValueTask AfterWrite(Sha1 sha1, string filePath)
            {
                created = await UploadAsync(sha1, filePath, metadata, forceOverwrite, cancellationToken)
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
        public override async Task<WriteResult<Sha1>> WriteObjectAsync(Stream stream, Metadata metadata, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var created = true;

            async ValueTask AfterWrite(Sha1 sha1, string filePath)
            {
                created = await UploadAsync(sha1, filePath, metadata, forceOverwrite, cancellationToken)
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
        public override async Task<WriteResult<Sha1>> WriteObjectAsync(Func<Stream, ValueTask> beforeHash, Metadata metadata, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (beforeHash == null) throw new ArgumentNullException(nameof(beforeHash));

            var created = true;

            async ValueTask AfterWrite(Sha1 sha1, string filePath)
            {
                created = await UploadAsync(sha1, filePath, metadata, forceOverwrite, cancellationToken)
                    .ConfigureAwait(false);
            }

            Sha1 objectId = await DiskChasmRepo.StageFileAsync(beforeHash, AfterWrite, cancellationToken)
                .ConfigureAwait(false);

            return new WriteResult<Sha1>(objectId, created);
        }

        /// <summary>
        /// Writes a list of buffers to the destination, returning the contents' <see cref="Sha1"/> values.
        /// </summary>
        /// <param name="buffers">The content to hash and write.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override async Task<IReadOnlyList<WriteResult<Sha1>>> WriteObjectsAsync(IEnumerable<IChasmBlob> blobs, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (blobs == null || !blobs.Any())
                return Array.Empty<WriteResult<Sha1>>();

            // Build batches
            IReadOnlyCollection<TableBatchOperation> batches = BuildWriteBatches(blobs, forceOverwrite, cancellationToken);

            CloudTable objectsTable = _objectsTable.Value;

            // Enumerate batches
            var tasks = new List<Task<IList<TableResult>>>(batches.Count);
            foreach (TableBatchOperation batch in batches)
            {
                // Concurrency: instantiate tasks without await
                Task<IList<TableResult>> task = objectsTable.ExecuteBatchAsync(batch, null, null, cancellationToken);
                tasks.Add(task);
            }

            // Await the tasks
            await Task.WhenAll(tasks)
                .ConfigureAwait(false);

            WriteResult<Sha1>[] list = batches
                .Select(batch => batch)
                .SelectMany(op => op)
                .Select(op => DataEntity.FromPartition(op.Entity))
                .Select(e => new WriteResult<Sha1>(e, true)) // Assume created
                .ToArray();

            return list;
        }

        private async ValueTask<bool> UploadAsync(Sha1 objectId, string filePath, Metadata metadata, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (!forceOverwrite)
            {
                bool exists = await ExistsOnCloudAsync(objectId, cancellationToken)
                    .ConfigureAwait(false);

                // Not created (already existed)
                if (exists)
                    return false;
            }

            var bytes = File.ReadAllBytes(filePath);
            var blob = new ChasmBlob(bytes, metadata);

            TableOperation op = DataEntity.BuildWriteOperation(objectId, blob, forceOverwrite);

            CloudTable objectsTable = _objectsTable.Value;
            await objectsTable.ExecuteAsync(op, default, default, cancellationToken)
                .ConfigureAwait(false);

            // Created
            return true;
        }

        private static IReadOnlyCollection<TableBatchOperation> BuildWriteBatches(IEnumerable<IChasmBlob> blobs, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var dict = new Dictionary<Sha1, IChasmBlob>();

            foreach (IChasmBlob blob in blobs)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                Sha1 sha1 = Hasher.HashData(blob.Content.Span);
                dict.Add(sha1, blob);
            }

            IReadOnlyCollection<TableBatchOperation> batches = DataEntity.BuildWriteBatches(dict, forceOverwrite);
            return batches;
        }

        #endregion
    }
}
