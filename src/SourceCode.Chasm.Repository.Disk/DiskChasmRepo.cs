using System;
using System.Globalization;
using System.IO;
using SourceCode.Chasm.Serializer;

namespace SourceCode.Chasm.Repository.Disk
{
    public sealed partial class DiskChasmRepo : ChasmRepository
    {
        public const int PrefixLength = 2;
        private const int _retryMax = 10;
        private const int _retryMs = 15;

        private readonly string _refsContainer;
        private readonly string _objectsContainer;

        /// <summary>
        /// Gets the root path for the repository.
        /// </summary>
        public string RootPath { get; }

        public DiskChasmRepo(string rootFolder, IChasmSerializer serializer, int maxDop = -1)
            : base(serializer, maxDop)
        {
            if (string.IsNullOrWhiteSpace(rootFolder) || rootFolder.Length < 3) throw new ArgumentNullException(nameof(rootFolder)); // "C:\" is shortest permitted path

#if !NETSTANDARD2_0
            RootPath = rootFolder.EndsWith(Path.DirectorySeparatorChar)
#else
            RootPath = rootFolder.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
#endif
                ? Path.GetFullPath(rootFolder)
                : Path.GetFullPath(rootFolder + Path.DirectorySeparatorChar);

            // Root
            {
                if (!Directory.Exists(RootPath))
                    Directory.CreateDirectory(RootPath);
            }

            // Objects
            {
                const string container = "objects";
                string path = Path.Combine(RootPath, container);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                _objectsContainer = path;
            }

            // Refs
            {
                const string container = "refs";
                string path = Path.Combine(RootPath, container);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                _refsContainer = path;
            }
        }
    }
}
