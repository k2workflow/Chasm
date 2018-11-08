using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Repository.Hybrid
{
    partial class HybridChasmRepo // .CommitRef
    {
        public override ValueTask<IReadOnlyList<CommitRef>> GetBranchesAsync(string name, CancellationToken cancellationToken)
            => Chain[0].GetBranchesAsync(name, cancellationToken);

        public override ValueTask<IReadOnlyList<string>> GetNamesAsync(CancellationToken cancellationToken)
            => Chain[0].GetNamesAsync(cancellationToken);

        public override async ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            // We only read from last repo
            IChasmRepository last = Chain[Chain.Count - 1];
            CommitRef? commitRef = await last.ReadCommitRefAsync(name, branch, cancellationToken)
                .ConfigureAwait(false);

            return commitRef;
        }

        public override async Task WriteCommitRefAsync(CommitId? previousCommitId, string name, CommitRef commitRef, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (commitRef == CommitRef.Empty) throw new ArgumentNullException(nameof(commitRef));

            // We only write to last repo
            IChasmRepository last = Chain[Chain.Count - 1];
            await last.WriteCommitRefAsync(previousCommitId, name, commitRef, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
