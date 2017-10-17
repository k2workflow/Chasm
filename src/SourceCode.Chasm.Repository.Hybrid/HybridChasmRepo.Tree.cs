#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Collections.Generic;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Hybrid
{
    partial class HybridChasmRepo // .Tree
    {
        #region Read

        public override async ValueTask<IReadOnlyDictionary<TreeId, TreeNodeList>> ReadTreeBatchAsync(IEnumerable<TreeId> treeIds, CancellationToken cancellationToken)
        {
            if (treeIds == null) return ReadOnlyDictionary.Empty<TreeId, TreeNodeList>();

            // TODO: Enable piecemeal reads (404s incur next repo)

            // We read from closest to furthest
            for (var i = 0; i < Chain.Length; i++)
            {
                var dict = await Chain[i].ReadTreeBatchAsync(treeIds, cancellationToken).ConfigureAwait(false);

                if (!dict.Equals(default))
                    return dict;
            }

            // NotFound
            return default;
        }

        #endregion
    }
}
