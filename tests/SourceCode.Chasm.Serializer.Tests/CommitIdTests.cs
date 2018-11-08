#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using SourceCode.Chasm.Serializer;
using SourceCode.Clay;
using Xunit;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.IO.Tests
{
    public static class CommitIdTests
    {
        private static readonly crypt.SHA1 s_hasher = crypt.SHA1.Create();

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_CommitId_Default))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_CommitId_Default(IChasmSerializer ser)
        {
            var expected = new CommitId();

            Memory<byte> mem = ser.Serialize(expected);

            CommitId actual = ser.DeserializeCommitId(mem.Span);
            Assert.Equal(expected, actual);
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_CommitId))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_CommitId(IChasmSerializer ser)
        {
            var expected = new CommitId(s_hasher.HashData("abc"));

            Memory<byte> mem = ser.Serialize(expected);

            CommitId actual = ser.DeserializeCommitId(mem.Span);
            Assert.Equal(expected, actual);
        }
    }
}
