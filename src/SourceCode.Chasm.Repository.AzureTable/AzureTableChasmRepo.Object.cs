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
using SourceCode.Clay.Threading;

namespace SourceCode.Chasm.Repository.AzureTable
{
    partial class AzureTableChasmRepo // .Object
    {
        public override async ValueTask<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            CloudTable objectsTable = _objectsTable.Value;
            TableOperation op = DataEntity.BuildReadOperation(objectId);

            try
            {
                TableResult result = await objectsTable.ExecuteAsync(op, default, default, cancellationToken).ConfigureAwait(false);

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

        public override async ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            if (objectIds == null) return ImmutableDictionary<Sha1, ReadOnlyMemory<byte>>.Empty;

            var dict = new ConcurrentDictionary<Sha1, ReadOnlyMemory<byte>>();

            // Build batches
            IReadOnlyCollection<TableBatchOperation> batches = DataEntity.BuildReadBatches(objectIds);

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDop,
                CancellationToken = cancellationToken
            };

            // Enumerate batches
            CloudTable objectsTable = _objectsTable.Value;
            await ParallelAsync.ForEachAsync(batches, parallelOptions, async batch =>
            {
                // Execute batch
                IList<TableResult> results = await objectsTable.ExecuteBatchAsync(batch, default, default, cancellationToken).ConfigureAwait(false);

                // Transform batch results
                foreach (TableResult result in results)
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
                        return;
                    }
                }
            }).ConfigureAwait(false);

            return dict;
        }

        private static IReadOnlyCollection<TableBatchOperation> BuildWriteBatches(IEnumerable<KeyValuePair<Sha1, Memory<byte>>> items, bool forceOverwrite, CompressionLevel compressionLevel, CancellationToken cancellationToken)
        {
            var zipped = new Dictionary<Sha1, Memory<byte>>();

            // Zip
            foreach (KeyValuePair<Sha1, Memory<byte>> item in items)
            {
                if (cancellationToken.IsCancellationRequested) break;

                using (var output = new MemoryStream())
                {
                    using (var gz = new GZipStream(output, compressionLevel, true))
                    {
                        gz.Write(item.Value.Span);
                    }
                    output.Position = 0;

                    var zip = new Memory<byte>(output.ToArray()); // TODO: Perf
                    zipped.Add(item.Key, zip);
                }
            }

            IReadOnlyCollection<TableBatchOperation> batches = DataEntity.BuildWriteBatches(zipped, forceOverwrite);
            return batches;
        }

        public override async Task WriteObjectAsync(Sha1 objectId, Memory<byte> item, bool forceOverwrite, CancellationToken cancellationToken)
        {
            CloudTable objectsTable = _objectsTable.Value;

            // TODO: Perf: Do not write if row already exists (objects are immutable)

            using (var output = new MemoryStream())
            {
                using (var gz = new GZipStream(output, CompressionLevel, true))
                {
                    gz.Write(item.Span);
                }
                output.Position = 0;

                var mem = new Memory<byte>(output.ToArray()); // TODO: Perf

                TableOperation op = DataEntity.BuildWriteOperation(objectId, mem, forceOverwrite);
                await objectsTable.ExecuteAsync(op, default, default, cancellationToken).ConfigureAwait(false);
            }
        }

        public override async Task WriteObjectBatchAsync(IEnumerable<KeyValuePair<Sha1, Memory<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (items == null || !items.Any()) return;

            // Build batches
            IReadOnlyCollection<TableBatchOperation> batches = BuildWriteBatches(items, forceOverwrite, CompressionLevel, cancellationToken);

            CloudTable objectsTable = _objectsTable.Value;

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDop,
                CancellationToken = cancellationToken
            };

            // Enumerate batches
            await ParallelAsync.ForEachAsync(batches, parallelOptions, async batch =>
            {
                // Execute batch
                await objectsTable.ExecuteBatchAsync(batch, null, null, cancellationToken).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
