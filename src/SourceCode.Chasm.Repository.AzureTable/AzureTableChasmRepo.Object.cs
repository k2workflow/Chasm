using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository.AzureTable
{
    partial class AzureTableChasmRepo // .Object
    {
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

                using (var input = new MemoryStream(entity.Content))
                using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
                using (var output = new MemoryStream())
                {
                    gzip.CopyTo(output);

                    byte[] buffer = output.ToArray(); // TODO: Perf
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

        public override Task<Stream> ReadStreamAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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

                    // Unzip
                    using (var input = new MemoryStream(entity.Content))
                    using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
                    using (var output = new MemoryStream())
                    {
                        gzip.CopyTo(output);

                        byte[] buffer = output.ToArray(); // TODO: Perf
                        dict[sha1] = buffer;
                    }
                }
            }

            return dict;
        }

        private IReadOnlyCollection<TableBatchOperation> BuildWriteBatches(IEnumerable<Memory<byte>> items, bool forceOverwrite, CompressionLevel compressionLevel, CancellationToken cancellationToken)
        {
            var zipped = new Dictionary<Sha1, Memory<byte>>();

            // Zip
            foreach (Memory<byte> item in items)
            {
                if (cancellationToken.IsCancellationRequested) break;

                Sha1 sha1 = Hasher.HashData(item.Span);
                using (var output = new MemoryStream())
                {
                    using (var gz = new GZipStream(output, compressionLevel, true))
                    {
                        gz.Write(item.Span);
                    }
                    output.Position = 0;

                    var zip = new Memory<byte>(output.ToArray()); // TODO: Perf
                    zipped.Add(sha1, zip);
                }
            }

            IReadOnlyCollection<TableBatchOperation> batches = DataEntity.BuildWriteBatches(zipped, forceOverwrite);
            return batches;
        }

        public override async Task<Sha1> WriteObjectAsync(Memory<byte> item, bool forceOverwrite, CancellationToken cancellationToken)
        {
            CloudTable objectsTable = _objectsTable.Value;

            // TODO: Perf: Do not write if row already exists (objects are immutable)

            Sha1 sha1 = Hasher.HashData(item.Span);
            using (var output = new MemoryStream())
            {
                using (var gz = new GZipStream(output, CompressionLevel, true))
                {
                    gz.Write(item.Span);
                }
                output.Position = 0;

                var mem = new Memory<byte>(output.ToArray()); // TODO: Perf

                TableOperation op = DataEntity.BuildWriteOperation(sha1, mem, forceOverwrite);
                await objectsTable.ExecuteAsync(op, default, default, cancellationToken)
                    .ConfigureAwait(false);
            }

            return sha1;
        }

        public override Task<Sha1> WriteObjectAsync(Stream stream, bool forceOverwrite, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override async Task WriteObjectBatchAsync(IEnumerable<Memory<byte>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (items == null || !items.Any()) return;

            // Build batches
            IReadOnlyCollection<TableBatchOperation> batches = BuildWriteBatches(items, forceOverwrite, CompressionLevel, cancellationToken);

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
    }
}
