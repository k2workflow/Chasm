using Xunit;

namespace SourceCode.Chasm.IO.Json.Tests
{
    public static partial class CommitTests // .TreeId
    {
        private static readonly TreeId TreeId1 = new TreeId(Sha1.Hash(nameof(TreeId1)));

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Commit_TreeId_Empty))]
        public static void JsonChasmSerializer_Roundtrip_Commit_TreeId_Empty()
        {
            var ser = new JsonChasmSerializer();

            var expected = new Commit(null, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Commit_TreeId))]
        public static void JsonChasmSerializer_Roundtrip_Commit_TreeId()
        {
            var ser = new JsonChasmSerializer();

            var expected = new Commit(null, TreeId1, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }
    }
}
