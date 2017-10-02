using System;
using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static partial class CommitTests // .Message
    {
        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Message_Null))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Message_Null(IChasmSerializer ser)
        {
            // Force Commit to be non-default
            var force = DateTime.UtcNow;

            var expected = new Commit(null, default, force, null);
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
            var expected = new Commit(null, default, default, string.Empty);
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
            var expected = new Commit(null, default, default, " ");
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
            var expected = new Commit(null, default, default, "hello");
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
            var expected = new Commit(null, default, default, TestData.LongStr);
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

                var expected = new Commit(null, default, default, TestData.SurrogatePair);
                using (var buf = ser.Serialize(expected))
                {
                    var actual = ser.DeserializeCommit(buf.Result);
                    Assert.Equal(expected, actual);
                }
            }
        }
    }
}
