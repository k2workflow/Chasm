#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    public static class ChasmRepositoryExtensions
    {
        #region Methods

        public static async ValueTask<TreeNodeMap?> ReadTreeAsync(this IChasmRepository chasmRepository, string branch, string commitRefName, CancellationToken cancellationToken)
        {
            if (chasmRepository == null) throw new ArgumentNullException(nameof(chasmRepository));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(commitRefName)) throw new ArgumentNullException(nameof(commitRefName));

            // CommitRef
            var commitRef = await chasmRepository.ReadCommitRefAsync(branch, commitRefName, cancellationToken).ConfigureAwait(false);

            // NotFound
            if (commitRef == null) return default;

            // Tree
            var tree = await chasmRepository.ReadTreeAsync(commitRef.Value.CommitId, cancellationToken).ConfigureAwait(false);
            return tree;
        }

        public static async ValueTask<TreeNodeMap?> ReadTreeAsync(this IChasmRepository chasmRepository, CommitId commitId, CancellationToken cancellationToken)
        {
            if (chasmRepository == null) throw new ArgumentNullException(nameof(chasmRepository));

            // Commit
            var commit = await chasmRepository.ReadCommitAsync(commitId, cancellationToken).ConfigureAwait(false);
            if (commit == null) return default;

            // Tree
            if (commit.Value.TreeId == null) return default;
            var tree = await chasmRepository.ReadTreeAsync(commit.Value.TreeId.Value, cancellationToken).ConfigureAwait(false);

            return tree;
        }

        public static async ValueTask<CommitId> WriteTreeAsync(this IChasmRepository chasmRepository, IReadOnlyList<CommitId> parents, TreeNodeMap tree, Audit author, Audit committer, string message, CancellationToken cancellationToken)
        {
            if (chasmRepository == null) throw new ArgumentNullException(nameof(chasmRepository));

            var treeId = await chasmRepository.WriteTreeAsync(tree, cancellationToken).ConfigureAwait(false);
            var commit = new Commit(parents, treeId, author, committer, message);
            var commitId = await chasmRepository.WriteCommitAsync(commit, cancellationToken).ConfigureAwait(false);

            return commitId;
        }

        #endregion
    }
}
