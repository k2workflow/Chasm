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
            var nullData = new TreeNode("a", NodeKind.None, Sha1.Empty);

            Assert.True(default == TreeNode.Empty);
            Assert.False(default != TreeNode.Empty);

            // Name
            Assert.Null(TreeNode.Empty.Name);
            Assert.Null(TreeNode.EmptyBlob.Name);
            Assert.Null(TreeNode.EmptyTree.Name);
            Assert.Null(noData.Name);
            Assert.Equal("a", nullData.Name);
            Assert.Throws<ArgumentNullException>(() => new TreeNode(null, NodeKind.None, Sha1.Empty));
            Assert.Throws<ArgumentNullException>(() => new TreeNode(null, BlobId.Empty));
            Assert.Throws<ArgumentNullException>(() => new TreeNode(null, TreeId.Empty));

            // NodeKind
            Assert.Equal(NodeKind.None, default);
            Assert.Equal(default, TreeNode.Empty.Kind);
            Assert.Equal(default, noData.Kind);
            Assert.Equal(default, nullData.Kind);
            Assert.Equal(NodeKind.Blob, TreeNode.EmptyBlob.Kind);
            Assert.Equal(NodeKind.Tree, TreeNode.EmptyTree.Kind);
            Assert.Throws<ArgumentOutOfRangeException>(() => new TreeNode("a", (NodeKind)4, Sha1.Empty));

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
            var expected = new TreeNode("a", NodeKind.Blob, Sha1.Hash("abc"));

            // Equal
            var actual = new TreeNode(expected.Name, expected.Kind, expected.Sha1);
            Assert.Equal(expected, actual);
            Assert.True(expected.Equals((object)actual));
            Assert.Equal(expected.GetHashCode(), actual.GetHashCode());

            // Name
            actual = new TreeNode("b", expected.Kind, expected.Sha1);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new TreeNode("ab", expected.Kind, expected.Sha1);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new TreeNode(expected.Name.ToUpperInvariant(), expected.Kind, expected.Sha1);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            // Kind
            actual = new TreeNode(expected.Name, default, expected.Sha1);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new TreeNode(expected.Name, NodeKind.Tree, expected.Sha1);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            // Sha1
            actual = new TreeNode(expected.Name, expected.Kind, Sha1.Hash("def"));
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());
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

        #endregion
    }
}
