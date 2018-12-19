using System;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository
{
    public static class ChasmRepositoryExtensions
    {
        public static ValueTask<Commit?> ReadCommitAsync(this IChasmRepository chasmRepository, CommitId? commitId, CancellationToken cancellationToken)
        {
            if (chasmRepository == null) throw new ArgumentNullException(nameof(chasmRepository));

            return commitId.HasValue
                ? chasmRepository.ReadCommitAsync(commitId.Value, cancellationToken)
                : default;
        }

        public static Task<ReadOnlyMemory<byte>?> ReadObjectAsync(this IChasmRepository chasmRepository, Sha1? objectId, CancellationToken cancellationToken)
        {
            if (chasmRepository == null) throw new ArgumentNullException(nameof(chasmRepository));

            return objectId.HasValue
                ? chasmRepository.ReadObjectAsync(objectId.Value, cancellationToken)
                : default;
        }

        public static Task<ReadOnlyMemory<byte>?> ReadObjectAsync(this IChasmRepository chasmRepository, BlobId? objectId, CancellationToken cancellationToken)
        {
            if (chasmRepository == null) throw new ArgumentNullException(nameof(chasmRepository));

            return objectId.HasValue
                ? chasmRepository.ReadObjectAsync(objectId.Value.Sha1, cancellationToken)
                : default;
        }

        public static ValueTask<TreeNodeMap?> ReadTreeAsync(this IChasmRepository chasmRepository, TreeId? treeId, CancellationToken cancellationToken)
        {
            if (chasmRepository == null) throw new ArgumentNullException(nameof(chasmRepository));

            return treeId.HasValue
                ? chasmRepository.ReadTreeAsync(treeId.Value, cancellationToken)
                : default;
        }
    }
}
