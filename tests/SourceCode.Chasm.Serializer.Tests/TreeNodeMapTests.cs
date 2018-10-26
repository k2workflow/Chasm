#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Buffers;
using SourceCode.Chasm.Serializer;
using SourceCode.Clay;
using Xunit;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.IO.Tests
{
    public static class TreeNodeMapTests
    {
        private static readonly crypt.SHA1 s_sha1 = crypt.SHA1.Create();

        #region Constants

        private static readonly TreeNode Node1 = new TreeNode(nameof(Node1), new BlobId(s_sha1.HashData(nameof(Node1))));
        private static readonly TreeNode Node2 = new TreeNode(nameof(Node2), new BlobId(s_sha1.HashData(nameof(Node2))));
        private static readonly TreeNode Node3 = new TreeNode(nameof(Node3), new BlobId(s_sha1.HashData(nameof(Node3))));

        #endregion

        #region Methods

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeNodeMap_Default))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeMap_Default(IChasmSerializer ser)
        {
            TreeNodeMap expected = default;
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                TreeNodeMap actual = ser.DeserializeTree(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeNodeMap_Empty))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeMap_Empty(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap();
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                TreeNodeMap actual = ser.DeserializeTree(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeNodeMap_Null))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeMap_Null(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap(null);
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                TreeNodeMap actual = ser.DeserializeTree(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeNodeMap_Empty_Array))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeMap_Empty_Array(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap();
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                TreeNodeMap actual = ser.DeserializeTree(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeNodeMap_1_Node))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeMap_1_Node(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap(Node1);
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                TreeNodeMap actual = ser.DeserializeTree(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeNodeMap_2_Nodes))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeMap_2_Nodes(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap(Node1, Node2);
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                TreeNodeMap actual = ser.DeserializeTree(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeNodeMap_3_Nodes))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeNodeMap_3_Nodes(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap(Node1, Node2, Node3);
            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                TreeNodeMap actual = ser.DeserializeTree(mem.Span);
                Assert.Equal(expected, actual);
            }
        }

        #endregion
    }
}
