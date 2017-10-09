#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static partial class CommitIdTests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_CommitId_Default))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_CommitId_Default(IChasmSerializer ser)
        {
            var expected = CommitId.Empty;
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommitId(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_CommitId))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_CommitId(IChasmSerializer ser)
        {
            var expected = new CommitId(Sha1.Hash("abc"));
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommitId(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        #endregion
    }
}
