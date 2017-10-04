using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Clay;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.AzureTableStorage
{
    partial class AzureTableChasmRepo // .Object
    {
        #region Read

        public override ReadOnlyMemory<byte> ReadObject(Sha1 objectId)
        {
            var objectsTable = _objectsTable.Value;
            var op = DataEntity.BuildReadOperation(objectId);

            try
            {
                using (var input = new MemoryStream())
                using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
                {
                    var result = objectsTable.ExecuteAsync(op).Result;

                    using (var output = new MemoryStream())
                    {
                        input.Position = 0; // Else gzip returns []
                        gzip.CopyTo(output);

                        var bytes = output.ToArray(); // TODO: Perf
                        return bytes;
                    }
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                // Try-catch is cheaper than a separate exists check
                se.Suppress();
            }

            return Array.Empty<byte>();
        }

        public override async ValueTask<ReadOnlyMemory<byte>> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            var objectsTable = _objectsTable.Value;
            var op = DataEntity.BuildReadOperation(objectId);

            try
            {
                using (var input = new MemoryStream())
                using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
                {
                    var result = await objectsTable.ExecuteAsync(op, new TableRequestOptions(), new OperationContext(), cancellationToken).ConfigureAwait(false);

                    using (var output = new MemoryStream())
                    {
                        input.Position = 0; // Else gzip returns []
                        gzip.CopyTo(output);

                        var bytes = output.ToArray(); // TODO: Perf
                        return bytes;
                    }
                }
            }
            // Try-catch is cheaper than a separate exists check
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                se.Suppress();
            }

            return Array.Empty<byte>();
        }

        #endregion

        #region Write

        public override void WriteObject(Sha1 objectId, ArraySegment<byte> segment, bool forceOverwrite)
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

                var op = DataEntity.BuildWriteOperation(objectId, seg, forceOverwrite);
                var result = objectsTable.ExecuteAsync(op).Result;
            }
        }

        public override async Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> segment, bool forceOverwrite, CancellationToken cancellationToken)
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

                var op = DataEntity.BuildWriteOperation(objectId, seg, forceOverwrite);
                await objectsTable.ExecuteAsync(op, new TableRequestOptions(), new OperationContext(), cancellationToken).ConfigureAwait(false);
            }
        }

        public override void WriteObjects(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (items == null) return;

            if (items is ICollection<KeyValuePair<Sha1, ArraySegment<byte>>> coll)
            {
                if (coll.Count == 0) return;

                // For small count, run non-concurrent
                if (coll.Count <= ConcurrentThreshold)
                {
                    foreach (var kvp in coll)
                    {
                        WriteObject(kvp.Key, kvp.Value, forceOverwrite);
                    }

                    return;
                }
            }

            var batches = BuildBatches(items, forceOverwrite, cancellationToken);

            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = MaxDop
            };

            var objectsTable = _objectsTable.Value;
            Parallel.ForEach(batches, options, batch =>
            {
                // Bad practice to use async within Parallel
                objectsTable.ExecuteBatchAsync(batch).Wait(cancellationToken);
            });
        }

        public override async Task WriteObjectsAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (items == null) return;

            if (items is ICollection<KeyValuePair<Sha1, ArraySegment<byte>>> coll)
            {
                if (coll.Count == 0) return;

                // For small count, run non-concurrent
                if (coll.Count <= ConcurrentThreshold)
                {
                    foreach (var kvp in coll)
                    {
                        await WriteObjectAsync(kvp.Key, kvp.Value, forceOverwrite, cancellationToken).ConfigureAwait(false);
                    }

                    return;
                }
            }

            var batches = BuildBatches(items, forceOverwrite, cancellationToken);

            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = MaxDop
            };

            var objectsTable = _objectsTable.Value;
            Parallel.ForEach(batches, options, batch =>
            {
                // Bad practice to use async within Parallel
                objectsTable.ExecuteBatchAsync(batch).Wait(cancellationToken);
            });
        }

        private IReadOnlyCollection<TableBatchOperation> BuildBatches(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var zipped = new Dictionary<Sha1, ArraySegment<byte>>();
            foreach (var item in items)
            {
                if (cancellationToken.IsCancellationRequested) break;

                using (var output = new MemoryStream())
                {
                    using (var gz = new GZipStream(output, CompressionLevel, true))
                    {
                        var segment = item.Value;
                        gz.Write(segment.Array, segment.Offset, segment.Count);
                    }

                    output.Position = 0;
                    var seg = new ArraySegment<byte>(output.ToArray()); // TODO: Perf

                    zipped.Add(item.Key, seg);
                }
            }

            var batches = new Dictionary<string, TableBatchOperation>(StringComparer.Ordinal);
            DataEntity.BuildBatchWriteOperation(batches, zipped, forceOverwrite);

            return batches.Values;
        }

        #endregion
    }
}
