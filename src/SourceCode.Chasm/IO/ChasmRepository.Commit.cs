using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial class ChasmRepository // .Commit
    {
        #region Read

        public Commit ReadCommit(CommitId commitId)
        {
            var buffer = ReadObject(commitId.Sha1);
            if (buffer.IsEmpty)
                return Commit.Empty;

            var model = Serializer.DeserializeCommit(buffer);
            return model;
        }

        public async ValueTask<Commit> ReadCommitAsync(CommitId commitId, CancellationToken cancellationToken)
        {
            var buffer = await ReadObjectAsync(commitId.Sha1, cancellationToken).ConfigureAwait(false);
            if (buffer.IsEmpty)
                return Commit.Empty;

            var model = Serializer.DeserializeCommit(buffer);
            return model;
        }

        #endregion

        #region Write

        public CommitId WriteCommit(Commit commit, bool forceOverwrite)
        {
            using (var session = Serializer.Serialize(commit))
            {
                var sha1 = Sha1.Hash(session.Result);

                WriteObject(sha1, session.Result, forceOverwrite);

                var commitId = new CommitId(sha1);
                return commitId;
            }
        }

        public async ValueTask<CommitId> WriteCommitAsync(Commit commit, bool forceOverwrite, CancellationToken cancellationToken)
        {
            using (var session = Serializer.Serialize(commit))
            {
                var sha1 = Sha1.Hash(session.Result);

                await WriteObjectAsync(sha1, session.Result, forceOverwrite, cancellationToken).ConfigureAwait(false);

                var commitId = new CommitId(sha1);
                return commitId;
            }
        }

        #endregion
    }
}
