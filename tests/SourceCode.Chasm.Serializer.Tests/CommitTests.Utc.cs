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
    partial class CommitTests // .Utc
    {
        private static readonly DateTimeOffset s_utc1 = new DateTimeOffset(new DateTime(new DateTime(2000, 1, 1).Ticks, DateTimeKind.Utc));

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Utc))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Utc(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), default, new Audit("bob", s_utc1), new Audit("mary", s_utc1), null);
            using (MemoryPool<byte> pool = MemoryPool<byte>.Shared)
            {
                Memory<byte> mem = ser.Serialize(expected);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }
    }
}
