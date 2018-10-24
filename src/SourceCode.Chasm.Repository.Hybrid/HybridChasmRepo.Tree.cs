using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Repository.Hybrid
{
    partial class HybridChasmRepo // .Tree
    {
        public override async ValueTask<IReadOnlyDictionary<TreeId, TreeNodeMap>> ReadTreeBatchAsync(IEnumerable<TreeId> treeIds, CancellationToken cancellationToken)
        {
            if (treeIds == null) return ImmutableDictionary<TreeId, TreeNodeMap>.Empty;

            // TODO: Enable piecemeal reads (404s incur next repo)

            // We read from closest to furthest
            TreeId[] trees = treeIds.ToArray();
            for (int i = 0; i < Chain.Length; i++)
            {
                IReadOnlyDictionary<TreeId, TreeNodeMap> dict = await Chain[i].ReadTreeBatchAsync(trees, cancellationToken).ConfigureAwait(false);
                if (dict.Count == trees.Length) return dict;
            }

            // NotFound
            return default;
        }
    }
}
