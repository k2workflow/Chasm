using System;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository
{
    public static class ChasmRepositoryExtensions
    {
        public static Task<IChasmBlob> ReadObjectAsync(this IChasmRepository chasmRepository, Sha1? objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (chasmRepository == null) throw new ArgumentNullException(nameof(chasmRepository));

            requestContext = ChasmRequestContext.Ensure(requestContext);

            return objectId.HasValue
                ? chasmRepository.ReadObjectAsync(objectId.Value, requestContext, cancellationToken)
                : default;
        }

        public static Task<IChasmBlob> ReadObjectAsync(this IChasmRepository chasmRepository, BlobId? objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (chasmRepository == null) throw new ArgumentNullException(nameof(chasmRepository));

            requestContext = ChasmRequestContext.Ensure(requestContext);

            return objectId.HasValue
                ? chasmRepository.ReadObjectAsync(objectId.Value.Sha1, requestContext, cancellationToken)
                : default;
        }

        public static ValueTask<TreeNodeMap?> ReadTreeAsync(this IChasmRepository chasmRepository, TreeId? treeId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (chasmRepository == null) throw new ArgumentNullException(nameof(chasmRepository));

            requestContext = ChasmRequestContext.Ensure(requestContext);

            return treeId.HasValue
                ? chasmRepository.ReadTreeAsync(treeId.Value, requestContext, cancellationToken)
                : default;
        }
    }
}
