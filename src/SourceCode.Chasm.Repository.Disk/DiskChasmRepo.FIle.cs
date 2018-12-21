using System;
using System.IO;
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
        /// The <paramref name="beforeWrite"/> function permits a transformation operation
        /// on the source value before calculating the hash and writing to the destination.
        /// For example, the source stream may be encoded as Json.
        /// The <paramref name="fileAction"/> function permits an operation to be
        /// performed on the file immediately after writing it. For example, the file
        /// may be uploaded to the cloud.
        /// </summary>
        /// <param name="beforeWrite">An action to take on the internal stream, before hashing and writing.</param>
        /// <param name="fileAction">An action to take on the file, after writing has finished.</param>
        /// <param name="deleteFile">Whether or not to delete the file after all actions are complete.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public static async Task<Sha1> WriteFileAsync(Func<Stream, Task> beforeWrite, Func<Sha1, string, Task> fileAction, bool deleteFile, CancellationToken cancellationToken)
        {
            if (beforeWrite == null) throw new ArgumentNullException(nameof(beforeWrite));

            // Note that an empty file is physically created
            var tempPath = Path.GetTempFileName();

            try
            {
                Sha1 sha1;
                using (var fs = new FileStream(tempPath, FileMode.Open, FileAccess.Write, FileShare.Read))
                using (var ct = crypt.SHA1.Create())
                {
                    using (var cs = new crypt.CryptoStream(fs, ct, crypt.CryptoStreamMode.Write, false))
                    {
                        await beforeWrite(cs)
                            .ConfigureAwait(false);
                    }

                    sha1 = new Sha1(ct.Hash);
                }

                if (fileAction != null)
                {
                    await fileAction(sha1, tempPath)
                        .ConfigureAwait(false);
                }

                return sha1;
            }
            finally
            {
                if (deleteFile
                    && File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        /// <summary>
        /// Writes a file to disk, returning the content's <see cref="Sha1"/> value.
        /// The <paramref name="fileAction"/> function permits an operation to be
        /// performed on the file immediately after writing it. For example, the file
        /// may be uploaded to the cloud.
        /// </summary>
        /// <param name="stream">The content to hash and write.</param>
        /// <param name="fileAction">An action to take on the file, after writing has finished.</param>
        /// <param name="deleteFile">Whether or not to delete the file after all actions are complete.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public static Task<Sha1> WriteFileAsync(Stream stream, Func<Sha1, string, Task> fileAction, bool deleteFile, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            Task FileAction(Stream inner)
                => stream.CopyToAsync(inner, cancellationToken);

            return WriteFileAsync(FileAction, fileAction, deleteFile, cancellationToken);
        }

        /// <summary>
        /// Writes a file to disk, returning the content's <see cref="Sha1"/> value.
        /// The <paramref name="fileAction"/> function permits an operation to be
        /// performed on the file immediately after writing it. For example, the file
        /// may be uploaded to the cloud.
        /// </summary>
        /// <param name="buffer">The content to hash and write.</param>
        /// <param name="fileAction">An action to take on the file, after writing has finished.</param>
        /// <param name="deleteFile">Whether or not to delete the file after all actions are complete.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public static Task<Sha1> WriteFileAsync(Memory<byte> buffer, Func<Sha1, string, Task> fileAction, bool deleteFile, CancellationToken cancellationToken)
        {
            Task FileAction(Stream inner)
                => inner.WriteAsync(buffer, cancellationToken).AsTask();

            return WriteFileAsync(FileAction, fileAction, deleteFile, cancellationToken);
        }

        private static async Task<byte[]> ReadFileAsync(string path, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                return default;

            if (!File.Exists(path))
                return default;

            using (FileStream fileStream = await WaitForFileAsync(path, FileMode.Open, FileAccess.Read, FileShare.Read, cancellationToken)
                .ConfigureAwait(false))
            {
                return await ReadFromStreamAsync(fileStream, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private static async Task<byte[]> ReadFromStreamAsync(Stream fileStream, CancellationToken cancellationToken)
        {
            int offset = 0;
            int remaining = (int)fileStream.Length;

            byte[] bytes = new byte[remaining];
            while (remaining > 0)
            {
                int count = await fileStream.ReadAsync(bytes, offset, remaining, cancellationToken)
                    .ConfigureAwait(false);

                if (count == 0)
                    throw new EndOfStreamException("End of file");

                offset += count;
                remaining -= count;
            }

            return bytes;
        }

        private static async Task TouchFileAsync(string path, CancellationToken cancellationToken)
        {
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

        private static async Task<FileStream> WaitForFileAsync(string path, FileMode mode, FileAccess access, FileShare share, CancellationToken cancellationToken, int bufferSize = 4096)
        {
            int retryCount = 0;
            while (true)
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(path, mode, access, share, bufferSize);
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

        private static string DeriveCommitRefFileName(string name, string branch)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (branch == null) return name;

            string refName = Path.Combine(name, $"{branch}{CommitExtension}");
            return refName;
        }

        public static string DeriveFileName(Sha1 sha1)
        {
            System.Collections.Generic.KeyValuePair<string, string> tokens = sha1.Split(PrefixLength);

            string fileName = Path.Combine(tokens.Key, tokens.Value);
            return fileName;
        }
    }
}
