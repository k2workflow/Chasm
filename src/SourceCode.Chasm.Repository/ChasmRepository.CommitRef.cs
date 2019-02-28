using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Repository
{
    partial class ChasmRepository // .CommitRef
    {
        public abstract ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default);

        public abstract Task WriteCommitRefAsync(CommitId? previousCommitId, string name, CommitRef commitRef, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default);

        public abstract ValueTask<IReadOnlyList<string>> GetNamesAsync(ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default);

        public abstract ValueTask<IReadOnlyList<CommitRef>> GetBranchesAsync(string name, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default);
    }
}
