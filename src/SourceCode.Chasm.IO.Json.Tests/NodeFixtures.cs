﻿using Xunit;

namespace SourceCode.Chasm.IO.Json.Tests
{
    public static class NodeFixtures
    {
        private static readonly ChasmSerializer _serializer = new JsonChasmSerializer();

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_WriteRead_NullTreeNodeList))]
        public static void JsonChasmSerializer_WriteRead_NullTreeNodeList()
        {
            var expected = new TreeNodeList();

            using (var seg = _serializer.Serialize(expected))
            {
                var actual = _serializer.DeserializeTree(seg.Result);

                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_WriteRead_EmptyTreeNodeList))]
        public static void JsonChasmSerializer_WriteRead_EmptyTreeNodeList()
        {
            var expected = new TreeNodeList(new TreeNode[0]);

            using (var seg = _serializer.Serialize(expected))
            {
                var actual = _serializer.DeserializeTree(seg.Result);

                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_WriteRead_TreeNodeList))]
        public static void JsonChasmSerializer_WriteRead_TreeNodeList()
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
