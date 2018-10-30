using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Repository
{
    partial class ChasmRepository // .CommitRef
    {
        public abstract ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, CancellationToken cancellationToken);

        public abstract Task WriteCommitRefAsync(CommitId? previousCommitId, string name, CommitRef commitRef, CancellationToken cancellationToken);

        public abstract ValueTask<IReadOnlyList<string>> GetNamesAsync(CancellationToken cancellationToken);

        public abstract ValueTask<IReadOnlyList<CommitRef>> GetBranchesAsync(string name, CancellationToken cancellationToken);
    }
}
