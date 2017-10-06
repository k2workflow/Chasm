#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial interface IChasmRepository // .Commit
    {
        #region Methods

        ValueTask<Commit> ReadCommitAsync(CommitId commitId, CancellationToken cancellationToken);

        ValueTask<CommitId> WriteCommitAsync(Commit commit, CancellationToken cancellationToken);

        #endregion
    }
}
