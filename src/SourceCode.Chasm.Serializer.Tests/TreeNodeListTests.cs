using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static class TreeNodeListTests
    {
        private static readonly TreeNode Node1 = new TreeNode(nameof(Node1), new BlobId(Sha1.Hash(nameof(Node1))));
        private static readonly TreeNode Node2 = new TreeNode(nameof(Node2), new BlobId(Sha1.Hash(nameof(Node2))));
        private static readonly TreeNode Node3 = new TreeNode(nameof(Node3), new BlobId(Sha1.Hash(nameof(Node3))));

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeNodeList_Default))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeList_Default(IChasmSerializer ser)
        {
            TreeNodeList expected = default;
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeNodeList_Empty))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeList_Empty(IChasmSerializer ser)
        {
            var expected = new TreeNodeList();
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeNodeList_Null))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeList_Null(IChasmSerializer ser)
        {
            var expected = new TreeNodeList(null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeNodeList_Empty_Array))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeList_Empty_Array(IChasmSerializer ser)
        {
            var expected = new TreeNodeList();
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeNodeList_1_Node))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeList_1_Node(IChasmSerializer ser)
        {
            var expected = new TreeNodeList(Node1);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeNodeList_2_Nodes))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeList_2_Nodes(IChasmSerializer ser)
        {
            var expected = new TreeNodeList(Node1, Node2);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeNodeList_3_Nodes))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeList_3_Nodes(IChasmSerializer ser)
        {
            var expected = new TreeNodeList(Node1, Node2, Node3);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(buf.Result);
                Assert.Equal(expected, actual);
            }
        }
    }
}
