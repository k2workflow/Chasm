using Xunit;

namespace SourceCode.Chasm.IO.Proto.Tests
{
    public static class NodeFixtures
    {
        private static readonly IChasmSerializer _serializer = new ProtoCasSerializer();

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoCasSerializer_WriteRead_NullTreeNodeList))]
        public static void ProtoCasSerializer_WriteRead_NullTreeNodeList()
        {
            var expected = new TreeNodeList();

            using (var seg = _serializer.Serialize(expected))
            {
                var actual = _serializer.DeserializeTree(seg.Result);

                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoCasSerializer_WriteRead_EmptyTreeNodeList))]
        public static void ProtoCasSerializer_WriteRead_EmptyTreeNodeList()
        {
            var expected = new TreeNodeList(new TreeNode[0]);

            using (var seg = _serializer.Serialize(expected))
            {
                var actual = _serializer.DeserializeTree(seg.Result);

                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoCasSerializer_WriteRead_TreeNodeList))]
        public static void ProtoCasSerializer_WriteRead_TreeNodeList()
        {
            var nodes = new[]
            {
                new TreeNode("a", NodeKind.Blob, Sha1.Hash("abc"))
            };

            var expected = new TreeNodeList(nodes);

            using (var seg = _serializer.Serialize(expected))
            {
                var actual = _serializer.DeserializeTree(seg.Result);

                Assert.Equal(expected, actual);
            }
        }
    }
}
