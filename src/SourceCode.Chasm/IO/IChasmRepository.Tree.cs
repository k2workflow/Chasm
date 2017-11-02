#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    // Arguably some of these could be extension methods. But by including them on the interface, concrete
    // classes have the opportunity to optimize the IO operations directly in storage.
    public partial interface IChasmRepository // .Tree
    {
        #region Read

        ValueTask<TreeNodeMap?> ReadTreeAsync(TreeId treeId, CancellationToken cancellationToken);

        ValueTask<TreeNodeMap?> ReadTreeAsync(string branch, string commitRefName, CancellationToken cancellationToken);

        ValueTask<TreeNodeMap?> ReadTreeAsync(CommitId commitId, CancellationToken cancellationToken);

        ValueTask<IReadOnlyDictionary<TreeId, TreeNodeMap>> ReadTreeBatchAsync(IEnumerable<TreeId> treeIds, CancellationToken cancellationToken);

        #endregion

        #region Write

        ValueTask<TreeId> WriteTreeAsync(TreeNodeMap tree, CancellationToken cancellationToken);

        ValueTask<CommitId> WriteTreeAsync(IReadOnlyList<CommitId> parents, TreeNodeMap tree, Audit author, Audit committer, string message, CancellationToken cancellationToken);

        #endregion
    }
}
