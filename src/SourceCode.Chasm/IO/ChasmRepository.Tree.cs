#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial class ChasmRepository // .Tree
    {
        #region Read

        public virtual async ValueTask<TreeNodeList> ReadTreeAsync(TreeId treeId, CancellationToken cancellationToken)
        {
            if (treeId == TreeId.Empty) return default;

            // Read bytes
            var buffer = await ReadObjectAsync(treeId.Sha1, cancellationToken).ConfigureAwait(false);
            if (buffer.IsEmpty) return default;

            // Deserialize
            var tree = Serializer.DeserializeTree(buffer.Span);
            return tree;
        }

        public virtual async ValueTask<IReadOnlyDictionary<TreeId, TreeNodeList>> ReadTreeBatchAsync(IEnumerable<TreeId> treeIds, CancellationToken cancellationToken)
        {
            if (treeIds == null) return ReadOnlyDictionary.Empty<TreeId, TreeNodeList>();

            // Read bytes in batch
            var sha1s = System.Linq.Enumerable.Select(treeIds, n => n.Sha1);
            var kvps = await ReadObjectBatchAsync(sha1s, cancellationToken).ConfigureAwait(false);

            // Deserialize batch
            if (kvps.Count == 0) return ReadOnlyDictionary.Empty<TreeId, TreeNodeList>();

            var dict = new Dictionary<TreeId, TreeNodeList>(kvps.Count);

            foreach (var kvp in kvps)
            {
                var tree = Serializer.DeserializeTree(kvp.Value.Span);

                var treeId = new TreeId(kvp.Key);
                dict[treeId] = tree;
            }

            return dict;
        }

        public virtual async ValueTask<TreeNodeList> ReadTreeAsync(string branch, string commitRefName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(commitRefName)) throw new ArgumentNullException(nameof(commitRefName));

            // CommitRef
            var commitRef = await ReadCommitRefAsync(branch, commitRefName, cancellationToken).ConfigureAwait(false);

            // NotFound
            if (commitRef == null) return default;

            // Tree
            var tree = await ReadTreeAsync(commitRef.Value.CommitId, cancellationToken).ConfigureAwait(false);
            return tree;
        }

        public virtual async ValueTask<TreeNodeList> ReadTreeAsync(CommitId commitId, CancellationToken cancellationToken)
        {
            if (commitId == CommitId.Empty) return default;

            // Commit
            var commit = await ReadCommitAsync(commitId, cancellationToken).ConfigureAwait(false);
            if (commit == Commit.Empty)
                return default;

            // Tree
            var tree = await ReadTreeAsync(commit.TreeId, cancellationToken).ConfigureAwait(false);
            return tree;
        }

        #endregion

        #region Write

        public virtual async ValueTask<TreeId> WriteTreeAsync(TreeNodeList tree, CancellationToken cancellationToken)
        {
            using (var session = Serializer.Serialize(tree))
            {
                var sha1 = Sha1.Hash(session.Result);

                await WriteObjectAsync(sha1, session.Result, false, cancellationToken).ConfigureAwait(false);

                var model = new TreeId(sha1);
                return model;
            }
        }

        public virtual async ValueTask<CommitId> WriteTreeAsync(IReadOnlyList<CommitId> parents, TreeNodeList tree, Audit author, Audit committer, string message, CancellationToken cancellationToken)
        {
            var treeId = TreeId.Empty;
            if (tree.Count > 0)
                treeId = await WriteTreeAsync(tree, cancellationToken).ConfigureAwait(false);

            var commit = new Commit(parents, treeId, author, committer, message);
            var commitId = await WriteCommitAsync(commit, cancellationToken).ConfigureAwait(false);

            return commitId;
        }

        #endregion
    }
}
