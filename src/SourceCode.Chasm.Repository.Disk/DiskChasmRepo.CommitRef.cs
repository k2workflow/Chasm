#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Disk
{
    partial class DiskChasmRepo // .CommitRef
    {
        #region Fields

        private const string CommitExtension = ".commit";

        #endregion

        #region List

        public override async ValueTask<IReadOnlyList<CommitRef>> GetBranchesAsync(string name, CancellationToken cancellationToken)
        {
            var filename = DeriveCommitRefFileName(name, null);
            var path = Path.Combine(_refsContainer, filename);
            var files = Directory.GetFiles(path, "*" + CommitExtension, SearchOption.TopDirectoryOnly);

            var results = new CommitRef[files.Length];
            for (var i = 0; i < results.Length; i++)
            {
                var bytes = await ReadFileAsync(files[i], cancellationToken).ConfigureAwait(false);

                var branch = Path.ChangeExtension(Path.GetFileName(files[i]), null);

                var commitId = Serializer.DeserializeCommitId(bytes);
                results[i] = new CommitRef(branch, commitId);
            }
            return results;
        }

        public override ValueTask<IReadOnlyList<string>> GetNamesAsync(CancellationToken cancellationToken)
        {
            if (!Directory.Exists(_refsContainer)) return new ValueTask<IReadOnlyList<string>>(Array.Empty<string>());

            var dirs = Directory.GetDirectories(_refsContainer);
            for (var i = 0; i < dirs.Length; i++)
                dirs[i] = Path.GetFileName(dirs[i]);

            return new ValueTask<IReadOnlyList<string>>(dirs);
        }

        #endregion

        #region Read

        public override async ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));

            var filename = DeriveCommitRefFileName(name, branch);
            var path = Path.Combine(_refsContainer, filename);

            var bytes = await ReadFileAsync(path, cancellationToken).ConfigureAwait(false);

            // NotFound
            if (bytes == null || bytes.Length == 0)
                return default;

            // CommitIds are not compressed
            var commitId = Serializer.DeserializeCommitId(bytes);

            var commitRef = new CommitRef(branch, commitId);
            return commitRef;
        }

        #endregion

        #region Write

        public override async Task WriteCommitRefAsync(CommitId? previousCommitId, string name, CommitRef commitRef, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (commitRef == CommitRef.Empty) throw new ArgumentNullException(nameof(commitRef));

            var filename = DeriveCommitRefFileName(name, commitRef.Branch);
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
                        throw BuildConcurrencyException(name, commitRef.Branch, null);
                }
                else
                {
                    // CommitIds are not compressed
                    var bytes = await ReadFromStreamAsync(file, cancellationToken).ConfigureAwait(false);
                    var commitId = Serializer.DeserializeCommitId(bytes);

                    if (!previousCommitId.HasValue)
                        throw BuildConcurrencyException(name, commitRef.Branch, null);

                    if (previousCommitId.Value != commitId)
                        throw BuildConcurrencyException(name, commitRef.Branch, null);

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
