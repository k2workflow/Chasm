using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Chasm.Serializer;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository.Disk
{
    public sealed partial class DiskChasmRepo : ChasmRepository
    {
        public const int PrefixLength = 2;
        private const int _retryMax = 10;
        private const int _retryMs = 15;
        private readonly string _refsContainer;
        private readonly string _objectsContainer;
        private readonly string _scratchPath;

        /// <summary>
        /// Gets the root path for the repository.
        /// </summary>
        public string RootPath { get; }

        public DiskChasmRepo(string rootFolder, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
            : base(serializer, compressionLevel, maxDop)
        {
            if (string.IsNullOrWhiteSpace(rootFolder) || rootFolder.Length < 3) throw new ArgumentNullException(nameof(rootFolder)); // "C:\" is shortest permitted path
            string rootPath = Path.GetFullPath(rootFolder);

            RootPath = rootPath;

            // Scratch area
            _scratchPath = Path.GetTempPath();

            // Root
            {
                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);
            }

            // Refs
            {
                const string container = "refs";
                string path = Path.Combine(rootPath, container);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                _refsContainer = path;
            }

            // Objects
            {
                const string container = "objects";
                string path = Path.Combine(rootPath, container);

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
