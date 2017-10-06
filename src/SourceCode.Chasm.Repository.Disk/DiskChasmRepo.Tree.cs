#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Disk
{
    partial class DiskChasmRepo // .Tree
    {
        #region Read (via TreeId)

        private static IReadOnlyDictionary<TreeId, TreeNodeList> DeserializeTreesImpl(IChasmSerializer serializer, IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>> kvps)
        {
            var dict = new Dictionary<TreeId, TreeNodeList>(kvps.Count);

            foreach (var kvp in kvps)
            {
                var tree = serializer.DeserializeTree(kvp.Value.Span);

                var treeId = new TreeId(kvp.Key);
                dict[treeId] = tree;
            }

            return dict;
        }

        public async ValueTask<TreeNodeList> ReadTreeAsync(TreeId treeId, CancellationToken cancellationToken)
        {
            if (treeId == TreeId.Empty) return default;

            // Read bytes
            var buffer = await ReadObjectAsync(treeId.Sha1, cancellationToken).ConfigureAwait(false);
            if (buffer.IsEmpty) return default;

            // Deserialize
            var tree = Serializer.DeserializeTree(buffer.Span);
            return tree;
        }

        public async ValueTask<IReadOnlyDictionary<TreeId, TreeNodeList>> ReadTreeBatchAsync(IEnumerable<TreeId> treeIds, ParallelOptions parallelOptions)
        {
            if (treeIds == null) return ReadOnlyDictionary.Empty<TreeId, TreeNodeList>();

            // Read bytes
            var sha1s = System.Linq.Enumerable.Select(treeIds, n => n.Sha1);
            var kvps = await ReadObjectBatchAsync(sha1s, parallelOptions).ConfigureAwait(false);

            // Deserialize
            var dict = DeserializeTreesImpl(Serializer, kvps);
            return dict;
        }

        #endregion

        #region Read (via CommitRef)

        public async ValueTask<TreeNodeList> ReadTreeAsync(string branch, string commitRefName, CancellationToken cancellationToken)
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

        #endregion

        #region Read (via CommitId)

        public async ValueTask<TreeNodeList> ReadTreeAsync(CommitId commitId, CancellationToken cancellationToken)
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

        #region Write (return TreeId)

        public async ValueTask<TreeId> WriteTreeAsync(TreeNodeList tree, CancellationToken cancellationToken)
        {
            using (var session = Serializer.Serialize(tree))
            {
                var sha1 = Sha1.Hash(session.Result);

                await WriteObjectAsync(sha1, session.Result, false, cancellationToken).ConfigureAwait(false);

                var model = new TreeId(sha1);
                return model;
            }
        }

        #endregion

        #region Write (return CommitId)

        public async ValueTask<CommitId> WriteTreeAsync(IReadOnlyList<CommitId> parents, TreeNodeList tree, DateTime commitUtc, string commitMessage, CancellationToken cancellationToken)
        {
            if (commitUtc.Kind != DateTimeKind.Utc) throw new ArgumentException(nameof(commitUtc));

            var treeId = TreeId.Empty;
            if (tree.Count > 0)
                treeId = await WriteTreeAsync(tree, cancellationToken).ConfigureAwait(false);

            var commit = new Commit(parents, treeId, commitUtc, commitMessage);
            var commitId = await WriteCommitAsync(commit, cancellationToken).ConfigureAwait(false);

            return commitId;
        }

        #endregion
    }
}
