using SourceCode.Clay.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.AzureBlob
{
    partial class AzureBlobChasmRepo // .Tree
    {
        #region Read (via TreeId)

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
            var dict = DeserializeTreesBatch(Serializer, kvps);
            return dict;
        }

        private static IReadOnlyDictionary<TreeId, TreeNodeList> DeserializeTreesBatch(IChasmSerializer serializer, IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>> kvps)
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

        #endregion

        #region Read (via CommitRef)

        public async ValueTask<TreeNodeList> ReadTreeAsync(string branch, string commitRefName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(commitRefName)) throw new ArgumentNullException(nameof(commitRefName));

            // CommitRef
            var commitId = await ReadCommitRefAsync(branch, commitRefName, cancellationToken).ConfigureAwait(false);
            if (commitId == CommitId.Empty) return default;

            // Tree
            var tree = await ReadTreeAsync(commitId, cancellationToken).ConfigureAwait(false);
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

                await WriteObjectAsync(sha1, session.Result, cancellationToken).ConfigureAwait(false);

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
