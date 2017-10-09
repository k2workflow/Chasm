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
    public static class ChasmUtil // .Tree
    {
        #region Read (via TreeId)

        public static async ValueTask<TreeNodeList> ReadTreeAsync(IChasmRepository chasmRepo, TreeId treeId, CancellationToken cancellationToken)
        {
            if (chasmRepo == null) throw new ArgumentNullException(nameof(chasmRepo));
            if (treeId == TreeId.Empty) return default;

            // Read bytes
            var buffer = await chasmRepo.ReadObjectAsync(treeId.Sha1, cancellationToken).ConfigureAwait(false);
            if (buffer.IsEmpty) return default;

            // Deserialize
            var tree = chasmRepo.Serializer.DeserializeTree(buffer.Span);
            return tree;
        }

        public static async ValueTask<IReadOnlyDictionary<TreeId, TreeNodeList>> ReadTreeBatchAsync(IChasmRepository chasmRepo, IEnumerable<TreeId> treeIds, ParallelOptions parallelOptions)
        {
            if (chasmRepo == null) throw new ArgumentNullException(nameof(chasmRepo));
            if (treeIds == null) return ReadOnlyDictionary.Empty<TreeId, TreeNodeList>();

            // Read bytes
            var sha1s = System.Linq.Enumerable.Select(treeIds, n => n.Sha1);
            var kvps = await chasmRepo.ReadObjectBatchAsync(sha1s, parallelOptions).ConfigureAwait(false);

            // Deserialize
            var dict = new Dictionary<TreeId, TreeNodeList>(kvps.Count);

            foreach (var kvp in kvps)
            {
                var tree = chasmRepo.Serializer.DeserializeTree(kvp.Value.Span);

                var treeId = new TreeId(kvp.Key);
                dict[treeId] = tree;
            }

            return dict;
        }

        #endregion

        #region Read (via CommitRef)

        public static async ValueTask<TreeNodeList> ReadTreeAsync(IChasmRepository chasmRepo, string branch, string commitRefName, CancellationToken cancellationToken)
        {
            if (chasmRepo == null) throw new ArgumentNullException(nameof(chasmRepo));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(commitRefName)) throw new ArgumentNullException(nameof(commitRefName));

            // CommitRef
            var commitRef = await chasmRepo.ReadCommitRefAsync(branch, commitRefName, cancellationToken).ConfigureAwait(false);

            // NotFound
            if (commitRef == null) return default;

            // Tree
            var tree = await chasmRepo.ReadTreeAsync(commitRef.Value.CommitId, cancellationToken).ConfigureAwait(false);
            return tree;
        }

        #endregion

        #region Read (via CommitId)

        public static async ValueTask<TreeNodeList> ReadTreeAsync(IChasmRepository chasmRepo, CommitId commitId, CancellationToken cancellationToken)
        {
            if (chasmRepo == null) throw new ArgumentNullException(nameof(chasmRepo));
            if (commitId == CommitId.Empty) return default;

            // Commit
            var commit = await chasmRepo.ReadCommitAsync(commitId, cancellationToken).ConfigureAwait(false);
            if (commit == Commit.Empty)
                return default;

            // Tree
            var tree = await chasmRepo.ReadTreeAsync(commit.TreeId, cancellationToken).ConfigureAwait(false);
            return tree;
        }

        #endregion

        #region Write (return TreeId)

        public static async ValueTask<TreeId> WriteTreeAsync(IChasmRepository chasmRepo, TreeNodeList tree, CancellationToken cancellationToken)
        {
            if (chasmRepo == null) throw new ArgumentNullException(nameof(chasmRepo));

            using (var session = chasmRepo.Serializer.Serialize(tree))
            {
                var sha1 = Sha1.Hash(session.Result);

                await chasmRepo.WriteObjectAsync(sha1, session.Result, false, cancellationToken).ConfigureAwait(false);

                var model = new TreeId(sha1);
                return model;
            }
        }

        #endregion

        #region Write (return CommitId)

        public static async ValueTask<CommitId> WriteTreeAsync(IChasmRepository chasmRepo, IReadOnlyList<CommitId> parents, TreeNodeList tree, DateTime commitUtc, string commitMessage, CancellationToken cancellationToken)
        {
            if (chasmRepo == null) throw new ArgumentNullException(nameof(chasmRepo));
            if (commitUtc.Kind != DateTimeKind.Utc) throw new ArgumentException(nameof(commitUtc));

            var treeId = TreeId.Empty;
            if (tree.Count > 0)
                treeId = await chasmRepo.WriteTreeAsync(tree, cancellationToken).ConfigureAwait(false);

            var commit = new Commit(parents, treeId, commitUtc, commitMessage);
            var commitId = await chasmRepo.WriteCommitAsync(commit, cancellationToken).ConfigureAwait(false);

            return commitId;
        }

        #endregion
    }
}
