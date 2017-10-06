#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Disk
{
    partial class DiskChasmRepo // .CommitRef
    {
        #region Read

        public async ValueTask<CommitRef> ReadCommitRefAsync(string branch, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var filename = DeriveCommitRefFileName(branch, name);
            var path = Path.Combine(_refsContainer, filename);

            var bytes = await ReadFileAsync(path, cancellationToken).ConfigureAwait(false);

            // Sha1s are not compressed
            var sha1 = Serializer.DeserializeSha1(bytes);

            var commitId = new CommitId(sha1);
            var commitRef = new CommitRef(name, commitId);
            return commitRef;
        }

        #endregion

        #region Write

        public async Task WriteCommitRefAsync(CommitId? previousCommitId, string branch, CommitRef commitRef, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (commitRef == CommitRef.Empty) throw new ArgumentNullException(nameof(commitRef));

            // TODO: Optimistic concurrency

            var filename = DeriveCommitRefFileName(branch, commitRef.Name);
            var path = Path.Combine(_refsContainer, filename);

            // Sha1s are not compressed
            using (var session = Serializer.Serialize(commitRef.CommitId.Sha1))
            {
                await WriteFileAsync(path, session.Result, cancellationToken).ConfigureAwait(false);
            }
        }

        #endregion
    }
}
