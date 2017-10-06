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

            var commitRef = Serializer.DeserializeCommitRef(bytes);
            return commitRef;
        }

        #endregion

        #region Write

        public async Task WriteCommitRefAsync(CommitId? previousCommitId, string branch, string name, CommitRef commitRef, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            // TODO: Optimistic concurrency

            var filename = DeriveCommitRefFileName(branch, name);
            var path = Path.Combine(_refsContainer, filename);

            using (var session = Serializer.Serialize(commitRef))
            {
                await WriteFileAsync(path, session.Result, cancellationToken).ConfigureAwait(false);
            }
        }

        #endregion
    }
}
