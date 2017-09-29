using Xunit;

namespace SourceCode.Chasm.IO.Json.Tests
{
    public static partial class CommitTests // .Parents
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Commit_Default))]
        public static void JsonChasmSerializer_Roundtrip_Commit_Default()
        {
            var ser = new JsonChasmSerializer();

            // All default
            var expected = new Commit(null, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }
    }
}
