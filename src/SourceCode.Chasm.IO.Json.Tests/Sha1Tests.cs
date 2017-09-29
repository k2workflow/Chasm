using Xunit;

namespace SourceCode.Chasm.IO.Json.Tests
{
    public static class Sha1Tests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Sha1_Empty))]
        public static void JsonChasmSerializer_Roundtrip_Sha1_Empty()
        {
            var ser = new JsonChasmSerializer();

            var expected = Sha1.Empty;
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeSha1(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Sha1))]
        public static void JsonChasmSerializer_Roundtrip_Sha1()
        {
            var ser = new JsonChasmSerializer();

            var expected = Sha1.Hash("a");
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeSha1(buf.Result);
                Assert.Equal(expected, actual);
            }
        }
    }
}
