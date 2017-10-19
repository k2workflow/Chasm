#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Collections.Generic;
using SourceCode.Clay.Threading;
using System;
using System.Collections.Generic;
using System.IO.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Hybrid
{
    partial class HybridChasmRepo // .Object
    {
        #region Read

        public override async ValueTask<ReadOnlyMemory<byte>> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            // We read from closest to furthest
            for (var i = 0; i < Chain.Length; i++)
            {
                var bytes = await Chain[i].ReadObjectAsync(objectId, cancellationToken).ConfigureAwait(false);

                if (!bytes.Equals(default))
                    return bytes;
            }

            // NotFound
            return default;
        }

        public override async ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            if (objectIds == null) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();

            // TODO: Enable piecemeal reads (404s incur next repo)

            // We read from closest to furthest
            for (var i = 0; i < Chain.Length; i++)
            {
                var dict = await Chain[i].ReadObjectBatchAsync(objectIds, cancellationToken).ConfigureAwait(false);

                if (!dict.Equals(default))
                    return dict;
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
            if (items == null) return;

            // TODO: Enable piecemeal writes (404s incur next repo)

            // We write all at once
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDop,
                CancellationToken = cancellationToken
            };

            await ParallelAsync.ForAsync(0, Chain.Length, parallelOptions, async i =>
            {
                await Chain[i].WriteObjectBatchAsync(items, forceOverwrite, cancellationToken).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public override async Task WriteObjectBatchAsync(Channel<KeyValuePair<Sha1, ArraySegment<byte>>> channel, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var tasks = new Task[Chain.Length];
            var channels = new Channel<KeyValuePair<Sha1, ArraySegment<byte>>>[Chain.Length];

            for (var i = 0; i < Chain.Length; i++)
            {
                channels[i] = Channel.CreateUnbounded<KeyValuePair<Sha1, ArraySegment<byte>>>();
                tasks[i] = Chain[i].WriteObjectBatchAsync(channels[i], forceOverwrite, cancellationToken);
            }

            while (await channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                if (await channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false) &&
                    channel.Reader.TryRead(out var batch))
                {
                    for (var i = 0; i < Chain.Length; i++)
                    {
                        if (await channels[i].Writer.WaitToWriteAsync(cancellationToken).ConfigureAwait(false))
                        {
                            await channels[i].Writer.WriteAsync(batch, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        #endregion
    }
}
