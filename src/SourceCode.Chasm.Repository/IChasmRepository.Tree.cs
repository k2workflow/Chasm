using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Repository
{
    // Arguably some of these could be extension methods. But by including them on the interface, concrete
    // classes have the opportunity to optimize the IO operations directly in storage.
    public partial interface IChasmRepository // .Tree
    {
        ValueTask<TreeNodeMap?> ReadTreeAsync(TreeId treeId, ChasmRequestContext requestContext, CancellationToken cancellationToken);

        ValueTask<TreeNodeMap?> ReadTreeAsync(string branch, string commitRefName, ChasmRequestContext requestContext, CancellationToken cancellationToken);

        ValueTask<TreeNodeMap?> ReadTreeAsync(CommitId commitId, ChasmRequestContext requestContext, CancellationToken cancellationToken);

        ValueTask<IReadOnlyDictionary<TreeId, TreeNodeMap>> ReadTreeBatchAsync(IEnumerable<TreeId> treeIds, ChasmRequestContext requestContext, CancellationToken cancellationToken);

        ValueTask<TreeId> WriteTreeAsync(TreeNodeMap tree, ChasmRequestContext requestContext, CancellationToken cancellationToken);

        ValueTask<CommitId> WriteTreeAsync(IReadOnlyList<CommitId> parents, TreeNodeMap tree, Audit author, Audit committer, string message, ChasmRequestContext requestContext, CancellationToken cancellationToken);
    }
}
