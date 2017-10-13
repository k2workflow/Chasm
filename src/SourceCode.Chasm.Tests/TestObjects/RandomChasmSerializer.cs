#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Chasm.IO;
using SourceCode.Clay.Buffers;
using System;

namespace SourceCode.Chasm.Tests.TestObjects
{
    public abstract class RandomChasmSerializer : IChasmSerializer
    {
        #region Methods

        public Commit DeserializeCommit(ReadOnlySpan<byte> span) => CommitTestObject.Random;

        public abstract CommitId DeserializeCommitId(ReadOnlySpan<byte> span);

        public abstract TreeNodeList DeserializeTree(ReadOnlySpan<byte> span);

        public abstract BufferSession Serialize(TreeNodeList model);

        public abstract BufferSession Serialize(CommitId model);

        public abstract BufferSession Serialize(Commit model);

        #endregion
    }
}
