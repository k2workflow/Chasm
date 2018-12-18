using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;
using SourceCode.Clay.Threading;

namespace SourceCode.Chasm.Repository
{
    partial class ChasmRepository // .Object
    {
        public abstract ValueTask<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken);

        public abstract Task<Stream> ReadStreamAsync(Sha1 objectId, CancellationToken cancellationToken);

        public virtual async ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            if (objectIds == null) return ImmutableDictionary<Sha1, ReadOnlyMemory<byte>>.Empty;

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDop,
                CancellationToken = cancellationToken
            };

            // Enumerate batches
            var dict = new ConcurrentDictionary<Sha1, ReadOnlyMemory<byte>>(Sha1Comparer.Default);
            await ParallelAsync.ForEachAsync(objectIds, parallelOptions, async sha1 =>
            {
                // Execute batch
                ReadOnlyMemory<byte>? buffer = await ReadObjectAsync(sha1, cancellationToken)
                    .ConfigureAwait(false);

                if (buffer == null || buffer.Value.Length == 0) return;

                dict[sha1] = buffer.Value;
            })
            .ConfigureAwait(false);

            return dict;
        }

        public abstract Task WriteObjectAsync(Sha1 objectId, Memory<byte> item, bool forceOverwrite, CancellationToken cancellationToken);

        public abstract ValueTask<Sha1> HashObjectAsync(Memory<byte> item, bool forceOverwrite, CancellationToken cancellationToken);

        public virtual async Task WriteObjectBatchAsync(IEnumerable<KeyValuePair<Sha1, Memory<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (items == null || !items.Any()) return;

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDop,
                CancellationToken = cancellationToken
            };

            // Enumerate batches
            await ParallelAsync.ForEachAsync(items, parallelOptions, async item =>
            {
                // Execute batch
                await WriteObjectAsync(item.Key, item.Value, forceOverwrite, cancellationToken)
                .ConfigureAwait(false);
            })
            .ConfigureAwait(false);
        }

        public abstract ValueTask<Sha1> HashObjectAsync(Stream item, bool forceOverwrite, CancellationToken cancellationToken);
    }
}
