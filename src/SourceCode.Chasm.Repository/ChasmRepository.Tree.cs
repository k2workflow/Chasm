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
        public virtual async ValueTask<TreeNodeMap?> ReadTreeAsync(TreeId treeId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            requestContext = ChasmRequestContext.Ensure(requestContext);

            // Read bytes
            IChasmBlob blob = await ReadObjectAsync(treeId.Sha1, requestContext, cancellationToken)
                .ConfigureAwait(false);

            if (blob == null || blob.Content.IsEmpty)
                return default;

            // Deserialize
            TreeNodeMap tree = Serializer.DeserializeTree(blob.Content.Span);
            return tree;
        }

        public virtual async ValueTask<IReadOnlyDictionary<TreeId, TreeNodeMap>> ReadTreeBatchAsync(IEnumerable<TreeId> treeIds, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (treeIds == null)
                return EmptyDictionary.Empty<TreeId, TreeNodeMap>();

            requestContext = ChasmRequestContext.Ensure(requestContext);

            // Read bytes in batch
            IEnumerable<Sha1> sha1s = System.Linq.Enumerable.Select(treeIds, n => n.Sha1);
            IReadOnlyDictionary<Sha1, IChasmBlob> kvps = await ReadObjectBatchAsync(sha1s, requestContext, cancellationToken)
                .ConfigureAwait(false);

            // Deserialize batch
            if (kvps.Count == 0)
                return EmptyDictionary.Empty<TreeId, TreeNodeMap>();

            var dict = new Dictionary<TreeId, TreeNodeMap>(kvps.Count);

            foreach (KeyValuePair<Sha1, IChasmBlob> kvp in kvps)
            {
                TreeNodeMap tree = Serializer.DeserializeTree(kvp.Value.Content.Span);

                var treeId = new TreeId(kvp.Key);
                dict[treeId] = tree;
            }

            return dict;
        }

        public virtual async ValueTask<TreeId> WriteTreeAsync(TreeNodeMap tree, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            using (IMemoryOwner<byte> owner = Serializer.Serialize(tree))
            {
                Sha1 sha1 = await WriteObjectAsync(owner.Memory, null, false, requestContext, cancellationToken)
                    .ConfigureAwait(false);

                var model = new TreeId(sha1);
                return model;
            }
        }
    }
}
