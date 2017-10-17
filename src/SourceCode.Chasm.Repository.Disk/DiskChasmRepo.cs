#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Disk
{
    public sealed partial class DiskChasmRepo : ChasmRepository
    {
        #region Constants

        private const int _retryMax = 10;
        private const int _retryMs = 15;

        #endregion

        #region Fields

        private readonly string _refsContainer;
        private readonly string _objectsContainer;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the root path for the repository.
        /// </summary>
        public string RootPath { get; }

        #endregion

        #region Constructors

        public DiskChasmRepo(string rootFolder, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
            : base(serializer, compressionLevel, maxDop)
        {
            if (string.IsNullOrWhiteSpace(rootFolder) || rootFolder.Length <= 2) throw new ArgumentNullException(nameof(rootFolder)); // "C:\" is shortest permitted path
            var rootPath = Path.GetFullPath(rootFolder);

            RootPath = rootPath;

            // Root
            {
                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);
            }

            // Refs
            {
                const string container = "refs";
                var path = Path.Combine(rootPath, container);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                _refsContainer = path;
            }

            // Objects
            {
                const string container = "objects";
                var path = Path.Combine(rootPath, container);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                _objectsContainer = path;
            }
        }

        public DiskChasmRepo(string rootFolder, IChasmSerializer serializer, CompressionLevel compressionLevel)
          : this(rootFolder, serializer, compressionLevel, -1)
        { }

        public DiskChasmRepo(string rootFolder, IChasmSerializer serializer)
            : this(rootFolder, serializer, CompressionLevel.Optimal)
        { }

        #endregion

        #region Helpers

        private static async ValueTask<byte[]> ReadFileAsync(string path, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                return default;

            if (!File.Exists(path))
                return default;

            using (var fileStream = await WaitForFileAsync(path, FileMode.Open, FileAccess.Read, FileShare.Read, cancellationToken).ConfigureAwait(false))
            {
                return await ReadFromStreamAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }
        }

        private static async ValueTask<byte[]> ReadFromStreamAsync(Stream fileStream, CancellationToken cancellationToken)
        {
            var offset = 0;
            var remaining = (int)fileStream.Length;

            var bytes = new byte[remaining];
            while (remaining > 0)
            {
                var count = await fileStream.ReadAsync(bytes, offset, remaining, cancellationToken).ConfigureAwait(false);
                if (count == 0)
                    throw new EndOfStreamException("End of file");

                offset += count;
                remaining -= count;
            }

            return bytes;
        }

        private static async Task WriteFileAsync(string path, Stream data, CancellationToken cancellationToken, bool forceOverwrite)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var dataLength = data.Length - data.Position;
            if (dataLength <= 0) return;

            using (var fileStream = await WaitForFileAsync(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, cancellationToken).ConfigureAwait(false))
            {
                // Only write to the file if it does not already exist.
                if ((fileStream.Length != dataLength || forceOverwrite))
                {
                    fileStream.Position = 0;

                    await data.CopyToAsync(fileStream, 81920, cancellationToken).ConfigureAwait(false);

                    if (fileStream.Position != dataLength)
                        fileStream.SetLength(dataLength);
                }
            }

            await TouchFileAsync(path, cancellationToken).ConfigureAwait(false);
        }

        private static async Task TouchFileAsync(string path, CancellationToken cancellationToken)
        {
            if (!File.Exists(path))
                return;

            for (var retryCount = 0; retryCount < _retryMax; retryCount++)
            {
                try
                {
                    File.SetLastAccessTimeUtc(path, DateTime.UtcNow);
                    break;
                }
                catch (IOException)
                {
                    await Task.Delay(_retryMs, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static async ValueTask<FileStream> WaitForFileAsync(string path, FileMode mode, FileAccess access, FileShare share, CancellationToken cancellationToken, int bufferSize = 4096)
        {
            var retryCount = 0;
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
                    await Task.Delay(_retryMs, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string DeriveCommitRefFileName(string branch, string name)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refName = Path.Combine(branch, $"{name}.commit");
            return refName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DeriveFileName(Sha1 sha1)
        {
            var tokens = sha1.Split(2);

            var fileName = Path.Combine(tokens.Key, tokens.Value);
            return fileName;
        }

        #endregion
    }
}
