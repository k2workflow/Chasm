#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System.Buffers;
using SourceCode.Chasm.Serializer;
using SourceCode.Clay;
using Xunit;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.IO.Tests
{
    public static class TreeNodeMapTests
    {
        private static readonly crypt.SHA1 s_hasher = crypt.SHA1.Create();

        private static readonly TreeNode s_node1 = new TreeNode(nameof(s_node1), new BlobId(s_hasher.HashData(nameof(s_node1))));
        private static readonly TreeNode s_node2 = new TreeNode(nameof(s_node2), new BlobId(s_hasher.HashData(nameof(s_node2))));
        private static readonly TreeNode s_node3 = new TreeNode(nameof(s_node3), new BlobId(s_hasher.HashData(nameof(s_node3))));

        [Trait("Type", "Unit")]
        [Theory]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeMap_Default(IChasmSerializer ser)
        {
            TreeNodeMap expected = default;

            using (IMemoryOwner<byte> owner = ser.Serialize(expected))
            {
                TreeNodeMap actual = ser.DeserializeTree(owner.Memory.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeMap_Empty(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap();

            using (IMemoryOwner<byte> owner = ser.Serialize(expected))
            {
                TreeNodeMap actual = ser.DeserializeTree(owner.Memory.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeMap_Null(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap(null);

            using (IMemoryOwner<byte> owner = ser.Serialize(expected))
            {
                TreeNodeMap actual = ser.DeserializeTree(owner.Memory.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeMap_Empty_Array(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap();

            using (IMemoryOwner<byte> owner = ser.Serialize(expected))
            {
                TreeNodeMap actual = ser.DeserializeTree(owner.Memory.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeMap_1_Node(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap(s_node1);

            using (IMemoryOwner<byte> owner = ser.Serialize(expected))
            {
                TreeNodeMap actual = ser.DeserializeTree(owner.Memory.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeMap_2_Nodes(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap(s_node1, s_node2);

            using (IMemoryOwner<byte> owner = ser.Serialize(expected))
            {
                TreeNodeMap actual = ser.DeserializeTree(owner.Memory.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeMap_3_Nodes(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap(s_node1, s_node2, s_node3);

            using (IMemoryOwner<byte> owner = ser.Serialize(expected))
            {
                TreeNodeMap actual = ser.DeserializeTree(owner.Memory.Span);
                Assert.Equal(expected, actual);
            }
        }
    }
}
