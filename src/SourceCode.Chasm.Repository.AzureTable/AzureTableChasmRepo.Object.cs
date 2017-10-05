using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Clay;
using SourceCode.Clay.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.AzureTable
{
    partial class AzureTableChasmRepo // .Object
    {
        #region Read

        public async ValueTask<ReadOnlyMemory<byte>> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            var objectsTable = _objectsTable.Value;
            var op = DataEntity.BuildReadOperation(objectId);

            try
            {
                var result = await objectsTable.ExecuteAsync(op, default, default, cancellationToken).ConfigureAwait(false);

                if (result.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    return Array.Empty<byte>();

                var bytes = (byte[])result.Result;

                using (var input = new MemoryStream(bytes))
                using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
                using (var output = new MemoryStream())
                {
                    gzip.CopyTo(output);

                    var buffer = output.ToArray(); // TODO: Perf
                    return buffer;
                }
            }
            // Try-catch is cheaper than a separate exists check
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                se.Suppress();
            }

            return Array.Empty<byte>();
        }

        public async ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, ParallelOptions parallelOptions)
        {
            if (objectIds == null) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();

            var dict = new ConcurrentDictionary<Sha1, ReadOnlyMemory<byte>>();

            // Build batches
            var batches = DataEntity.BuildReadBatches(objectIds);

            // Execute batches
            var objectsTable = _objectsTable.Value;
            await AsyncParallelUtil.ForEachAsync(batches, parallelOptions, async batch =>
            {
                // Execute batch
                var results = await objectsTable.ExecuteBatchAsync(batch, default, default, parallelOptions.CancellationToken).ConfigureAwait(false);

                // Transform batch results
                foreach (var result in results)
                {
                    var entity = (DataEntity)result.Result;
                    var sha1 = DataEntity.FromPartition(entity);

                    dict[sha1] = entity.Content;
                }
            });

            return dict;
        }

        #endregion

        #region Write

        public async Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> segment, CancellationToken cancellationToken)
        {
            var objectsTable = _objectsTable.Value;

            using (var output = new MemoryStream())
            {
                using (var gz = new GZipStream(output, CompressionLevel, true))
                {
                    gz.Write(segment.Array, segment.Offset, segment.Count);
                }
                output.Position = 0;

                var seg = new ArraySegment<byte>(output.ToArray()); // TODO: Perf

                var op = DataEntity.BuildWriteOperation(objectId, seg);
                await objectsTable.ExecuteAsync(op, default, default, cancellationToken).ConfigureAwait(false);
            }
        }

        public Task WriteObjectBatchAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, ParallelOptions parallelOptions)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            // Build batches
            var batches = BuildWriteBatches(items, CompressionLevel, parallelOptions.CancellationToken);

            // Execute batches
            var objectsTable = _objectsTable.Value;
            return AsyncParallelUtil.ForEachAsync(batches, parallelOptions, async batch =>
            {
                // Execute batch
                await objectsTable.ExecuteBatchAsync(batch, null, null, parallelOptions.CancellationToken);
            });
        }

        private static IReadOnlyCollection<TableBatchOperation> BuildWriteBatches(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, CompressionLevel compressionLevel, CancellationToken cancellationToken)
        {
            var zipped = new Dictionary<Sha1, ArraySegment<byte>>();

            // Zip
            foreach (var item in items)
            {
                if (cancellationToken.IsCancellationRequested) break;

                using (var output = new MemoryStream())
                {
                    using (var gz = new GZipStream(output, compressionLevel, true))
                    {
                        var segment = item.Value;
                        gz.Write(segment.Array, segment.Offset, segment.Count);
                    }
                    output.Position = 0;

                    var seg = new ArraySegment<byte>(output.ToArray()); // TODO: Perf

                    zipped.Add(item.Key, seg);
                }
            }

            var batches = DataEntity.BuildWriteBatches(zipped);
            return batches;
        }

        #endregion
    }
}
