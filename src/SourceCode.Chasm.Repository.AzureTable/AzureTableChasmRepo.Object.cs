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

        public override async Task<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
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
        /// The <paramref name="beforeHash"/> function permits a transformation operation
        /// on the source value before calculating the hash and writing to the destination.
        /// For example, the source stream may be encoded as Json.
        /// </summary>
        /// <param name="beforeHash">An action to take on the internal stream, before calculating the hash.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override Task<Sha1> WriteObjectAsync(Func<Stream, Task> beforeHash, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (beforeHash == null) throw new ArgumentNullException(nameof(beforeHash));

            Task Curry(Sha1 sha1, string tempPath)
                => UploadAsync(tempPath, sha1, forceOverwrite, cancellationToken);

            return DiskChasmRepo.WriteFileAsync(beforeHash, Curry, true, cancellationToken);
        }

        /// <summary>
        /// Writes a list of buffers to the destination, returning the content's <see cref="Sha1"/> value.
        /// </summary>
        /// <param name="buffers">The content to hash and write.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override async Task WriteObjectBatchAsync(IEnumerable<Memory<byte>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (items == null || !items.Any()) return;

            // Build batches
            IReadOnlyCollection<TableBatchOperation> batches = BuildWriteBatches(items, forceOverwrite, cancellationToken);

            CloudTable objectsTable = _objectsTable.Value;

            // Enumerate batches
            var tasks = new List<Task>();
            foreach (TableBatchOperation batch in batches)
            {
                Task<IList<TableResult>> task = objectsTable.ExecuteBatchAsync(batch, null, null, cancellationToken);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks)
                .ConfigureAwait(false);
        }

        private async Task UploadAsync(string tempPath, Sha1 sha1, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var bytes = await File.ReadAllBytesAsync(tempPath, cancellationToken)
                .ConfigureAwait(false);

            TableOperation op = DataEntity.BuildWriteOperation(sha1, bytes, forceOverwrite);

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
