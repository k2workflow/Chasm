using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Disk
{
    partial class DiskChasmRepo // .CommitRef
    {
        #region Read

        public ValueTask<CommitId> ReadCommitRefAsync(string branch, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var filename = DeriveCommitRefFileName(branch, name);
            var path = Path.Combine(_refsContainer.FullName, filename);

            var bytes = ReadFile(path);

            var sha1 = Serializer.DeserializeSha1(bytes);
            var commitId = new CommitId(sha1);

            return new ValueTask<CommitId>(commitId);
        }

        #endregion

        #region Write

        public Task WriteCommitRefAsync(CommitId? previousCommitId, string branch, string name, CommitId newCommitId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            // TODO: Optimistic concurrency

            var filename = DeriveCommitRefFileName(branch, name);
            var path = Path.Combine(_refsContainer.FullName, filename);

            using (var session = Serializer.Serialize(newCommitId.Sha1))
            {
                WriteFile(path, session.Result);
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
