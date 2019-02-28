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
            requestContext = ChasmRequestContext.Ensure(requestContext);

            string filename = DeriveCommitRefFileName(name, null);
            string path = Path.Combine(_refsContainer, filename);
            string[] files = Directory.GetFiles(path, "*" + CommitExtension, SearchOption.TopDirectoryOnly);

            var results = new CommitRef[files.Length];
            for (int i = 0; i < results.Length; i++)
            {
                byte[] bytes = await ReadFileAsync(files[i], requestContext, cancellationToken)
                    .ConfigureAwait(false);

                string branch = Path.ChangeExtension(Path.GetFileName(files[i]), null);

                CommitId commitId = Serializer.DeserializeCommitId(bytes);
                results[i] = new CommitRef(branch, commitId);
            }
            return results;
        }

        public override ValueTask<IReadOnlyList<string>> GetNamesAsync(ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(_refsContainer))
                return new ValueTask<IReadOnlyList<string>>(Array.Empty<string>());

            requestContext = ChasmRequestContext.Ensure(requestContext);

            string[] dirs = Directory.GetDirectories(_refsContainer);
            for (int i = 0; i < dirs.Length; i++)
                dirs[i] = Path.GetFileName(dirs[i]);

            return new ValueTask<IReadOnlyList<string>>(dirs);
        }

        public override async ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));

            requestContext = ChasmRequestContext.Ensure(requestContext);

            string filename = DeriveCommitRefFileName(name, branch);
            string path = Path.Combine(_refsContainer, filename);

            byte[] bytes = await ReadFileAsync(path, requestContext, cancellationToken)
                .ConfigureAwait(false);

            // NotFound
            if (bytes == null || bytes.Length == 0)
                return default;

            // CommitIds are not compressed
            CommitId commitId = Serializer.DeserializeCommitId(bytes);

            var commitRef = new CommitRef(branch, commitId);
            return commitRef;
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
            using (FileStream file = await WaitForFileAsync(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, cancellationToken)
                .ConfigureAwait(false))
            {
                if (file.Length == 0)
                {
                    if (previousCommitId.HasValue)
                        throw BuildConcurrencyException(name, commitRef.Branch, null, requestContext);
                }
                else
                {
                    // CommitIds are not compressed
                    byte[] bytes = await ReadBytesAsync(file, requestContext, cancellationToken)
                        .ConfigureAwait(false);

                    CommitId commitId = Serializer.DeserializeCommitId(bytes);

                    if (!previousCommitId.HasValue)
                        throw BuildConcurrencyException(name, commitRef.Branch, null, requestContext);

                    if (previousCommitId.Value != commitId)
                        throw BuildConcurrencyException(name, commitRef.Branch, null, requestContext);

                    file.Position = 0;
                }

#if !NETSTANDARD2_0
                await file.WriteAsync(owner.Memory, cancellationToken)
                    .ConfigureAwait(false);
#else
                byte[] array = owner.Memory.ToArray();
                await file.WriteAsync(array, 0, array.Length, cancellationToken)
                    .ConfigureAwait(false);
#endif

                if (file.Position != owner.Memory.Length)
                    file.Position = owner.Memory.Length;
            }

            await TouchFileAsync(path, requestContext, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
