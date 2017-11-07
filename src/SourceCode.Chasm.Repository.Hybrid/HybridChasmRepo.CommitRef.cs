#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Hybrid
{
    partial class HybridChasmRepo // .CommitRef
    {
        #region List

        public override ValueTask<IReadOnlyList<CommitRef>> GetBranchesAsync(string name, CancellationToken cancellationToken)
        {
            return Chain[0].GetBranchesAsync(name, cancellationToken);
        }

        public override ValueTask<IReadOnlyList<string>> GetNamesAsync(CancellationToken cancellationToken)
        {
            return Chain[0].GetNamesAsync(cancellationToken);
        }

        #endregion

        #region Read

        public override async ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            // We only read from last repo
            var last = Chain[Chain.Length - 1];
            var commitRef = await last.ReadCommitRefAsync(name, branch, cancellationToken).ConfigureAwait(false);

            return commitRef;
        }

        #endregion

        #region Write

        public override async Task WriteCommitRefAsync(CommitId? previousCommitId, string name, CommitRef commitRef, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (commitRef == CommitRef.Empty) throw new ArgumentNullException(nameof(commitRef));

            // We only write to last repo
            var last = Chain[Chain.Length - 1];
            await last.WriteCommitRefAsync(previousCommitId, name, commitRef, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}
