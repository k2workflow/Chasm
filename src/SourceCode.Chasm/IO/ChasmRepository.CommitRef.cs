using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial class ChasmRepository // .CommitRef
    {
        #region Read

        public abstract CommitId ReadCommitRef(string repo, string name);

        public abstract ValueTask<CommitId> ReadCommitRefAsync(string repo, string name, CancellationToken cancellationToken);

        #endregion

        #region Write

        public abstract void WriteCommitRef(CommitId? previousCommitId, string repo, string name, CommitId newCommitId);

        public abstract Task WriteCommitRefAsync(CommitId? previousCommitId, string repo, string name, CommitId newCommitId, CancellationToken cancellationToken);

        #endregion
    }
}
