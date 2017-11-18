#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    public static class ChasmRepositoryExtensions
    {
        #region Methods

        public static ValueTask<Commit?> ReadCommitAsync(this IChasmRepository chasmRepository, CommitId? commitId, CancellationToken cancellationToken)
        {
            if (chasmRepository == null) throw new ArgumentNullException(nameof(chasmRepository));
            return commitId.HasValue
                ? chasmRepository.ReadCommitAsync(commitId.Value, cancellationToken)
                : default;
        }

        public static ValueTask<ReadOnlyMemory<byte>?> ReadObjectAsync(this IChasmRepository chasmRepository, Sha1? objectId, CancellationToken cancellationToken)
        {
            if (chasmRepository == null) throw new ArgumentNullException(nameof(chasmRepository));
            return objectId.HasValue
                ? chasmRepository.ReadObjectAsync(objectId.Value, cancellationToken)
                : default;
        }

        public static ValueTask<ReadOnlyMemory<byte>?> ReadObjectAsync(this IChasmRepository chasmRepository, BlobId? objectId, CancellationToken cancellationToken)
        {
            if (chasmRepository == null) throw new ArgumentNullException(nameof(chasmRepository));
            return objectId.HasValue
                ? chasmRepository.ReadObjectAsync(objectId.Value.Sha1, cancellationToken)
                : default;
        }

        public static ValueTask<TreeMap?> ReadTreeMapAsync(this IChasmRepository chasmRepository, TreeMapId? treeMapId, CancellationToken cancellationToken)
        {
            if (chasmRepository == null) throw new ArgumentNullException(nameof(chasmRepository));
            return treeMapId.HasValue
                ? chasmRepository.ReadTreeMapAsync(treeMapId.Value, cancellationToken)
                : default;
        }

        #endregion
    }
}
