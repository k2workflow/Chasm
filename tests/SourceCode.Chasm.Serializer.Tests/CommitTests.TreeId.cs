#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Buffers;
using SourceCode.Chasm.Serializer;
using SourceCode.Clay;
using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static partial class CommitTests // .TreeId
    {
        private static readonly TreeId s_treeId1 = new TreeId(Sha1.Hash(nameof(s_treeId1)));

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_TreeId_Empty))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_TreeId_Empty(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), default, default, default, null);
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

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
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }
    }
}
