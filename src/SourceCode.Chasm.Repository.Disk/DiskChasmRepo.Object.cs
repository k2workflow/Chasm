using SourceCode.Clay.Collections.Generic;
using SourceCode.Clay.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Disk
{
    partial class DiskChasmRepo // .Object
    {
        #region Constants

        private const int _retryMax = 10;
        private const int _retryMs = 15;

        #endregion

        #region Read

        public ValueTask<ReadOnlyMemory<byte>> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            var filename = DeriveFileName(objectId);
            var path = Path.Combine(_refsContainer.FullName, filename);

            var bytes = ReadFile(path);

            using (var input = new MemoryStream(bytes.ToArray())) // TODO: Perf
            using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
            using (var output = new MemoryStream())
            {
                input.Position = 0; // Else gzip returns []
                gzip.CopyTo(output);

                if (output.Length > 0)
                {
                    return new ValueTask<ReadOnlyMemory<byte>>(output.ToArray()); // TODO: Perf
                }
            }

            return default;
        }

        public async ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, ParallelOptions parallelOptions)
        {
            if (objectIds == null) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();

            Dictionary<Sha1, ReadOnlyMemory<byte>> dict;
            if (objectIds is ICollection<Sha1> sha1s)
            {
                if (sha1s.Count == 0) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();
                dict = new Dictionary<Sha1, ReadOnlyMemory<byte>>(sha1s.Count);
            }
            else
            {
                if (!objectIds.Any()) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();
                dict = new Dictionary<Sha1, ReadOnlyMemory<byte>>();
            }

            // TODO: Parallelize
            foreach (var sha1 in objectIds)
            {
                var buffer = await ReadObjectAsync(sha1, parallelOptions.CancellationToken).ConfigureAwait(false);

                dict[sha1] = buffer;
            }

            return dict;
        }

        #endregion

        #region Write

        public Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> item, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var filename = DeriveFileName(objectId);
            var path = Path.Combine(_refsContainer.FullName, filename);

            // Objects are immutable
            if (!File.Exists(path)
                && !forceOverwrite)
                return Task.CompletedTask;

            using (var output = new MemoryStream())
            {
                using (var gz = new GZipStream(output, CompressionLevel, true))
                {
                    gz.Write(item.Array, item.Offset, item.Count);
                }
                output.Position = 0;

                WriteFile(path, output.ToArray()); // TODO: Perf
            }

            return Task.CompletedTask;
        }

        public Task WriteObjectBatchAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, ParallelOptions parallelOptions)
        {
            if (items == null) return Task.CompletedTask;

            return ParallelAsync.ForEachAsync(items, parallelOptions, async kvps =>
            {
                await WriteObjectAsync(kvps.Key, kvps.Value, forceOverwrite, parallelOptions.CancellationToken).ConfigureAwait(false);
            });
        }

        #endregion
    }
}
