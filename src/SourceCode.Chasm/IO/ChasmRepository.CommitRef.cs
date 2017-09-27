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

        public abstract void WriteCommitRef(string repo, string name, CommitId newCommitId, CommitId previousCommitId);

        public abstract Task WriteCommitRefAsync(string repo, string name, CommitId newCommitId, CommitId previousCommitId, CancellationToken cancellationToken);

        #endregion
    }
}
