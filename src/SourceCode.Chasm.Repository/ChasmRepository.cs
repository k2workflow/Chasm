using SourceCode.Chasm.Serializer;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.Repository
{
    public abstract partial class ChasmRepository : IChasmRepository
    {
        // TODO: Is this a valid approach

        // Use a thread-local instance of the underlying crypto algorithm.
        [ThreadStatic]
        protected readonly crypt.SHA1 _hasher;

        public IChasmSerializer Serializer { get; }

        public CompressionLevel CompressionLevel { get; }

        public int MaxDop { get; }

        protected ChasmRepository(IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop, crypt.SHA1 hasher)
        {
            if (!Enum.IsDefined(typeof(CompressionLevel), compressionLevel)) throw new ArgumentOutOfRangeException(nameof(compressionLevel));
            if (maxDop < -1 || maxDop == 0) throw new ArgumentOutOfRangeException(nameof(maxDop));

            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            CompressionLevel = compressionLevel;
            MaxDop = maxDop;
        }

        public abstract ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, CancellationToken cancellationToken);

        public abstract Task WriteCommitRefAsync(CommitId? previousCommitId, string name, CommitRef commitRef, CancellationToken cancellationToken);

        public abstract ValueTask<IReadOnlyList<string>> GetNamesAsync(CancellationToken cancellationToken);

        public abstract ValueTask<IReadOnlyList<CommitRef>> GetBranchesAsync(string name, CancellationToken cancellationToken);

        protected static ChasmConcurrencyException BuildConcurrencyException(string name, string branch, Exception innerException)
            => new ChasmConcurrencyException($"Concurrent write detected on {nameof(CommitRef)} {name}/{branch}", innerException);
    }
}
