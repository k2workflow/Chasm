#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay;
using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static partial class CommitTests // .TreeId
    {
        #region Fields

        private static readonly TreeId TreeId1 = new TreeId(Sha1.Hash(nameof(TreeId1)));

        #endregion

        #region Methods

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_TreeId_Empty))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_TreeId_Empty(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), default, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_TreeId))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_TreeId(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), TreeId1, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        #endregion
    }
}
