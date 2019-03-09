using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Repository.Disk
{
    partial class DiskChasmRepo // .CommitRef
    {
        private const string CommitExtension = ".commit";

        public override async ValueTask<IReadOnlyList<CommitRef>> GetBranchesAsync(string name, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            string filename = DeriveCommitRefFileName(name, null);
            string path = Path.Combine(_refsContainer, filename);

            string[] files = Directory.GetFiles(path, "*" + CommitExtension, SearchOption.TopDirectoryOnly);

            var results = new CommitRef[files.Length];

            for (int i = 0; i < results.Length; i++)
            {
                using (IMemoryOwner<byte> owned = await ReadFileAsync(files[i], cancellationToken)
                    .ConfigureAwait(false))
                {
                    string branch = Path.ChangeExtension(Path.GetFileName(files[i]), null);

                    CommitId commitId = Serializer.DeserializeCommitId(owned.Memory.Span);
                    results[i] = new CommitRef(branch, commitId);
                }
            }

            return results;
        }

        public override ValueTask<IReadOnlyList<string>> GetNamesAsync(ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(_refsContainer))
                return new ValueTask<IReadOnlyList<string>>(Array.Empty<string>());

            string[] dirs = Directory.GetDirectories(_refsContainer);
            for (int i = 0; i < dirs.Length; i++)
                dirs[i] = Path.GetFileName(dirs[i]);

            return new ValueTask<IReadOnlyList<string>>(dirs);
        }

        public override async ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));

            string filename = DeriveCommitRefFileName(name, branch);
            string path = Path.Combine(_refsContainer, filename);

            using (IMemoryOwner<byte> bytes = await ReadFileAsync(path, cancellationToken)
                .ConfigureAwait(false))
            {
                if (bytes == null || bytes.Memory.Length == 0)
                    return default;

                CommitId commitId = Serializer.DeserializeCommitId(bytes.Memory.Span);

                return new CommitRef(branch, commitId);
            }
        }

        public override async Task WriteCommitRefAsync(CommitId? previousCommitId, string name, CommitRef commitRef, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (commitRef == CommitRef.Empty) throw new ArgumentNullException(nameof(commitRef));

            requestContext = ChasmRequestContext.Ensure(requestContext);

            string filename = DeriveCommitRefFileName(name, commitRef.Branch);
            string path = Path.Combine(_refsContainer, filename);

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (IMemoryOwner<byte> owner = Serializer.Serialize(commitRef.CommitId))
            using (FileStream file = await WaitForFileAsync(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, cancellationToken: cancellationToken)
                .ConfigureAwait(false))
            {
                if (file.Length == 0)
                {
                    if (previousCommitId.HasValue)
                        throw BuildConcurrencyException(name, commitRef.Branch, null, requestContext);
                }
                else
                {
                    using (IMemoryOwner<byte> owned = await ReadBytesAsync(file, cancellationToken)
                         .ConfigureAwait(false))
                    {
                        CommitId commitId = Serializer.DeserializeCommitId(owned.Memory.Span);

                        if (!previousCommitId.HasValue)
                            throw BuildConcurrencyException(name, commitRef.Branch, null, requestContext);

                        if (previousCommitId.Value != commitId)
                            throw BuildConcurrencyException(name, commitRef.Branch, null, requestContext);

                        file.Position = 0;
                    }
                }

                await file.WriteAsync(owner.Memory, cancellationToken)
                    .ConfigureAwait(false);

                if (file.Position != owner.Memory.Length)
                    file.Position = owner.Memory.Length;
            }

            await TouchFileAsync(path, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
