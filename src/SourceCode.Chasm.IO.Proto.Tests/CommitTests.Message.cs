using System;
using Xunit;

namespace SourceCode.Chasm.IO.Proto.Tests
{
    public static partial class CommitTests // .Message
    {
        private const string LongStr = @"From Wikipedia: Astley was born on 6 February 1966 in Newton-le-Willows in Lancashire, the fourth child of his family. His parents divorced when he was five, and Astley was brought up by his father.[9] His musical career started when he was ten, singing in the local church choir.[10] During his schooldays, Astley formed and played the drums in a number of local bands, where he met guitarist David Morris.[2][11] After leaving school at sixteen, Astley was employed during the day as a driver in his father's market-gardening business and played drums on the Northern club circuit at night in bands such as Give Way – specialising in covering Beatles and Shadows songs – and FBI, which won several local talent competitions.[10]";
        private const string SurrogatePair = "\uD869\uDE01";

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoChasmSerializer_Roundtrip_Commit_Message_Null))]
        public static void ProtoChasmSerializer_Roundtrip_Commit_Message_Null()
        {
            var ser = new ProtoChasmSerializer();

            // Force Commit to be non-default
            var expected = new Commit(null, default, DateTime.UtcNow, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoChasmSerializer_Roundtrip_Commit_Message_Empty))]
        public static void ProtoChasmSerializer_Roundtrip_Commit_Message_Empty()
        {
            var ser = new ProtoChasmSerializer();

            var expected = new Commit(null, default, default, string.Empty);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoChasmSerializer_Roundtrip_Commit_Message_Whitespace))]
        public static void ProtoChasmSerializer_Roundtrip_Commit_Message_Whitespace()
        {
            var ser = new ProtoChasmSerializer();

            var expected = new Commit(null, default, default, " ");
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoChasmSerializer_Roundtrip_Commit_Message_Short))]
        public static void ProtoChasmSerializer_Roundtrip_Commit_Message_Short()
        {
            var ser = new ProtoChasmSerializer();

            var expected = new Commit(null, default, default, "hello");
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoChasmSerializer_Roundtrip_Commit_Message_Long))]
        public static void ProtoChasmSerializer_Roundtrip_Commit_Message_Long()
        {
            var ser = new ProtoChasmSerializer();

            var expected = new Commit(null, default, default, LongStr);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoChasmSerializer_Roundtrip_Commit_Message_Wide))]
        public static void ProtoChasmSerializer_Roundtrip_Commit_Message_Wide()
        {
            var ser = new ProtoChasmSerializer();

            var str = string.Empty;
            for (var i = 0; i < 200; i++)
            {
                str += SurrogatePair;

                var expected = new Commit(null, default, default, SurrogatePair);
                using (var buf = ser.Serialize(expected))
                {
                    var actual = ser.DeserializeCommit(buf.Result);
                    Assert.Equal(expected, actual);
                }
            }
        }
    }
}
