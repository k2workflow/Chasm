#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Buffers;
using SourceCode.Chasm.Serializer;
using SourceCode.Clay;
using Xunit;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.IO.Tests
{
    public static class CommitIdTests
    {
        private static readonly crypt.SHA1 s_sha1 = crypt.SHA1.Create();

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_CommitId_Default))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_CommitId_Default(IChasmSerializer ser)
        {
            var expected = new CommitId();
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                CommitId actual = ser.DeserializeCommitId(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_CommitId))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_CommitId(IChasmSerializer ser)
        {
            var expected = new CommitId(s_sha1.HashData("abc"));
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                CommitId actual = ser.DeserializeCommitId(mem.Span);
                Assert.Equal(expected, actual);
            }
        }
    }
}
