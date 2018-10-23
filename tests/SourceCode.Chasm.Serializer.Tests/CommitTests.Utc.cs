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
    public static partial class CommitTests // .Utc
    {
        private static readonly DateTimeOffset Utc1 = new DateTimeOffset(new DateTime(new DateTime(2000, 1, 1).Ticks, DateTimeKind.Utc));

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Utc))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Utc(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), default, new Audit("bob", Utc1), new Audit("mary", Utc1), null);
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }
    }
}
