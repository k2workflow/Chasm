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

        public override async ValueTask<CommitRef?> ReadCommitRefAsync(string branch, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var filename = DeriveCommitRefFileName(branch, name);
            var path = Path.Combine(_refsContainer, filename);

            var bytes = await ReadFileAsync(path, cancellationToken).ConfigureAwait(false);

            // NotFound
            if (bytes == null || bytes.Length == 0)
                return default;

            // CommitIds are not compressed
            var commitId = Serializer.DeserializeCommitId(bytes);

            var commitRef = new CommitRef(name, commitId);
            return commitRef;
        }

        #endregion

        #region Write

        public override async Task WriteCommitRefAsync(CommitId? previousCommitId, string branch, CommitRef commitRef, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (commitRef == CommitRef.Empty) throw new ArgumentNullException(nameof(commitRef));

            var filename = DeriveCommitRefFileName(branch, commitRef.Name);
            var path = Path.Combine(_refsContainer, filename);

            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var session = Serializer.Serialize(commitRef.CommitId))
            using (var file = await WaitForFileAsync(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, cancellationToken).ConfigureAwait(false))
            {
                if (file.Length == 0)
                {
                    if (previousCommitId.HasValue)
                        throw BuildConcurrencyException(branch, commitRef.Name, null);
                }
                else
                {
                    // CommitIds are not compressed
                    var bytes = await ReadFromStreamAsync(file, cancellationToken).ConfigureAwait(false);
                    var commitId = Serializer.DeserializeCommitId(bytes);

                    if (previousCommitId.Value != commitId)
                        throw BuildConcurrencyException(branch, commitRef.Name, null);

                    file.Position = 0;
                }

                // CommitIds are not compressed
                await file.WriteAsync(session.Result.Array, session.Result.Offset, session.Result.Count, cancellationToken).ConfigureAwait(false);

                if (file.Position != session.Result.Count)
                    file.Position = session.Result.Count;
            }

            await TouchFileAsync(path, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}
