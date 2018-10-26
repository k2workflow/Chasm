#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using SourceCode.Chasm.Serializer;
using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    partial class CommitTests // .Message
    {
        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Message_Null))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Message_Null(IChasmSerializer ser)
        {
            // Force Commit to be non-default
            var force = new Audit("bob", DateTimeOffset.Now);

            var expected = new Commit(new CommitId?(), default, force, default, null);
            using (var pool = new SessionMemoryPool<byte>())
            {
                Memory<byte> mem = ser.Serialize(expected, pool);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Message_Empty))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Message_Empty(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), default, default, default, string.Empty);
            using (var pool = new SessionMemoryPool<byte>())
            {
                Memory<byte> mem = ser.Serialize(expected, pool);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Message_Whitespace))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Message_Whitespace(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), default, default, default, " ");
            using (var pool = new SessionMemoryPool<byte>())
            {
                Memory<byte> mem = ser.Serialize(expected, pool);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Message_Short))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Message_Short(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), default, default, default, "hello");
            using (var pool = new SessionMemoryPool<byte>())
            {
                Memory<byte> mem = ser.Serialize(expected, pool);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Message_Long))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Message_Long(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), default, default, default, TestData.LongStr);
            using (var pool = new SessionMemoryPool<byte>())
            {
                Memory<byte> mem = ser.Serialize(expected, pool);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Message_Wide))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Message_Wide(IChasmSerializer ser)
        {
            string str = string.Empty;
            for (int i = 0; i < 200; i++)
            {
                str += TestData.SurrogatePair;

                var expected = new Commit(new CommitId?(), default, default, default, TestData.SurrogatePair);
                using (var pool = new SessionMemoryPool<byte>())
                {
                    Memory<byte> mem = ser.Serialize(expected, pool);

                    Commit actual = ser.DeserializeCommit(mem.Span);
                    Assert.Equal(expected, actual);
                }
            }
        }
    }
}
