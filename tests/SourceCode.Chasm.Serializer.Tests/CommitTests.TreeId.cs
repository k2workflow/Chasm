#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Buffers;
using SourceCode.Chasm.Serializer;
using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    partial class CommitTests // .TreeId
    {
        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_TreeId_Empty))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_TreeId_Empty(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), default, default, default, null);
            using (MemoryPool<byte> pool = MemoryPool<byte>.Shared)
            {
                Memory<byte> mem = ser.Serialize(expected);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_TreeId))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_TreeId(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), s_treeId1, default, default, null);
            using (MemoryPool<byte> pool = MemoryPool<byte>.Shared)
            {
                Memory<byte> mem = ser.Serialize(expected);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }
    }
}
