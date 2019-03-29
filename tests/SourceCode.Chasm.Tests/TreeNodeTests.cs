using System;
using SourceCode.Clay;
using Xunit;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.Tests
{
    public static class TreeNodeTests
    {
        private static readonly crypt.SHA1 s_hasher = crypt.SHA1.Create();

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNode_is_empty))]
        public static void TreeNode_is_empty()
        {
            var noData = new TreeNode();
            var nullData = new TreeNode("a", NodeKind.Blob, default);

            // Name
            Assert.Null(TreeNode.Empty.Name);
            Assert.Null(noData.Name);
            Assert.Equal("a", nullData.Name);
            Assert.Throws<ArgumentNullException>(() => new TreeNode(null, NodeKind.Blob, default));
            Assert.Throws<ArgumentNullException>(() => new TreeNode(null, new BlobId()));
            Assert.Throws<ArgumentNullException>(() => new TreeNode(null, new TreeId()));

            // NodeKind
            Assert.Equal(NodeKind.Blob, default);
            Assert.Equal(default, TreeNode.Empty.Kind);
            Assert.Equal(default, noData.Kind);
            Assert.Equal(default, nullData.Kind);
            Assert.Throws<ArgumentOutOfRangeException>(() => new TreeNode("a", (NodeKind)2, default));

            // Sha1
            Assert.Equal(default, TreeNode.Empty.Sha1);
            Assert.Equal(default, noData.Sha1);
            Assert.Equal(default, nullData.Sha1);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNode_equality))]
        public static void TreeNode_equality()
        {
            var tree1 = new TreeNode("a", NodeKind.Tree, s_hasher.HashData("abc"), new byte[] { 1, 2, 3 });
            var tree2 = new TreeNode("a", NodeKind.Tree, s_hasher.HashData("abc"), new byte[] { 1, 2, 3 });

            Assert.True(tree1 == tree2);
            Assert.False(tree1 != tree2);
            Assert.True(tree1.Equals((object)tree2));

            // Equal
            var actual = new TreeNode(tree1.Name, tree1.Kind, tree1.Sha1, tree1.Data);
            Assert.Equal(tree1, actual);
            Assert.Equal(tree1.GetHashCode(), actual.GetHashCode());

            // Name
            actual = new TreeNode("b", tree1.Kind, tree1.Sha1, tree1.Data);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            actual = new TreeNode("ab", tree1.Kind, tree1.Sha1, tree1.Data);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            actual = new TreeNode(tree1.Name.ToUpperInvariant(), tree1.Kind, tree1.Sha1, tree1.Data);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            // Kind
            actual = new TreeNode(tree1.Name, default, tree1.Sha1, tree1.Data);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            actual = new TreeNode(tree1.Name, NodeKind.Blob, tree1.Sha1, tree1.Data);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            // Sha1
            actual = new TreeNode(tree1.Name, tree1.Kind, s_hasher.HashData("def"), tree1.Data);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            // Data
            actual = new TreeNode(tree1.Name, tree1.Kind, tree1.Sha1, null);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            actual = new TreeNode(tree1.Name, tree1.Kind, tree1.Sha1, new byte[0]);
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            actual = new TreeNode(tree1.Name, tree1.Kind, tree1.Sha1, new byte[] { 1, 2 });
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());

            actual = new TreeNode(tree1.Name, tree1.Kind, tree1.Sha1, new byte[] { 4, 5, 6 });
            Assert.NotEqual(tree1, actual);
            Assert.NotEqual(tree1.GetHashCode(), actual.GetHashCode());
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNode_Deconstruct))]
        public static void TreeNode_Deconstruct()
        {
            var expected = new TreeNode("a", NodeKind.Blob, s_hasher.HashData("abc"), new byte[] { 1, 2, 3 });

            (string name, NodeKind kind, Sha1 sha, ReadOnlyMemory<byte>? data) = expected;
            var actual = new TreeNode(name, kind, sha, data);

            Assert.Equal(expected, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNode_Compare))]
        public static void TreeNode_Compare()
        {
            TreeNodeComparer comparer = TreeNodeComparer.Default;

            var tree1 = new TreeNode("a", NodeKind.Blob, s_hasher.HashData("abc"));
            var tree2 = new TreeNode("a", NodeKind.Blob, s_hasher.HashData("abc"));
            var tree3 = new TreeNode("a", NodeKind.Tree, s_hasher.HashData("abc"));
            var tree4 = new TreeNode("d", NodeKind.Blob, s_hasher.HashData("aaa"), new byte[] { 1, 2, 3 });
            var tree5 = new TreeNode("d", NodeKind.Blob, s_hasher.HashData("aaa"), new byte[] { 1, 1 });

            Assert.True(TreeNode.Empty < tree1);
            Assert.True(tree1 > TreeNode.Empty);

            Assert.Equal(0, tree1.CompareTo(tree2));
            Assert.Equal(-1, tree1.CompareTo(tree4)); // Name
            Assert.Equal(-1, tree1.CompareTo(tree3)); // Kind
            Assert.Equal(1, tree4.CompareTo(tree5)); // Data

            TreeNode[] list = new[] { tree1, tree2, tree3, tree4, tree5 };
            Array.Sort(list, comparer.Compare);

            Assert.True(list[0] <= list[1]);
            Assert.True(list[2] >= list[1]);
            Assert.True(list[3] >= list[2]);
            Assert.True(list[3] <= list[4]);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNode_Constructor_String_TreeId))]
        public static void TreeNode_Constructor_String_TreeId()
        {
            // Arrange
            var expectedTreeId = new TreeId();
            string expectedName = Guid.NewGuid().ToString();

            // Action
            var actual = new TreeNode(expectedName, expectedTreeId);

            // Assert
            Assert.Equal(expectedName, actual.Name);
            Assert.Equal(expectedTreeId.Sha1, actual.Sha1);
        }
    }
}
