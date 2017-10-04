using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial interface IChasmRepository // .Tree
    {
        #region Read

        ValueTask<TreeNodeList> ReadTreeAsync(TreeId treeId, CancellationToken cancellationToken);

        ValueTask<IReadOnlyDictionary<TreeId, TreeNodeList>> ReadTreesAsync(IEnumerable<TreeId> treeIds, CancellationToken cancellationToken);

        #endregion

        #region Write

        ValueTask<TreeId> WriteTreeAsync(TreeNodeList tree, bool forceOverwrite, CancellationToken cancellationToken);

        #endregion
    }
}
