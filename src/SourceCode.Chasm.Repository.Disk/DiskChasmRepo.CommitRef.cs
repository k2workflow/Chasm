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

        public async ValueTask<CommitRef?> ReadCommitRefAsync(string branch, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var filename = DeriveCommitRefFileName(branch, name);
            var path = Path.Combine(_refsContainer, filename);

            var bytes = await ReadFileAsync(path, cancellationToken).ConfigureAwait(false);

            // NotFound
            if (bytes == null || bytes.Length == 0)
                return null;

            // CommitIds are not compressed
            var commitId = Serializer.DeserializeCommitId(bytes);

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

            // CommitIds are not compressed
            using (var session = Serializer.Serialize(commitRef.CommitId))
            {
                await WriteFileAsync(path, session.Result, cancellationToken).ConfigureAwait(false);
            }
        }

        #endregion
    }
}
