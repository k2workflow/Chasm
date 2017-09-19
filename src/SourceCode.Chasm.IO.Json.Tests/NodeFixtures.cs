using SourceCode.Chasm;
using SourceCode.Chasm.IO;
using SourceCode.Chasm.IO.Json;
using SourceCode.Clay;
using Xunit;

namespace SourceCode.Mamba.SqlServer.Schema.IO.Json.Units
{
    public static class NodeFixtures
    {
        private static readonly ChasmSerializer _serializer = new JsonChasmSerializer();

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonCasSerializer_WriteRead_EmptyTreeNodeList))]
        public static void JsonCasSerializer_WriteRead_EmptyTreeNodeList()
        {
            var expected = new TreeNodeList();

            using (var seg = _serializer.Serialize(expected))
            {
                var actual = _serializer.DeserializeTree(seg.Result);

                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonCasSerializer_WriteRead_TreeNodeList))]
        public static void JsonCasSerializer_WriteRead_TreeNodeList()
        {
            var expected = new TreeNodeList(new TreeNode("a", NodeKind.Blob, Sha1.Hash("abc")));

            using (var seg = _serializer.Serialize(expected))
            {
                var actual = _serializer.DeserializeTree(seg.Result);

                Assert.Equal(expected, actual);
            }
        }
    }
}
