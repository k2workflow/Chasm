using SourceCode.Chasm.Serializer;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Repository
{
    public abstract partial class ChasmRepository : IChasmRepository
    {
        public IChasmSerializer Serializer { get; }

        public CompressionLevel CompressionLevel { get; }

        public int MaxDop { get; }

        protected ChasmRepository(IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
        {
            if (!Enum.IsDefined(typeof(CompressionLevel), compressionLevel)) throw new ArgumentOutOfRangeException(nameof(compressionLevel));
            if (maxDop < -1 || maxDop == 0) throw new ArgumentOutOfRangeException(nameof(maxDop));

            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            CompressionLevel = compressionLevel;
            MaxDop = maxDop;
        }

        protected static ChasmConcurrencyException BuildConcurrencyException(string name, string branch, Exception innerException)
            => new ChasmConcurrencyException($"Concurrent write detected on {nameof(CommitRef)} {name}/{branch}", innerException);

        public abstract ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, CancellationToken cancellationToken);

        public abstract Task WriteCommitRefAsync(CommitId? previousCommitId, string name, CommitRef commitRef, CancellationToken cancellationToken);

        public abstract ValueTask<IReadOnlyList<string>> GetNamesAsync(CancellationToken cancellationToken);

        public abstract ValueTask<IReadOnlyList<CommitRef>> GetBranchesAsync(string name, CancellationToken cancellationToken);
    }
}
