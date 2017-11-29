#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay;
using SourceCode.Clay.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Hybrid
{
    partial class HybridChasmRepo // .Object
    {
        #region Read

        public override async ValueTask<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            // We read from closest to furthest
            for (var i = 0; i < Chain.Length; i++)
            {
                var bytes = await Chain[i].ReadObjectAsync(objectId, cancellationToken).ConfigureAwait(false);
                if (bytes != null) return bytes;
            }

            // NotFound
            return default;
        }

        public override async ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            if (objectIds == null) return ImmutableDictionary<Sha1, ReadOnlyMemory<byte>>.Empty;

            // TODO: Perf

            // We read from closest to furthest
            var objects = objectIds.ToArray();
            for (var i = 0; i < Chain.Length; i++)
            {
                var dict = await Chain[i].ReadObjectBatchAsync(objects, cancellationToken).ConfigureAwait(false);
                if (dict.Count == objects.Length) return dict;
            }

            // NotFound
            return default;
        }

        #endregion

        #region Write

        public override async Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> item, bool forceOverwrite, CancellationToken cancellationToken)
        {
            // We write all at once
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDop,
                CancellationToken = cancellationToken
            };

            await ParallelAsync.ForAsync(0, Chain.Length, parallelOptions, async i =>
            {
                await Chain[i].WriteObjectAsync(objectId, item, forceOverwrite, cancellationToken).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public override async Task WriteObjectBatchAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (items == null || !items.Any()) return;

            // TODO: Enable piecemeal writes (404s incur next repo)

            // We write all at once
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDop,
                CancellationToken = cancellationToken
            };

            // Enumerate batches
            await ParallelAsync.ForAsync(0, Chain.Length, parallelOptions, async i =>
            {
                // Execute batch
                await Chain[i].WriteObjectBatchAsync(items, forceOverwrite, cancellationToken).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        #endregion
    }
}
