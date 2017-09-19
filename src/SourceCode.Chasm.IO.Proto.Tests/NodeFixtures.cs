using SourceCode.Mamba.CasRepo;
using SourceCode.Mamba.CasRepo.IO;
using SourceCode.Mamba.CasRepo.IO.Proto;
using Xunit;

namespace SourceCode.Mamba.SqlServer.Schema.IO.Proto.Units
{
    public static class NodeFixtures
    {
        private static readonly ICasSerializer _serializer = new ProtoCasSerializer();

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoCasSerializer_WriteRead_EmptyTreeNodeList))]
        public static void ProtoCasSerializer_WriteRead_EmptyTreeNodeList()
        {
            var expected = new TreeNodeList();

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
