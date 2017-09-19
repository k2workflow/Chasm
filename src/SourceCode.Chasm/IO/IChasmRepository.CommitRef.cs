using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial interface IChasmRepository // .CommitRef
    {
        #region Read

        CommitId ReadCommitRef(string repo, string name);

        ValueTask<CommitId> ReadCommitRefAsync(string repo, string name, CancellationToken cancellationToken);

        #endregion

        #region Write

        void WriteCommitRef(string repo, string name, CommitId commitId, bool forceOverwrite);

        Task WriteCommitRefAsync(string repo, string name, CommitId commitId, bool forceOverwrite, CancellationToken cancellationToken);

        #endregion
    }
}
