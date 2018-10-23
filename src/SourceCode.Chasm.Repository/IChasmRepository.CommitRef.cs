using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Repository
{
    partial interface IChasmRepository // .CommitRef
    {
        ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, CancellationToken cancellationToken);

        /// <summary>
        /// Write a <see cref="CommitRef"/> to the repository using the provided values.
        /// Note that the underlying store should use pessimistic concurrency control to prevent data loss.
        /// </summary>
        /// <param name="name">The repository name.</param>
        /// <param name="previousCommitId">The previous <see cref="CommitId"/> that the caller used for reading.</param>
        /// <param name="commitRef">The new <see cref="CommitRef"/> that represents the content being written.</param>
        /// <param name="cancellationToken">Allows the <see cref="Task"/> to be cancelled.</param>
        /// <exception cref="ChasmConcurrencyException">Thrown when a concurrency exception is detected.</exception>
        /// <returns></returns>
        Task WriteCommitRefAsync(CommitId? previousCommitId, string name, CommitRef commitRef, CancellationToken cancellationToken);

        ValueTask<IReadOnlyList<string>> GetNamesAsync(CancellationToken cancellationToken);

        ValueTask<IReadOnlyList<CommitRef>> GetBranchesAsync(string name, CancellationToken cancellationToken);
    }
}
