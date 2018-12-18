using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository.Hybrid
{
    partial class HybridChasmRepo // .Object
    {
        public override async Task<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            // We read from closest to furthest
            for (int i = 0; i < Chain.Count; i++)
            {
                ReadOnlyMemory<byte>? bytes = await Chain[i].ReadObjectAsync(objectId, cancellationToken)
                    .ConfigureAwait(false);

                if (bytes != null) return bytes;
            }

            // NotFound
            return default;
        }

        public override Task<Stream> ReadStreamAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override async Task<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            if (objectIds == null) return ImmutableDictionary<Sha1, ReadOnlyMemory<byte>>.Empty;

            // TODO: Perf

            // We read from closest to furthest
            Sha1[] objects = objectIds.ToArray();
            for (int i = 0; i < Chain.Count; i++)
            {
                IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>> dict = await Chain[i].ReadObjectBatchAsync(objects, cancellationToken).ConfigureAwait(false);
                if (dict.Count == objects.Length) return dict;
            }

            // NotFound
            return default;
        }

        public override async Task<Sha1> WriteObjectAsync(Memory<byte> item, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var tasks = new Task<Sha1>[Chain.Count];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Chain[i].WriteObjectAsync(item, forceOverwrite, cancellationToken);
            }

            await Task.WhenAll(tasks)
                .ConfigureAwait(false);

            return tasks[0].Result;
        }

        public override Task<Sha1> WriteObjectAsync(Stream stream, bool forceOverwrite, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override async Task WriteObjectBatchAsync(IEnumerable<Memory<byte>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (items == null || !items.Any()) return;

            // TODO: Enable piecemeal writes (404s incur next repo)

            var tasks = new Task[Chain.Count];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Chain[i].WriteObjectBatchAsync(items, forceOverwrite, cancellationToken);
            }

            await Task.WhenAll(tasks)
                .ConfigureAwait(false);
        }
    }
}
