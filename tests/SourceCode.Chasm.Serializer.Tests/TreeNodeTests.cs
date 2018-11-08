#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using SourceCode.Chasm.Serializer;
using SourceCode.Clay;
using Xunit;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.IO.Tests
{
    public static class TreeNodeTests
    {
        private static readonly crypt.SHA1 s_hasher = crypt.SHA1.Create();

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_WriteRead_NullTreeNodeMap))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_WriteRead_NullTreeNodeMap(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap();

            Memory<byte> mem = ser.Serialize(expected);

            TreeNodeMap actual = ser.DeserializeTree(mem.Span);

            Assert.Equal(expected, actual);
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_WriteRead_EmptyTreeNodeMap))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_WriteRead_EmptyTreeNodeMap(IChasmSerializer ser)
        {
            var expected = new TreeNodeMap(new TreeNode[0]);

            Memory<byte> mem = ser.Serialize(expected);

            TreeNodeMap actual = ser.DeserializeTree(mem.Span);

            Assert.Equal(expected, actual);
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_WriteRead_TreeNodeMap))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_WriteRead_TreeNodeMap(IChasmSerializer ser)
        {
            var node0 = new TreeNode("a", NodeKind.Blob, s_hasher.HashData("abc"));
            var node1 = new TreeNode("b", NodeKind.Tree, s_hasher.HashData("def"));
            var node2 = new TreeNode("c", NodeKind.Tree, s_hasher.HashData("hij"));
            var expected = new TreeNodeMap(node0, node1, node2);

            Memory<byte> mem = ser.Serialize(expected);

            TreeNodeMap actual = ser.DeserializeTree(mem.Span);

            Assert.Equal(expected, actual);
        }
    }
}
