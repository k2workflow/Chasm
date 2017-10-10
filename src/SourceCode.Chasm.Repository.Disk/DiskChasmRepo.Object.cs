#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Collections.Generic;
using SourceCode.Clay.Threading;
using System;
using System.Collections.Concurrent;
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

        public override async ValueTask<ReadOnlyMemory<byte>> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            var filename = DeriveFileName(objectId);
            var path = Path.Combine(_objectsContainer, filename);

            var bytes = await ReadFileAsync(path, cancellationToken).ConfigureAwait(false);
            if (bytes == null)
                return default;

            using (var input = new MemoryStream(bytes.ToArray())) // TODO: Perf
            using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
            using (var output = new MemoryStream())
            {
                input.Position = 0; // Else gzip returns []
                gzip.CopyTo(output);

                if (output.Length > 0)
                {
                    return output.ToArray(); // TODO: Perf
                }
            }

            return default;
        }

        public override async ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, ParallelOptions parallelOptions)
        {
            if (objectIds == null) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();

            ConcurrentDictionary<Sha1, ReadOnlyMemory<byte>> dict;
            if (objectIds is ICollection<Sha1> sha1s)
            {
                if (sha1s.Count == 0) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();
                dict = new ConcurrentDictionary<Sha1, ReadOnlyMemory<byte>>(4 * Environment.ProcessorCount, sha1s.Count);
            }
            else
            {
                if (!objectIds.Any()) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();
                dict = new ConcurrentDictionary<Sha1, ReadOnlyMemory<byte>>();
            }

            await ParallelAsync.ForEachAsync(objectIds, parallelOptions, async sha1 =>
            {
                var buffer = await ReadObjectAsync(sha1, parallelOptions.CancellationToken).ConfigureAwait(false);
                dict[sha1] = buffer;
            }).ConfigureAwait(false);

            return dict;
        }

        #endregion

        #region Write

        public override async Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> item, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var filename = DeriveFileName(objectId);
            var path = Path.Combine(_objectsContainer, filename);

            // Objects are immutable
            if (!File.Exists(path)
                && !forceOverwrite)
                return;

            using (var output = new MemoryStream())
            {
                using (var gz = new GZipStream(output, CompressionLevel, true))
                {
                    gz.Write(item.Array, item.Offset, item.Count);
                }
                output.Position = 0;

                await WriteFileAsync(path, output, cancellationToken, false).ConfigureAwait(false); // TODO: Perf
            }
        }

        public override Task WriteObjectBatchAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, ParallelOptions parallelOptions)
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
