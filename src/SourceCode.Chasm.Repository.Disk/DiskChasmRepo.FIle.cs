using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.Repository.Disk
{
    partial class DiskChasmRepo // .File
    {
        /// <summary>
        /// Writes a file to disk, returning the content's <see cref="Sha1"/> value.
        /// The <paramref name="onWrite"/> function permits a transformation operation
        /// on the source value before calculating the hash and writing to the destination.
        /// For example, the source stream may be encoded as Json.
        /// The <paramref name="afterWrite"/> function permits an operation to be
        /// performed on the file immediately after writing it. For example, the file
        /// may be uploaded to the cloud.
        /// </summary>
        /// <param name="onWrite">An action to take on the internal hashing stream.</param>
        /// <param name="afterWrite">An action to take on the file after writing has finished.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        /// <remarks>Note that the <paramref name="onWrite"/> function should maintain the integrity
        /// of the source stream: the hash will be taken on the result of this operation.
        /// For example, transforming to Json is appropriate but compression is not since the latter
        /// is not a representative model of the original content, but rather a storage optimization.</remarks>
        public static async Task<Sha1> StageFileAsync(Func<Stream, ValueTask> onWrite, Func<Sha1, string, ValueTask> afterWrite, CancellationToken cancellationToken)
        {
            if (onWrite == null) throw new ArgumentNullException(nameof(onWrite));

            // Note that an empty file is physically created
            var filePath = Path.GetTempFileName();

            try
            {
                Sha1 sha1;
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.Read))
                using (var ct = crypt.SHA1.Create())
                {
                    using (var cs = new crypt.CryptoStream(fs, ct, crypt.CryptoStreamMode.Write))
                    {
                        await onWrite(cs)
                            .ConfigureAwait(false);
                    }
                    sha1 = new Sha1(ct.Hash);
                }

                if (afterWrite != null)
                {
                    await afterWrite(sha1, filePath)
                        .ConfigureAwait(false);
                }

                return sha1;
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        /// <summary>
        /// Writes a file to disk, returning the content's <see cref="Sha1"/> value.
        /// The <paramref name="afterWrite"/> function permits an operation to be
        /// performed on the file immediately after writing it. For example, the file
        /// may be uploaded to the cloud.
        /// </summary>
        /// <param name="stream">The content to hash and write.</param>
        /// <param name="afterWrite">An action to take on the file, after writing has finished.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public static Task<Sha1> WriteFileAsync(Stream stream, Func<Sha1, string, ValueTask> afterWrite, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            ValueTask HashWriter(Stream output)
                => new ValueTask(stream.CopyToAsync(output, cancellationToken));

            return StageFileAsync(HashWriter, afterWrite, cancellationToken);
        }

        /// <summary>
        /// Writes a file to disk, returning the content's <see cref="Sha1"/> value.
        /// The <paramref name="afterWrite"/> function permits an operation to be
        /// performed on the file immediately after writing it. For example, the file
        /// may be uploaded to the cloud.
        /// </summary>
        /// <param name="buffer">The content to hash and write.</param>
        /// <param name="afterWrite">An action to take on the file, after writing has finished.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public static Task<Sha1> WriteFileAsync(ReadOnlyMemory<byte> buffer, Func<Sha1, string, ValueTask> afterWrite, CancellationToken cancellationToken)
        {
            ValueTask HashWriter(Stream output)
                => output.WriteAsync(buffer, cancellationToken);

            return StageFileAsync(HashWriter, afterWrite, cancellationToken);
        }

        private static async Task<IMemoryOwner<byte>> ReadFileAsync(string path, CancellationToken cancellationToken)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(path));

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                return default;

            if (!File.Exists(path))
                return default;

            using (FileStream fileStream = await WaitForFileAsync(path, FileMode.Open, FileAccess.Read, FileShare.Read, cancellationToken)
                .ConfigureAwait(false))
            {
                IMemoryOwner<byte> owned = await ReadBytesAsync(fileStream, cancellationToken)
                    .ConfigureAwait(false);

                return owned;
            }
        }

        private static async Task<IMemoryOwner<byte>> ReadBytesAsync(Stream stream, CancellationToken cancellationToken)
        {
            Debug.Assert(stream != null);

            int offset = 0;
            int remaining = (int)stream.Length;

            IMemoryOwner<byte> owned = MemoryPool<byte>.Shared.Rent(remaining);

            while (remaining > 0)
            {
                int count = await stream.ReadAsync(owned.Memory.Slice(offset, remaining), cancellationToken)
                    .ConfigureAwait(false);

                if (count == 0)
                    throw new EndOfStreamException("End of file");

                offset += count;
                remaining -= count;
            }

            return owned;
        }

        private static async Task TouchFileAsync(string path, CancellationToken cancellationToken)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(path));

            if (!File.Exists(path))
                return;

            for (int retryCount = 0; retryCount < _retryMax; retryCount++)
            {
                try
                {
                    File.SetLastAccessTimeUtc(path, DateTime.UtcNow);
                    break;
                }
                catch (IOException)
                {
                    await Task.Delay(_retryMs, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }

        private static async Task<FileStream> WaitForFileAsync(string path, FileMode mode, FileAccess access, FileShare share, CancellationToken cancellationToken)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(path));

            int retryCount = 0;
            while (true)
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(path, mode, access, share);
                    return fs;
                }
                catch (IOException) when (++retryCount < _retryMax)
                {
                    fs?.Dispose();
                    await Task.Delay(_retryMs, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }

        private static void WriteMetadata(ChasmMetadata metadata, string path)
        {
            var dto = new JsonMetadata(metadata?.ContentType, metadata?.Filename);
            var json = dto.ToJson();
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        private static ChasmMetadata ReadMetadata(string path)
        {
            if (!File.Exists(path))
                return null;

            string json = File.ReadAllText(path, Encoding.UTF8);
            var dto = JsonMetadata.FromJson(json);

            return new ChasmMetadata(dto.ContentType, dto.Filename);
        }

        private static string DeriveCommitRefFileName(string name, string branch)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(name));

            if (branch == null) return name;

            return Path.Combine(name, $"{branch}{CommitExtension}");
        }

        public static (string filePath, string metaPath) DeriveFileNames(string root, Sha1 sha1)
        {
            System.Collections.Generic.KeyValuePair<string, string> tokens = sha1.Split(PrefixLength);

            string fileName = Path.Combine(tokens.Key, tokens.Value);
            string filePath = Path.Combine(root, fileName);
            string metaPath = filePath + ".metadata";

            return (filePath, metaPath);
        }
    }
}
