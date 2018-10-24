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
    public static partial class CommitTests // .Parents
    {
        #region Constants

        private static readonly CommitId s_parent1 = new CommitId(Sha1.Hash(nameof(s_parent1)));
        private static readonly CommitId s_parent2 = new CommitId(Sha1.Hash(nameof(s_parent2)));
        private static readonly CommitId s_parent3 = new CommitId(Sha1.Hash(nameof(s_parent3)));

        #endregion

        #region Methods

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Parents_Null))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Parents_Null(IChasmSerializer ser)
        {
            // Force Commit to be non-default
            var expected = new Commit(new CommitId?(), default, default, default, "force");
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Parents_Empty))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Parents_Empty(IChasmSerializer ser)
        {
            var expected = new Commit(Array.Empty<CommitId>(), default, default, default, null);
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Parents_1_Empty))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Parents_1_Empty(IChasmSerializer ser)
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
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Parents_1))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Parents_1(IChasmSerializer ser)
        {
            var expected = new Commit(s_parent1, default, default, default, null);
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Parents_2))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Parents_2(IChasmSerializer ser)
        {
            CommitId[] parents = new[] { s_parent1, s_parent2 };

            var expected = new Commit(parents, default, default, default, null);
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Parents_3))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Parents_3(IChasmSerializer ser)
        {
            CommitId[] parents = new[] { s_parent1, s_parent2, s_parent3 };

            var expected = new Commit(parents, default, default, default, null);
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        #endregion
    }
}
