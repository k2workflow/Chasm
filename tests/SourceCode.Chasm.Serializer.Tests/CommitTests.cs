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
    public static partial class CommitTests
    {
        #region Constants

        private static readonly crypt.SHA1 s_sha1 = crypt.SHA1.Create();

        private static readonly CommitId s_parent1 = new CommitId(s_sha1.HashData(nameof(s_parent1)));
        private static readonly CommitId s_parent2 = new CommitId(s_sha1.HashData(nameof(s_parent2)));
        private static readonly CommitId s_parent3 = new CommitId(s_sha1.HashData(nameof(s_parent3)));
        private static readonly TreeId s_treeId1 = new TreeId(s_sha1.HashData(nameof(s_treeId1)));

        #endregion

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Default))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Default(IChasmSerializer ser)
        {
            var expected = new Commit();
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                Commit actual = ser.DeserializeCommit(mem.Span);
                Assert.Equal(expected, actual);
            }
        }
    }
}
