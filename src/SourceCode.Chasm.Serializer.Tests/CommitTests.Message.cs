#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static partial class CommitTests // .Message
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Message_Null))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Message_Null(IChasmSerializer ser)
        {
            // Force Commit to be non-default
            var force = new Audit("bob", default);

            var expected = new Commit(new CommitId?(), default, force, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Message_Empty))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Message_Empty(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), default, default, default, string.Empty);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Message_Whitespace))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Message_Whitespace(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), default, default, default, " ");
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Message_Short))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Message_Short(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), default, default, default, "hello");
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Message_Long))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Message_Long(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), default, default, default, TestData.LongStr);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Message_Wide))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Message_Wide(IChasmSerializer ser)
        {
            var str = string.Empty;
            for (var i = 0; i < 200; i++)
            {
                str += TestData.SurrogatePair;

                var expected = new Commit(new CommitId?(), default, default, default, TestData.SurrogatePair);
                using (var buf = ser.Serialize(expected))
                {
                    var actual = ser.DeserializeCommit(buf.Result);
                    Assert.Equal(expected, actual);
                }
            }
        }

        #endregion
    }
}
