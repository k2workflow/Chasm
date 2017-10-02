using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static class NodeFixtures
    {
        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_WriteRead_NullTreeNodeList))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_WriteRead_NullTreeNodeList(IChasmSerializer ser)
        {
            var expected = new TreeNodeList();

            using (var seg = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(seg.Result);

                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_WriteRead_EmptyTreeNodeList))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_WriteRead_EmptyTreeNodeList(IChasmSerializer ser)
        {
            var expected = new TreeNodeList(new TreeNode[0]);

            using (var seg = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(seg.Result);

                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_WriteRead_TreeNodeList))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_WriteRead_TreeNodeList(IChasmSerializer ser)
        {
            var expected = new TreeNodeList(new TreeNode("a", NodeKind.Blob, Sha1.Hash("abc")));

            using (var seg = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(seg.Result);

                Assert.Equal(expected, actual);
            }
        }
    }
}
