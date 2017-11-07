#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    public abstract partial class ChasmRepository : IChasmRepository
    {
        #region Properties

        public IChasmSerializer Serializer { get; }

        public CompressionLevel CompressionLevel { get; }

        public int MaxDop { get; }

        #endregion

        #region Constructors

        protected ChasmRepository(IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
        {
            if (!Enum.IsDefined(typeof(CompressionLevel), compressionLevel)) throw new ArgumentOutOfRangeException(nameof(compressionLevel));
            if (maxDop < -1 || maxDop == 0) throw new ArgumentOutOfRangeException(nameof(maxDop));

            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            CompressionLevel = compressionLevel;
            MaxDop = maxDop;
        }

        #endregion

        #region CommitRef

        protected static ChasmConcurrencyException BuildConcurrencyException(string name, string branch, Exception innerException)
            => new ChasmConcurrencyException($"Concurrent write detected on {nameof(CommitRef)} {name}/{branch}", innerException);

        public abstract ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, CancellationToken cancellationToken);

        public abstract Task WriteCommitRefAsync(CommitId? previousCommitId, string name, CommitRef commitRef, CancellationToken cancellationToken);

        public abstract ValueTask<IReadOnlyList<string>> GetNamesAsync(CancellationToken cancellationToken);

        public abstract ValueTask<IReadOnlyList<CommitRef>> GetBranchesAsync(string name, CancellationToken cancellationToken);

        #endregion
    }
}
