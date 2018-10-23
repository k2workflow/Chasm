using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Repository
{
    partial interface IChasmRepository // .Commit
    {
        ValueTask<Commit?> ReadCommitAsync(CommitId commitId, CancellationToken cancellationToken);

        ValueTask<CommitId> WriteCommitAsync(Commit commit, CancellationToken cancellationToken);
    }
}
