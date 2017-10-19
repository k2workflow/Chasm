#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Collections.Generic;
using SourceCode.Clay.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial class ChasmRepository // .Object
    {
        #region Read

        public abstract ValueTask<ReadOnlyMemory<byte>> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken);

        public virtual async ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            if (objectIds == null) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDop,
                CancellationToken = cancellationToken
            };

            // Enumerate batches
            var dict = await ParallelAsync.ForEachAsync(objectIds, parallelOptions, async sha1 =>
            {
                // Execute batch
                var buffer = await ReadObjectAsync(sha1, cancellationToken).ConfigureAwait(false);

                // Transform batch result
                var kvp = new KeyValuePair<Sha1, ReadOnlyMemory<byte>>(sha1, buffer);
                return kvp;
            }).ConfigureAwait(false);

            return dict;
        }

        #endregion

        #region Write

        public abstract Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> item, bool forceOverwrite, CancellationToken cancellationToken);

        public virtual async Task WriteObjectBatchAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken)
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
                await WriteObjectAsync(item.Key, item.Value, forceOverwrite, cancellationToken).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        #endregion
    }
}
