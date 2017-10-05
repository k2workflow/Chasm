using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.AzureTable
{
    partial class AzureTableChasmRepo // .Commit
    {
        public async ValueTask<Commit> ReadCommitAsync(CommitId commitId, CancellationToken cancellationToken)
        {
            var buffer = await ReadObjectAsync(commitId.Sha1, cancellationToken).ConfigureAwait(false);
            if (buffer.IsEmpty)
                return Commit.Empty;

            var model = Serializer.DeserializeCommit(buffer.Span);
            return model;
        }

        public async ValueTask<CommitId> WriteCommitAsync(Commit commit, CancellationToken cancellationToken)
        {
            using (var session = Serializer.Serialize(commit))
            {
                var sha1 = Sha1.Hash(session.Result);

                await WriteObjectAsync(sha1, session.Result, cancellationToken).ConfigureAwait(false);

                var commitId = new CommitId(sha1);
                return commitId;
            }
        }
    }
}
