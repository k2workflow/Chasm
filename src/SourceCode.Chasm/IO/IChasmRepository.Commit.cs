using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial interface IChasmRepository // .Commit
    {
        ValueTask<Commit> ReadCommitAsync(CommitId commitId, CancellationToken cancellationToken);

        ValueTask<CommitId> WriteCommitAsync(Commit commit, bool forceOverwrite, CancellationToken cancellationToken);
    }
}
