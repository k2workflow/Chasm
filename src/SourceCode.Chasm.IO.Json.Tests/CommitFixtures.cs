using SourceCode.Clay;
using System;
using Xunit;

namespace SourceCode.Chasm.IO.Json.Tests
{
    public static class CommitFixtures
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonCasSerializer_WriteRead_Commit))]
        public static void JsonCasSerializer_WriteRead_Commit()
        {
            var ser = new JsonChasmSerializer();

            var parent = new CommitId(Sha1.Hash("abc"));
            var treeId = new TreeId(Sha1.Hash("def"));
            var expected = new Commit(parent, treeId, DateTime.UtcNow, "Updating doc.");

            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }
    }
}
