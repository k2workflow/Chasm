using SourceCode.Clay.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial class ChasmRepository // .Tree
    {
        #region Read (via TreeId)

        public TreeNodeList ReadTree(TreeId treeId)
        {
            if (treeId == TreeId.Empty) return default;

            // Read bytes
            var buffer = ReadObject(treeId.Sha1);
            if (buffer.IsEmpty) return default;

            // Deserialize
            var tree = Serializer.DeserializeTree(buffer.Span);
            return tree;
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

        public virtual IReadOnlyDictionary<TreeId, TreeNodeList> ReadTrees(IEnumerable<TreeId> treeIds, CancellationToken cancellationToken)
        {
            if (treeIds == null) return ReadOnlyDictionary.Empty<TreeId, TreeNodeList>();

            // Read bytes
            var sha1s = System.Linq.Enumerable.Select(treeIds, n => n.Sha1);
            var kvps = ReadObjects(sha1s, cancellationToken);

            // Deserialize
            var dict = DeserializeTrees(kvps);
            return dict;
        }

        public virtual async ValueTask<IReadOnlyDictionary<TreeId, TreeNodeList>> ReadTreesAsync(IEnumerable<TreeId> treeIds, CancellationToken cancellationToken)
        {
            if (treeIds == null) return ReadOnlyDictionary.Empty<TreeId, TreeNodeList>();

            // Read bytes
            var sha1s = System.Linq.Enumerable.Select(treeIds, n => n.Sha1);
            var kvps = await ReadObjectsAsync(sha1s, cancellationToken).ConfigureAwait(false);

            // Deserialize
            var dict = DeserializeTrees(kvps);
            return dict;
        }

        private IReadOnlyDictionary<TreeId, TreeNodeList> DeserializeTrees(IReadOnlyDictionary<Sha1, System.ReadOnlyMemory<byte>> kvps)
        {
            var dict = new Dictionary<TreeId, TreeNodeList>(kvps.Count);

            foreach (var kvp in kvps)
            {
                var tree = Serializer.DeserializeTree(kvp.Value.Span);

                var treeId = new TreeId(kvp.Key);
                dict[treeId] = tree;
            }

            return dict;
        }

        #endregion

        #region Read (via CommitRef)

        public virtual TreeNodeList ReadTree(string branch, string commitRefName)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(commitRefName)) throw new ArgumentNullException(nameof(commitRefName));

            // CommitRef
            var commitId = ReadCommitRef(branch, commitRefName);
            if (commitId == CommitId.Empty) return default;

            // Tree
            var tree = ReadTree(commitId);
            return tree;
        }

        public virtual async ValueTask<TreeNodeList> ReadTreeAsync(string branch, string commitRefName, CancellationToken cancellationToken)
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

        public virtual TreeNodeList ReadTree(CommitId commitId)
        {
            if (commitId == CommitId.Empty) return default;

            // Commit
            var commit = ReadCommit(commitId);
            if (commit == Commit.Empty) return default;

            // Tree
            var tree = ReadTree(commit.TreeId);
            return tree;
        }

        public virtual async ValueTask<TreeNodeList> ReadTreeAsync(CommitId commitId, CancellationToken cancellationToken)
        {
            if (commitId == CommitId.Empty) return default;

            // Commit
            var commit = await ReadCommitAsync(commitId, cancellationToken).ConfigureAwait(false); ;
            if (commit == Commit.Empty)
                return default;

            // Tree
            var tree = await ReadTreeAsync(commit.TreeId, cancellationToken);
            return tree;
        }

        #endregion

        #region Write (return TreeId)

        public TreeId WriteTree(TreeNodeList tree, bool forceOverwrite)
        {
            using (var session = Serializer.Serialize(tree))
            {
                var sha1 = Sha1.Hash(session.Result);

                WriteObject(sha1, session.Result, forceOverwrite);

                var model = new TreeId(sha1);
                return model;
            }
        }

        public async ValueTask<TreeId> WriteTreeAsync(TreeNodeList tree, bool forceOverwrite, CancellationToken cancellationToken)
        {
            using (var session = Serializer.Serialize(tree))
            {
                var sha1 = Sha1.Hash(session.Result);

                await WriteObjectAsync(sha1, session.Result, forceOverwrite, cancellationToken).ConfigureAwait(false);

                var model = new TreeId(sha1);
                return model;
            }
        }

        #endregion

        #region Write (return CommitId)

        public virtual CommitId Write(IReadOnlyList<CommitId> parents, TreeNodeList tree, DateTime commitUtc, string commitMessage, bool forceOverwrite)
        {
            if (commitUtc.Kind != DateTimeKind.Utc) throw new ArgumentException(nameof(commitUtc));

            var treeId = TreeId.Empty;
            if (tree != null && tree.Count > 0)
                treeId = WriteTree(tree, forceOverwrite);

            var commit = new Commit(parents, treeId, commitUtc, commitMessage);
            var commitId = WriteCommit(commit, forceOverwrite);

            return commitId;
        }

        public virtual async ValueTask<CommitId> WriteAsync(IReadOnlyList<CommitId> parents, TreeNodeList tree, DateTime commitUtc, string commitMessage, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (commitUtc.Kind != DateTimeKind.Utc) throw new ArgumentException(nameof(commitUtc));

            var treeId = TreeId.Empty;
            if (tree != null && tree.Count > 0)
                treeId = await WriteTreeAsync(tree, forceOverwrite, cancellationToken).ConfigureAwait(false);

            var commit = new Commit(parents, treeId, commitUtc, commitMessage);
            var commitId = await WriteCommitAsync(commit, forceOverwrite, cancellationToken).ConfigureAwait(false);

            return commitId;
        }

        #endregion
    }
}
