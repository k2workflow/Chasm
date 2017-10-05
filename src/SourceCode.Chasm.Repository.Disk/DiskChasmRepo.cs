using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SourceCode.Chasm.IO.Disk
{
    public sealed partial class DiskChasmRepo : IChasmRepository
    {
        #region Fields

        private readonly string _refsContainer;
        private readonly string _objectsContainer;

        #endregion

        #region Properties

        public IChasmSerializer Serializer { get; }

        public CompressionLevel CompressionLevel { get; }

        public int MaxDop { get; }

        #endregion

        #region Constructors

        public DiskChasmRepo(string rootFolder, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
        {
            if (string.IsNullOrWhiteSpace(rootFolder) || rootFolder.Length <= 2) throw new ArgumentNullException(nameof(rootFolder)); // "C:\" is shortest permitted path
            if (rootFolder[rootFolder.Length - 1] != Path.DirectorySeparatorChar) throw new ArgumentException("Path must end with " + Path.DirectorySeparatorChar, nameof(rootFolder));
            var rootPath = Path.GetFullPath(rootFolder);
            if (rootPath != rootFolder) throw new ArgumentException("Path must be fully qualified", nameof(rootFolder));

            if (!Enum.IsDefined(typeof(CompressionLevel), compressionLevel)) throw new ArgumentOutOfRangeException(nameof(compressionLevel));
            if (maxDop < -1 || maxDop == 0) throw new ArgumentOutOfRangeException(nameof(maxDop));

            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            CompressionLevel = compressionLevel;
            MaxDop = maxDop;

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

        private static byte[] ReadFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                return default;

            using (var fileStream = WaitForFile(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (fileStream == null)
                    return default;

                var offset = 0;
                var remaining = (int)fileStream.Length;

                var bytes = new byte[remaining];
                while (remaining > 0)
                {
                    var count = fileStream.Read(bytes, offset, remaining);
                    if (count == 0)
                        throw new EndOfStreamException("End of file");

                    offset += count;
                    remaining -= count;
                }

                return bytes;
            }
        }

        private static void WriteFile(string path, ArraySegment<byte> segment)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var fileStream = WaitForFile(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fileStream?.Write(segment.Array, segment.Offset, segment.Count);
            }

            // Touch
            for (var retryCount = 0; retryCount < _retryMax; retryCount++)
            {
                try
                {
                    File.SetLastAccessTime(path, DateTime.Now);
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

        private static void DeleteFile(string path)
        {
            for (var retryCount = 0; retryCount < _retryMax; retryCount++)
            {
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
                catch (IOException)
                {
                    Thread.Sleep(_retryMs);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DeriveFileName(Sha1 sha1)
        {
            var tokens = sha1.Split(2);

            var fileName = $@"{tokens.Key}\{tokens.Value}";
            return fileName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string DeriveCommitRefFileName(string branch, string name)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refName = $@"{branch}\{name}.commit";
            return refName;
        }

        #endregion
    }
}
