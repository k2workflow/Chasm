using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository
{
    partial class ChasmRepository // .Commit
    {
        public virtual async ValueTask<Commit?> ReadCommitAsync(CommitId commitId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            requestContext = ChasmRequestContext.Ensure(requestContext);

            using IChasmBlob blob = await ReadObjectAsync(commitId.Sha1, requestContext, cancellationToken)
                .ConfigureAwait(false);

            if (blob == null || blob.Content.IsEmpty)
                return default;

            Commit model = Serializer.DeserializeCommit(blob.Content.Span);
            return model;
        }

        public virtual async ValueTask<CommitId> WriteCommitAsync(Commit commit, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            requestContext = ChasmRequestContext.Ensure(requestContext);

            using (IMemoryOwner<byte> owner = Serializer.Serialize(commit))
            {
                Sha1 sha1 = await WriteObjectAsync(owner.Memory, null, false, requestContext, cancellationToken)
                    .ConfigureAwait(false);

                var commitId = new CommitId(sha1);
                return commitId;
            }
        }
    }
}
