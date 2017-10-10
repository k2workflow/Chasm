#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class TreeNodeTests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNode_is_empty))]
        public static void TreeNode_is_empty()
        {
            var noData = new TreeNode();
            var nullData = new TreeNode("a", NodeKind.Blob, Sha1.Empty);

            // Name
            Assert.Null(TreeNode.Empty.Name);
            Assert.Null(TreeNode.EmptyBlob.Name);
            Assert.Null(TreeNode.EmptyTree.Name);
            Assert.Null(noData.Name);
            Assert.Equal("a", nullData.Name);
            Assert.Throws<ArgumentNullException>(() => new TreeNode(null, NodeKind.Blob, Sha1.Empty));
            Assert.Throws<ArgumentNullException>(() => new TreeNode(null, BlobId.Empty));
            Assert.Throws<ArgumentNullException>(() => new TreeNode(null, TreeId.Empty));

            // NodeKind
            Assert.Equal(NodeKind.Blob, default);
            Assert.Equal(default, TreeNode.Empty.Kind);
            Assert.Equal(default, noData.Kind);
            Assert.Equal(default, nullData.Kind);
            Assert.Equal(NodeKind.Blob, TreeNode.EmptyBlob.Kind);
            Assert.Equal(NodeKind.Tree, TreeNode.EmptyTree.Kind);
            Assert.Throws<ArgumentOutOfRangeException>(() => new TreeNode("a", (NodeKind)2, Sha1.Empty));

            // Sha1
            Assert.Equal(default, TreeNode.Empty.Sha1);
            Assert.Equal(default, TreeNode.EmptyBlob.Sha1);
            Assert.Equal(default, TreeNode.EmptyTree.Sha1);
            Assert.Equal(default, noData.Sha1);
            Assert.Equal(default, nullData.Sha1);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNode_equality))]
        public static void TreeNode_equality()
        {
            var tree1 = new TreeNode("a", NodeKind.Tree, Sha1.Hash("abc"));
            var tree2 = new TreeNode("a", NodeKind.Tree, Sha1.Hash("abc"));

            Assert.True(tree1 == tree2);
            Assert.False(tree1 != tree2);
            Assert.True(tree1.Equals((object)tree2));

            // Equal
            var actual = new TreeNode(tree1.Name, tree1.Kind, tree1.Sha1);
            Assert.Equal(tree1, actual);
            Assert.Equal(tree1.GetHashCode(), actual.GetHashCode());

            // Name
            actual = new TreeNode("b", tree1.Kind, tree1.Sha1);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            actual = new TreeNode("ab", tree1.Kind, tree1.Sha1);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            actual = new TreeNode(tree1.Name.ToUpperInvariant(), tree1.Kind, tree1.Sha1);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            // Kind
            actual = new TreeNode(tree1.Name, default, tree1.Sha1);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            actual = new TreeNode(tree1.Name, NodeKind.Blob, tree1.Sha1);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            // Sha1
            actual = new TreeNode(tree1.Name, tree1.Kind, Sha1.Hash("def"));
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNode_Deconstruct))]
        public static void TreeNode_Deconstruct()
        {
            var expected = new TreeNode("a", NodeKind.Blob, Sha1.Hash("abc"));

            var (name, kind, sha) = expected;
            var actual = new TreeNode(name, kind, sha);

            Assert.Equal(expected, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNode_Compare))]
        public static void TreeNode_Compare()
        {
            var comparer = TreeNodeComparer.Default;

            var tree1 = new TreeNode("a", NodeKind.Blob, Sha1.Hash("abc"));
            var tree2 = new TreeNode("a", NodeKind.Blob, Sha1.Hash("abc"));
            var tree3 = new TreeNode("d", NodeKind.Blob, Sha1.Hash("def"));
            var list = new[] { tree1, tree2, tree3 };

            Assert.True(TreeNode.Empty < tree1);
            Assert.True(tree1 > TreeNode.Empty);

            Assert.True(tree1.CompareTo(tree2) == 0);
            Assert.True(tree1.CompareTo(tree3) != 0);

            Array.Sort(list, comparer.Compare);

            Assert.True(list[0] <= list[1]);
            Assert.True(list[2] >= list[1]);
        }

        #endregion
    }
}
