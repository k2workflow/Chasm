#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Disk
{
    partial class DiskChasmRepo // .Tree
    {
        #region Read

        public ValueTask<TreeNodeList> ReadTreeAsync(TreeId treeId, CancellationToken cancellationToken)
            => ChasmUtil.ReadTreeAsync(this, treeId, cancellationToken);

        public ValueTask<IReadOnlyDictionary<TreeId, TreeNodeList>> ReadTreeBatchAsync(IEnumerable<TreeId> treeIds, ParallelOptions parallelOptions)
            => ChasmUtil.ReadTreeBatchAsync(this, treeIds, parallelOptions);

        public ValueTask<TreeNodeList> ReadTreeAsync(string branch, string commitRefName, CancellationToken cancellationToken)
             => ChasmUtil.ReadTreeAsync(this, branch, commitRefName, cancellationToken);

        public ValueTask<TreeNodeList> ReadTreeAsync(CommitId commitId, CancellationToken cancellationToken)
            => ChasmUtil.ReadTreeAsync(this, commitId, cancellationToken);

        #endregion

        #region Write

        public ValueTask<TreeId> WriteTreeAsync(TreeNodeList tree, CancellationToken cancellationToken)
            => ChasmUtil.WriteTreeAsync(this, tree, cancellationToken);

        public ValueTask<CommitId> WriteTreeAsync(IReadOnlyList<CommitId> parents, TreeNodeList tree, DateTime utc, string message, CancellationToken cancellationToken)
            => ChasmUtil.WriteTreeAsync(this, parents, tree, utc, message, cancellationToken);

        #endregion
    }
}
