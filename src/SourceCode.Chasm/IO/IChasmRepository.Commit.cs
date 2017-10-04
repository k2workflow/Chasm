using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial interface IChasmRepository // .Commit
    {
        #region Read

        ValueTask<Commit> ReadCommitAsync(CommitId commitId, CancellationToken cancellationToken);

        #endregion

        #region Write

        ValueTask<CommitId> WriteCommitAsync(Commit commit, bool forceOverwrite, CancellationToken cancellationToken);

        #endregion
    }
}
