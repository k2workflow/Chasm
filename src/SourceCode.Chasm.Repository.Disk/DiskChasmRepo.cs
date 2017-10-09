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
        #region Fields

        private readonly string _refsContainer;
        private readonly string _objectsContainer;

        #endregion

        #region Constructors

        public DiskChasmRepo(string rootFolder, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
            : base(serializer, compressionLevel, maxDop)
        {
            if (string.IsNullOrWhiteSpace(rootFolder) || rootFolder.Length <= 2) throw new ArgumentNullException(nameof(rootFolder)); // "C:\" is shortest permitted path
            if (rootFolder[rootFolder.Length - 1] != Path.DirectorySeparatorChar) throw new ArgumentException("Path must end with " + Path.DirectorySeparatorChar, nameof(rootFolder));
            var rootPath = Path.GetFullPath(rootFolder);
            if (rootPath != rootFolder) throw new ArgumentException("Path must be fully qualified", nameof(rootFolder));

            // Root
            {
                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);
            }

            // Refs
            {
                const string container = "refs";
                var path = Path.Combine(rootPath, container) + Path.DirectorySeparatorChar;

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                _refsContainer = path;
            }

            // Objects
            {
                const string container = "objects";
                var path = Path.Combine(rootPath, container) + Path.DirectorySeparatorChar;

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

            using (var fileStream = WaitForFile(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (fileStream == null)
                    return default;

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
        }

        private static async Task WriteFileAsync(string path, ArraySegment<byte> segment, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var fileStream = WaitForFile(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await fileStream.WriteAsync(segment.Array, segment.Offset, segment.Count, cancellationToken).ConfigureAwait(false);
            }

            // Touch

            // TODO: Under load we see intermittent IO exceptions. This is a temp fix.
            for (var retryCount = 0; retryCount < _retryMax; retryCount++)
            {
                try
                {
                    File.SetLastAccessTimeUtc(path, DateTime.UtcNow);
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(_retryMs);
                }
            }
        }

        private static FileStream WaitForFile(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize = 4096)
        {
            // TODO: Under load we see intermittent IO exceptions. This is a temp fix.
            for (var retryCount = 0; retryCount < _retryMax; retryCount++)
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(path, mode, access, share, bufferSize);
                    return fs;
                }
                catch (IOException)
                {
                    fs?.Dispose();

                    Thread.Sleep(_retryMs);
                    continue;
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string DeriveCommitRefFileName(string branch, string name)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refName = $@"{branch}\{name}.commit";
            return refName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DeriveFileName(Sha1 sha1)
        {
            var tokens = sha1.Split(2);

            var fileName = $@"{tokens.Key}\{tokens.Value}";
            return fileName;
        }

        #endregion
    }
}
