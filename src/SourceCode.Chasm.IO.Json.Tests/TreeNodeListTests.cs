using System;
using Xunit;

namespace SourceCode.Chasm.IO.Json.Tests
{
    public static class TreeNodeListTests
    {
        private static readonly TreeNode Node1 = new TreeNode(nameof(Node1), new BlobId(Sha1.Hash(nameof(Node1))));
        private static readonly TreeNode Node2 = new TreeNode(nameof(Node2), new BlobId(Sha1.Hash(nameof(Node2))));
        private static readonly TreeNode Node3 = new TreeNode(nameof(Node3), new BlobId(Sha1.Hash(nameof(Node3))));

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_TreeNodeList_Default))]
        public static void JsonChasmSerializer_Roundtrip_TreeNodeList_Default()
        {
            var ser = new JsonChasmSerializer();

            TreeNodeList expected = default;
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_TreeNodeList_Empty))]
        public static void JsonChasmSerializer_Roundtrip_TreeNodeList_Empty()
        {
            var ser = new JsonChasmSerializer();

            var expected = new TreeNodeList();
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_TreeNodeList_Null))]
        public static void JsonChasmSerializer_Roundtrip_TreeNodeList_Null()
        {
            var ser = new JsonChasmSerializer();

            var expected = new TreeNodeList(null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_TreeNodeList_Empty_Array))]
        public static void JsonChasmSerializer_Roundtrip_TreeNodeList_Empty_Array()
        {
            var ser = new JsonChasmSerializer();

            var expected = new TreeNodeList(Array.Empty<TreeNode>());
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_TreeNodeList_1_Node))]
        public static void JsonChasmSerializer_Roundtrip_TreeNodeList_1_Node()
        {
            var ser = new JsonChasmSerializer();

            var expected = new TreeNodeList(Node1);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_TreeNodeList_2_Nodes))]
        public static void JsonChasmSerializer_Roundtrip_TreeNodeList_2_Nodes()
        {
            var ser = new JsonChasmSerializer();

            var expected = new TreeNodeList(Node1, Node2);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_TreeNodeList_3_Nodes))]
        public static void JsonChasmSerializer_Roundtrip_TreeNodeList_3_Nodes()
        {
            var ser = new JsonChasmSerializer();

            var expected = new TreeNodeList(Node1, Node2, Node3);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTree(buf.Result);
                Assert.Equal(expected, actual);
            }
        }
    }
}
