#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Buffers;
using System;

namespace SourceCode.Chasm.IO
{
    partial interface IChasmSerializer // .CommitId
    {
        #region Methods

        BufferSession Serialize(CommitId model);

        CommitId DeserializeCommitId(ReadOnlySpan<byte> span);

        #endregion
    }
}
