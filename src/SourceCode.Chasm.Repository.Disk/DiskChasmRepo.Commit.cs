#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Disk
{
    partial class DiskChasmRepo // .Commit
    {
        #region Methods

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

                await WriteObjectAsync(sha1, session.Result, false, cancellationToken).ConfigureAwait(false);

                var commitId = new CommitId(sha1);
                return commitId;
            }
        }

        #endregion
    }
}
