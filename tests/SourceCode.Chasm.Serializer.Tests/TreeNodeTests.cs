#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Buffers;
using SourceCode.Chasm.Serializer;
using SourceCode.Clay;
using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static class TreeNodeTests
    {
        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_WriteRead_NullTreeNodeMap))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_WriteRead_NullTreeNodeMap(IChasmSerializer ser)
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
        [Theory(DisplayName = nameof(ChasmSerializer_WriteRead_EmptyTreeNodeMap))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_WriteRead_EmptyTreeNodeMap(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap(new TreeNode[0]);

            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                TreeNodeMap actual = ser.DeserializeTree(mem.Span);

                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_WriteRead_TreeNodeMap))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_WriteRead_TreeNodeMap(IChasmSerializer ser)
        {
            var node0 = new TreeNode("a", NodeKind.Blob, Sha1.Hash("abc"));
            var node1 = new TreeNode("b", NodeKind.Tree, Sha1.Hash("def"));
            var node2 = new TreeNode("c", NodeKind.Tree, Sha1.Hash("hij"));
            var expected = new TreeNodeMap(node0, node1, node2);

            using (IMemoryOwner<byte> owner = ser.Serialize(expected, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);

                TreeNodeMap actual = ser.DeserializeTree(mem.Span);

                Assert.Equal(expected, actual);
            }
        }
    }
}