#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Hybrid
{
    partial class HybridChasmRepo // .CommitRef
    {
        #region Read

        public override async ValueTask<CommitRef?> ReadCommitRefAsync(string branch, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            // We only read from last repo
            var last = Chain[Chain.Length - 1];
            var commitRef = await last.ReadCommitRefAsync(branch, name, cancellationToken).ConfigureAwait(false);

            return commitRef;
        }

        #endregion

        #region Write

        public override async Task WriteCommitRefAsync(CommitId? previousCommitId, string branch, CommitRef commitRef, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (commitRef == CommitRef.Empty) throw new ArgumentNullException(nameof(commitRef));

            // We only write to last repo
            var last = Chain[Chain.Length - 1];
            await last.WriteCommitRefAsync(previousCommitId, branch, commitRef, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}
