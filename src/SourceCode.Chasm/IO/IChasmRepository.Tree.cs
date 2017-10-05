using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial interface IChasmRepository // .Tree
    {
        #region Read

        ValueTask<TreeNodeList> ReadTreeAsync(TreeId treeId, CancellationToken cancellationToken);

        ValueTask<TreeNodeList> ReadTreeAsync(string branch, string commitRefName, CancellationToken cancellationToken);

        ValueTask<TreeNodeList> ReadTreeAsync(CommitId commitId, CancellationToken cancellationToken);

        ValueTask<IReadOnlyDictionary<TreeId, TreeNodeList>> ReadTreeBatchAsync(IEnumerable<TreeId> treeIds, ParallelOptions parallelOptions);

        #endregion

        #region Write

        ValueTask<TreeId> WriteTreeAsync(TreeNodeList tree, CancellationToken cancellationToken);

        ValueTask<CommitId> WriteTreeAsync(IReadOnlyList<CommitId> parents, TreeNodeList tree, DateTime commitUtc, string commitMessage, CancellationToken cancellationToken);

        #endregion
    }
}
