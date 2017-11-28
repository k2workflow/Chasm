#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Hybrid
{
    partial class HybridChasmRepo // .Tree
    {
        #region Read

        public override async ValueTask<IReadOnlyDictionary<TreeId, TreeNodeMap>> ReadTreeBatchAsync(IEnumerable<TreeId> treeIds, CancellationToken cancellationToken)
        {
            if (treeIds == null) return ImmutableDictionary<TreeId, TreeNodeMap>.Empty;

            // TODO: Enable piecemeal reads (404s incur next repo)

            // We read from closest to furthest
            var trees = treeIds.ToArray();
            for (var i = 0; i < Chain.Length; i++)
            {
                var dict = await Chain[i].ReadTreeBatchAsync(trees, cancellationToken).ConfigureAwait(false);
                if (dict.Count == trees.Length) return dict;
            }

            // NotFound
            return default;
        }

        #endregion
    }
}
