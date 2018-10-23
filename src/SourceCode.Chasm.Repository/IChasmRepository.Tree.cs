using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Repository
{
    // Arguably some of these could be extension methods. But by including them on the interface, concrete
    // classes have the opportunity to optimize the IO operations directly in storage.
    public partial interface IChasmRepository // .Tree
    {
        ValueTask<TreeNodeMap?> ReadTreeAsync(TreeId treeId, CancellationToken cancellationToken);

        ValueTask<TreeNodeMap?> ReadTreeAsync(string branch, string commitRefName, CancellationToken cancellationToken);

        ValueTask<TreeNodeMap?> ReadTreeAsync(CommitId commitId, CancellationToken cancellationToken);

        ValueTask<IReadOnlyDictionary<TreeId, TreeNodeMap>> ReadTreeBatchAsync(IEnumerable<TreeId> treeIds, CancellationToken cancellationToken);

        ValueTask<TreeId> WriteTreeAsync(TreeNodeMap tree, CancellationToken cancellationToken);

        ValueTask<CommitId> WriteTreeAsync(IReadOnlyList<CommitId> parents, TreeNodeMap tree, Audit author, Audit committer, string message, CancellationToken cancellationToken);
    }
}
