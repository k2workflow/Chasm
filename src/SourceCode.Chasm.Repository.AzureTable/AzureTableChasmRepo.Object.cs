using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Chasm.Repository.Disk;
using SourceCode.Clay;

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

        public override async Task<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            // Try disk repo first

            ReadOnlyMemory<byte>? cached = await _diskRepo.ReadObjectAsync(objectId, cancellationToken)
                .ConfigureAwait(false);

            if (cached.HasValue)
                return cached.Value;

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
                return entity.Content; // TODO: Perf
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

            ReadOnlyMemory<byte>? memory = await ReadObjectAsync(objectId, cancellationToken)
                .ConfigureAwait(false);

            if (memory == null)
                return null;

            // TODO: Use a real stream from source if possible
            var stream = new MemoryStream(memory.Value.ToArray());
            return stream;
        }

        public override async Task<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            if (objectIds == null) return ImmutableDictionary<Sha1, ReadOnlyMemory<byte>>.Empty;

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
            var dict = new ConcurrentDictionary<Sha1, ReadOnlyMemory<byte>>();
            foreach (Task<IList<TableResult>> task in tasks)
            {
                foreach (TableResult result in task.Result)
                {
                    if (result.HttpStatusCode == (int)HttpStatusCode.NotFound)
                        continue;

                    var entity = (DataEntity)result.Result;
                    Sha1 sha1 = DataEntity.FromPartition(entity);

                    dict[sha1] = entity.Content; // TODO: Perf;
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
        public override Task<Sha1> WriteObjectAsync(Memory<byte> buffer, bool forceOverwrite, CancellationToken cancellationToken)
        {
            ValueTask UploadAsync(Sha1 objectId, string tempPath)
                => IdempotentUploadAsync(tempPath, objectId, forceOverwrite, cancellationToken);

            return DiskChasmRepo.WriteFileAsync(buffer, UploadAsync, cancellationToken);
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

            ValueTask UploadAsync(Sha1 objectId, string tempPath)
                => IdempotentUploadAsync(tempPath, objectId, forceOverwrite, cancellationToken);

            return DiskChasmRepo.WriteFileAsync(stream, UploadAsync, cancellationToken);
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
        public override Task<Sha1> WriteObjectAsync(Func<Stream, ValueTask> beforeHash, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (beforeHash == null) throw new ArgumentNullException(nameof(beforeHash));

            ValueTask UploadAsync(Sha1 objectId, string tempPath)
                => IdempotentUploadAsync(tempPath, objectId, forceOverwrite, cancellationToken);

            return DiskChasmRepo.WriteFileAsync(beforeHash, UploadAsync, cancellationToken);
        }

        /// <summary>
        /// Writes a list of buffers to the destination, returning the contents' <see cref="Sha1"/> values.
        /// </summary>
        /// <param name="buffers">The content to hash and write.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override async Task<IReadOnlyList<Sha1>> WriteObjectsAsync(IEnumerable<Memory<byte>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (items == null || !items.Any())
                return Array.Empty<Sha1>();

            // Build batches
            IReadOnlyCollection<TableBatchOperation> batches = BuildWriteBatches(items, forceOverwrite, cancellationToken);

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

            Sha1[] list = batches
                .Select(batch => batch)
                .SelectMany(op => op)
                .Select(op => DataEntity.FromPartition(op.Entity))
                .ToArray();

            return list;
        }

        private async ValueTask IdempotentUploadAsync(string tempPath, Sha1 objectId, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (!forceOverwrite)
            {
                bool exists = await ExistsOnCloudAsync(objectId, cancellationToken)
                    .ConfigureAwait(false);

                // Idempotent success (already exists)
                if (exists)
                    return;
            }

            var bytes = await File.ReadAllBytesAsync(tempPath, cancellationToken)
                .ConfigureAwait(false);

            TableOperation op = DataEntity.BuildWriteOperation(objectId, bytes, forceOverwrite);

            CloudTable objectsTable = _objectsTable.Value;
            await objectsTable.ExecuteAsync(op, default, default, cancellationToken)
                .ConfigureAwait(false);
        }

        private static IReadOnlyCollection<TableBatchOperation> BuildWriteBatches(IEnumerable<Memory<byte>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var dict = new Dictionary<Sha1, Memory<byte>>();

            foreach (Memory<byte> item in items)
            {
                if (cancellationToken.IsCancellationRequested) break;

                Sha1 sha1 = Hasher.HashData(item.Span);
                dict.Add(sha1, item);
            }

            IReadOnlyCollection<TableBatchOperation> batches = DataEntity.BuildWriteBatches(dict, forceOverwrite);
            return batches;
        }

        #endregion
    }
}
