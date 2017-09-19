using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial interface IChasmRepository // .Tree
    {
        #region Read

        TreeNodeList ReadTree(TreeId treeId);

        ValueTask<TreeNodeList> ReadTreeAsync(TreeId treeId, CancellationToken cancellationToken);

        IReadOnlyDictionary<TreeId, TreeNodeList> ReadTrees(IEnumerable<TreeId> treeIds, CancellationToken cancellationToken);

        ValueTask<IReadOnlyDictionary<TreeId, TreeNodeList>> ReadTreesAsync(IEnumerable<TreeId> treeIds, CancellationToken cancellationToken);

        #endregion

        #region Write

        TreeId WriteTree(TreeNodeList tree, bool forceOverwrite);

        ValueTask<TreeId> WriteTreeAsync(TreeNodeList tree, bool forceOverwrite, CancellationToken cancellationToken);

        #endregion
    }
}
