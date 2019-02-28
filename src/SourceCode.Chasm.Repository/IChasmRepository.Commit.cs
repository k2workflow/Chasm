using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Repository
{
    partial interface IChasmRepository // .Commit
    {
        ValueTask<Commit?> ReadCommitAsync(CommitId commitId, ChasmRequestContext requestContext, CancellationToken cancellationToken);

        ValueTask<CommitId> WriteCommitAsync(Commit commit, ChasmRequestContext requestContext, CancellationToken cancellationToken);
    }
}
