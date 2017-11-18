#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class TreeMapNodeTests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNode_equality))]
        public static void TreeNode_equality()
        {
            var tree1 = new TreeNode(NodeKind.Map, Sha1.Hash("abc"));
            var tree2 = new TreeNode(NodeKind.Map, Sha1.Hash("abc"));

            Assert.True(tree1 == tree2);
            Assert.False(tree1 != tree2);
            Assert.True(tree1.Equals((object)tree2));

            // Equal
            var actual = new TreeNode(tree1.Kind, tree1.Sha1);
            Assert.Equal(tree1, actual);
            Assert.Equal(tree1.GetHashCode(), actual.GetHashCode());

            // Kind
            actual = new TreeNode(default, tree1.Sha1);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            actual = new TreeNode(NodeKind.Blob, tree1.Sha1);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            // Sha1
            actual = new TreeNode(tree1.Kind, Sha1.Hash("def"));
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNode_Deconstruct))]
        public static void TreeNode_Deconstruct()
        {
            var expected = new TreeNode(NodeKind.Blob, Sha1.Hash("abc"));

            var (kind, sha) = expected;
            var actual = new TreeNode(kind, sha);

            Assert.Equal(expected, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNode_Compare))]
        public static void TreeNode_Compare()
        {
            var comparer = TreeNodeComparer.Default;

            var tree1 = new TreeNode(NodeKind.Blob, Sha1.Hash("abc"));
            var tree2 = new TreeNode(NodeKind.Blob, Sha1.Hash("abc"));
            var tree3 = new TreeNode(NodeKind.Blob, Sha1.Hash("def"));
            var list = new[] { tree1, tree2, tree3 };

            Assert.True(default(TreeNode) < tree1);
            Assert.True(tree1 > default(TreeNode));

            Assert.True(tree1.CompareTo(tree2) == 0);
            Assert.True(tree1.CompareTo(tree3) != 0);

            Array.Sort(list, comparer.Compare);

            Assert.True(list[0] <= list[1]);
            Assert.True(list[2] >= list[1]);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNode_Constructor_String_TreeMapId))]
        public static void TreeNode_Constructor_String_TreeMapId()
        {
            // Arrange
            var expectedTreeMapId = new TreeMapId();

            // Action
            var actual = new TreeNode(expectedTreeMapId);

            // Assert
            Assert.Equal(expectedTreeMapId.Sha1, actual.Sha1);
        }

        #endregion
    }
}
