using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Clay;
using SourceCode.Clay.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Repository.AzureTable
{
    partial class AzureTableChasmRepo // .Object
    {
        #region Constants

        private const int ConcurrentThreshold = 3;

        #endregion

        #region Read

        public async ValueTask<ReadOnlyMemory<byte>> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            var objectsTable = _objectsTable.Value;
            var op = DataEntity.BuildReadOperation(objectId);

            try
            {
                var result = await objectsTable.ExecuteAsync(op, new TableRequestOptions(), new OperationContext(), cancellationToken).ConfigureAwait(false);

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

        public async ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectsAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            if (objectIds == null) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();

            if (objectIds is ICollection<Sha1> sha1s)
            {
                if (sha1s.Count == 0) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();

                // For small count, run non-concurrent
                if (sha1s.Count <= ConcurrentThreshold)
                {
                    var dict = new Dictionary<Sha1, ReadOnlyMemory<byte>>(sha1s.Count);

                    foreach (var sha1 in sha1s)
                    {
                        var buffer = await ReadObjectAsync(sha1, cancellationToken).ConfigureAwait(false);
                        dict[sha1] = buffer;
                    }

                    return dict;
                }
            }
            else if (!objectIds.Any())
            {
                return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();
            }

            // Run concurrent
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = MaxDop
            };

            var objectsTable = _objectsTable.Value;

            var result = ReadConcurrentImpl(objectsTable, objectIds, options);
            return result;
        }

        private static IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>> ReadConcurrentImpl(CloudTable objectsTable, IEnumerable<Sha1> objectIds, ParallelOptions options)
        {
            var dict = new ConcurrentDictionary<Sha1, ReadOnlyMemory<byte>>();

            Parallel.ForEach(objectIds, options, sha1 =>
            {
                var op = DataEntity.BuildReadOperation(sha1);

                try
                {
                    // Bad practice to use async within Parallel
                    var result = objectsTable.ExecuteAsync(op, new TableRequestOptions(), new OperationContext(), options.CancellationToken).Result;

                    if (result.HttpStatusCode == (int)HttpStatusCode.NotFound)
                        return;

                    var bytes = (byte[])result.Result;

                    using (var input = new MemoryStream(bytes))
                    using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
                    using (var output = new MemoryStream())
                    {
                        gzip.CopyTo(output);

                        var buffer = output.ToArray(); // TODO: Perf
                        dict[sha1] = buffer;
                        return;
                    }
                }
                catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                {
                    // Try-catch is cheaper than a separate exists check
                    se.Suppress();
                    dict[sha1] = Array.Empty<byte>(); // TODO: Is this sufficient. Maybe use null or NotFound?
                }
            });

            return dict;
        }

        #endregion

        #region Write

        public async Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> segment, bool forceOverwrite, CancellationToken cancellationToken)
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

        public async Task WriteObjectsAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken)
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

            var batches = BuildBatchesImpl(items, forceOverwrite, CompressionLevel, cancellationToken);

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

        private static IReadOnlyCollection<TableBatchOperation> BuildBatchesImpl(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CompressionLevel compressionLevel, CancellationToken cancellationToken)
        {
            var zipped = new Dictionary<Sha1, ArraySegment<byte>>();
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

            var batches = new Dictionary<string, TableBatchOperation>(StringComparer.Ordinal);
            DataEntity.BuildBatchWriteOperation(batches, zipped, forceOverwrite);

            return batches.Values;
        }

        #endregion
    }
}
