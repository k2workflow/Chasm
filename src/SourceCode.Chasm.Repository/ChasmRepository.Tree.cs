using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;
using SourceCode.Clay.Collections.Generic;

namespace SourceCode.Chasm.Repository
{
    partial class ChasmRepository // .Tree
    {
        public virtual async ValueTask<TreeNodeMap?> ReadTreeAsync(TreeId treeId, CancellationToken cancellationToken)
        {
            // Read bytes
            IChasmBlob blob = await ReadObjectAsync(treeId.Sha1, cancellationToken)
                .ConfigureAwait(false);

            if (blob == null || blob.Content.IsEmpty)
                return default;

            // Deserialize
            TreeNodeMap tree = Serializer.DeserializeTree(blob.Content.Span);
            return tree;
        }

        public virtual async ValueTask<IReadOnlyDictionary<TreeId, TreeNodeMap>> ReadTreeBatchAsync(IEnumerable<TreeId> treeIds, CancellationToken cancellationToken)
        {
            if (treeIds == null)
                return EmptyMap<TreeId, TreeNodeMap>.Empty;

            // Read bytes in batch
            IEnumerable<Sha1> sha1s = System.Linq.Enumerable.Select(treeIds, n => n.Sha1);
            IReadOnlyDictionary<Sha1, IChasmBlob> kvps = await ReadObjectBatchAsync(sha1s, cancellationToken)
                .ConfigureAwait(false);

            // Deserialize batch
            if (kvps.Count == 0)
                return EmptyMap<TreeId, TreeNodeMap>.Empty;

            var dict = new Dictionary<TreeId, TreeNodeMap>(kvps.Count);

            foreach (KeyValuePair<Sha1, IChasmBlob> kvp in kvps)
            {
                TreeNodeMap tree = Serializer.DeserializeTree(kvp.Value.Content.Span);

                var treeId = new TreeId(kvp.Key);
                dict[treeId] = tree;
            }

            return dict;
        }

        public virtual async ValueTask<TreeNodeMap?> ReadTreeAsync(string branch, string commitRefName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(commitRefName)) throw new ArgumentNullException(nameof(commitRefName));

            // CommitRef
            CommitRef? commitRef = await ReadCommitRefAsync(branch, commitRefName, cancellationToken)
                .ConfigureAwait(false);

            // NotFound
            if (commitRef == null) return default;

            // Tree
            TreeNodeMap? tree = await ReadTreeAsync(commitRef.Value.CommitId, cancellationToken)
                .ConfigureAwait(false);

            return tree;
        }

        public virtual async ValueTask<TreeNodeMap?> ReadTreeAsync(CommitId commitId, CancellationToken cancellationToken)
        {
            // Commit
            Commit? commit = await ReadCommitAsync(commitId, cancellationToken)
                .ConfigureAwait(false);

            if (commit == null) return default;

            // Tree
            if (commit.Value.TreeId == null) return default;
            TreeNodeMap? tree = await ReadTreeAsync(commit.Value.TreeId.Value, cancellationToken)
                .ConfigureAwait(false);

            return tree;
        }

        public virtual async ValueTask<TreeId> WriteTreeAsync(TreeNodeMap tree, CancellationToken cancellationToken)
        {
            using (IMemoryOwner<byte> owner = Serializer.Serialize(tree))
            {
                Sha1 sha1 = await WriteObjectAsync(owner.Memory, null, false, cancellationToken)
                    .ConfigureAwait(false);

                var model = new TreeId(sha1);
                return model;
            }
        }

        public virtual async ValueTask<CommitId> WriteTreeAsync(IReadOnlyList<CommitId> parents, TreeNodeMap tree, Audit author, Audit committer, string message, CancellationToken cancellationToken)
        {
            TreeId treeId = await WriteTreeAsync(tree, cancellationToken)
                .ConfigureAwait(false);

            var commit = new Commit(parents, treeId, author, committer, message);
            CommitId commitId = await WriteCommitAsync(commit, cancellationToken)
                .ConfigureAwait(false);

            return commitId;
        }
    }
}
