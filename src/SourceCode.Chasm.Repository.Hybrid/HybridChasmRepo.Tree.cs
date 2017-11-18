#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Hybrid
{
    partial class HybridChasmRepo // .Tree
    {
        #region Read

        public override async ValueTask<IReadOnlyDictionary<TreeMapId, TreeMap>> ReadTreeMapBatchAsync(IEnumerable<TreeMapId> treeMapIds, CancellationToken cancellationToken)
        {
            if (treeMapIds == null) return ReadOnlyDictionary.Empty<TreeMapId, TreeMap>();

            // TODO: Enable piecemeal reads (404s incur next repo)

            // We read from closest to furthest
            var trees = treeMapIds.ToArray();
            for (var i = 0; i < Chain.Length; i++)
            {
                var dict = await Chain[i].ReadTreeMapBatchAsync(trees, cancellationToken).ConfigureAwait(false);
                if (dict.Count == trees.Length) return dict;
            }

            // NotFound
            return default;
        }

        #endregion
    }
}
