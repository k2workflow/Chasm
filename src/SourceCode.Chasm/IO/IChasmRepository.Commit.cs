using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial interface IChasmRepository // .Commit
    {
        #region Read

        Commit ReadCommit(CommitId commitId);

        ValueTask<Commit> ReadCommitAsync(CommitId commitId, CancellationToken cancellationToken);

        #endregion

        #region Write

        CommitId WriteCommit(Commit commit, bool forceOverwrite);

        ValueTask<CommitId> WriteCommitAsync(Commit commit, bool forceOverwrite, CancellationToken cancellationToken);

        #endregion
    }
}
