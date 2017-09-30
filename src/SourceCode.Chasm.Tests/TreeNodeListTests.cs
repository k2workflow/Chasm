using System;
using System.Linq;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class TreeNodeListTests
    {
        private static readonly TreeNode Node0 = new TreeNode(nameof(Node0), NodeKind.Tree, Sha1.Hash(nameof(Node0)));
        private static readonly TreeNode Node1 = new TreeNode(nameof(Node1), NodeKind.Tree, Sha1.Hash(nameof(Node1)));
        private static readonly TreeNode Node2 = new TreeNode(nameof(Node2), NodeKind.Tree, Sha1.Hash(nameof(Node2)));
        private static readonly TreeNode Node3 = new TreeNode(nameof(Node3), NodeKind.Tree, Sha1.Hash(nameof(Node3)));

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeList_Empty))]
        public static void TreeNodeList_Empty()
        {
            var noData = new TreeNodeList();
            var nullData = new TreeNodeList(null);
            var emptyData = new TreeNodeList(Array.Empty<TreeNode>());

            Assert.Equal(noData, nullData);
            Assert.Equal(noData.GetHashCode(), nullData.GetHashCode());

            Assert.Empty(noData);
            Assert.Empty(nullData);
            Assert.Empty(emptyData);

            Assert.Equal(noData, emptyData); // By design
            Assert.Equal(noData.GetHashCode(), emptyData.GetHashCode());
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeList_Sorting))]
        public static void TreeNodeList_Sorting()
        {
            var nodes = new[] { Node0, Node1 };
            var tree0 = new TreeNodeList(nodes.OrderBy(n => n.Sha1).ToArray());
            var tree1 = new TreeNodeList(nodes.OrderByDescending(n => n.Sha1).ToList()); // ICollection<T>

            Assert.Equal(tree0[0], tree1[0]);
            Assert.Equal(tree0[1], tree1[1]);

            nodes = new[] { Node0, Node1, Node2 };
            tree0 = new TreeNodeList(nodes.OrderBy(n => n.Sha1).ToArray());
            tree1 = new TreeNodeList(nodes.OrderByDescending(n => n.Sha1).ToList()); // ICollection<T>

            Assert.Equal(tree0[0], tree1[0]);
            Assert.Equal(tree0[1], tree1[1]);
            Assert.Equal(tree0[2], tree1[2]);

            nodes = new[] { Node0, Node1, Node2, Node3 };
            tree0 = new TreeNodeList(nodes.OrderBy(n => n.Sha1).ToArray());
            tree1 = new TreeNodeList(nodes.OrderByDescending(n => n.Sha1).ToList()); // ICollection<T>

            Assert.Equal(tree0[0], tree1[0]);
            Assert.Equal(tree0[1], tree1[1]);
            Assert.Equal(tree0[2], tree1[2]);
            Assert.Equal(tree0[3], tree1[3]);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeList_Duplicates))]
        public static void TreeNodeList_Duplicates()
        {
            var nodes = new[] { Node0, Node0 };
            Assert.Throws<ArgumentException>(() => new TreeNodeList(nodes));
            Assert.Throws<ArgumentException>(() => new TreeNodeList(nodes.ToList())); // ICollection<T>

            nodes = new[] { Node0, Node1, Node0 };
            Assert.Throws<ArgumentException>(() => new TreeNodeList(nodes));
            Assert.Throws<ArgumentException>(() => new TreeNodeList(nodes.ToList())); // ICollection<T>

            nodes = new[] { Node0, Node1, Node2, Node0 };
            Assert.Throws<ArgumentException>(() => new TreeNodeList(nodes));
            Assert.Throws<ArgumentException>(() => new TreeNodeList(nodes.ToList())); // ICollection<T>

            Assert.Throws<ArgumentException>(() => new TreeNodeList(Node0, new TreeNode(Node1.Name, NodeKind.Tree, Node0.Sha1))); // Duplicate Name
            Assert.Throws<ArgumentException>(() => new TreeNodeList(Node0, new TreeNode(Node0.Name, NodeKind.Tree, Node1.Sha1))); // Duplicate Sha1
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeList_Equality))]
        public static void TreeNodeList_Equality()
        {
            var expected = new TreeNodeList(new[] { new TreeNode("c1", NodeKind.Blob, Sha1.Hash("c1")), new TreeNode("c2", NodeKind.Tree, Sha1.Hash("c2")) });
            var node3 = new TreeNode("c3", NodeKind.Tree, Sha1.Hash("c3"));

            // Equal
            var actual = new TreeNodeList().Merge(expected);
            Assert.Equal(expected, actual);
            Assert.Equal(expected.GetHashCode(), actual.GetHashCode());

            // Less Nodes
            actual = new TreeNodeList().Merge(expected[0]);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            // More Nodes
            actual = new TreeNodeList().Merge(expected).Merge(node3);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            // Different Nodes
            actual = new TreeNodeList().Merge(expected[0]).Merge(node3);
            Assert.NotEqual(expected, actual); // hashcode is the same (node count)
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeList_Merge_Single))]
        public static void TreeNodeList_Merge_Single()
        {
            var list = new TreeNodeList();

            list = list.Merge(new TreeNode("b", NodeKind.Blob, Sha1.Hash("Test1")));
            list = list.Merge(new TreeNode("a", NodeKind.Tree, Sha1.Hash("Test2")));
            list = list.Merge(new TreeNode("c", NodeKind.Blob, Sha1.Hash("Test3")));
            list = list.Merge(new TreeNode("d", NodeKind.Tree, Sha1.Hash("Test4")));
            list = list.Merge(new TreeNode("g", NodeKind.Blob, Sha1.Hash("Test5")));
            list = list.Merge(new TreeNode("e", NodeKind.Tree, Sha1.Hash("Test6")));
            list = list.Merge(new TreeNode("f", NodeKind.Blob, Sha1.Hash("Test7")));

            var prev = list.Keys.First();
            foreach (var cur in list.Keys.Skip(1))
            {
                Assert.True(StringComparer.Ordinal.Compare(cur, prev) > 0);
                prev = cur;
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeList_Merge_TreeNodeList))]
        public static void TreeNodeList_Merge_TreeNodeList()
        {
            var list1 = new TreeNodeList();

            list1 = list1.Merge(new TreeNode("d", NodeKind.Tree, Sha1.Hash("Test4")));
            list1 = list1.Merge(new TreeNode("e", NodeKind.Tree, Sha1.Hash("Test5")));
            list1 = list1.Merge(new TreeNode("f", NodeKind.Blob, Sha1.Hash("Test6")));
            list1 = list1.Merge(new TreeNode("g", NodeKind.Blob, Sha1.Hash("Test7")));

            var list2 = new TreeNodeList();

            list2 = list2.Merge(new TreeNode("a", NodeKind.Tree, Sha1.Hash("Test1")));
            list2 = list2.Merge(new TreeNode("b", NodeKind.Blob, Sha1.Hash("Test2")));
            list2 = list2.Merge(new TreeNode("c", NodeKind.Blob, Sha1.Hash("Test3")));
            list2 = list2.Merge(new TreeNode("d", NodeKind.Tree, Sha1.Hash("Test4 Replace")));
            list2 = list2.Merge(new TreeNode("g", NodeKind.Blob, Sha1.Hash("Test5 Replace")));
            list2 = list2.Merge(new TreeNode("q", NodeKind.Tree, Sha1.Hash("Test8")));
            list2 = list2.Merge(new TreeNode("r", NodeKind.Blob, Sha1.Hash("Test9")));

            var list3 = list1.Merge(list2);

            Assert.Equal(9, list3.Count);

            Assert.Equal("a", list3[0].Name);
            Assert.Equal("b", list3[1].Name);
            Assert.Equal("c", list3[2].Name);
            Assert.Equal("d", list3[3].Name);
            Assert.Equal("e", list3[4].Name);
            Assert.Equal("f", list3[5].Name);
            Assert.Equal("g", list3[6].Name);
            Assert.Equal("q", list3[7].Name);
            Assert.Equal("r", list3[8].Name);

            Assert.Equal(list2[0].Sha1, list3[0].Sha1);
            Assert.Equal(list2[1].Sha1, list3[1].Sha1);
            Assert.Equal(list2[2].Sha1, list3[2].Sha1);
            Assert.Equal(list2[3].Sha1, list3[3].Sha1);
            Assert.Equal(list1[1].Sha1, list3[4].Sha1);
            Assert.Equal(list1[2].Sha1, list3[5].Sha1);
            Assert.Equal(list2[4].Sha1, list3[6].Sha1);
            Assert.Equal(list2[5].Sha1, list3[7].Sha1);
            Assert.Equal(list2[6].Sha1, list3[8].Sha1);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeList_Merge_Collection))]
        public static void TreeNodeList_Merge_Collection()
        {
            var list1 = new TreeNodeList();

            list1 = list1.Merge(new TreeNode("d", NodeKind.Tree, Sha1.Hash("Test4")));
            list1 = list1.Merge(new TreeNode("e", NodeKind.Tree, Sha1.Hash("Test5")));
            list1 = list1.Merge(new TreeNode("f", NodeKind.Blob, Sha1.Hash("Test6")));
            list1 = list1.Merge(new TreeNode("g", NodeKind.Blob, Sha1.Hash("Test7")));

            var list2 = new[]
            {
                new TreeNode("c", NodeKind.Blob, Sha1.Hash("Test3")),
                new TreeNode("a", NodeKind.Tree, Sha1.Hash("Test1")),
                new TreeNode("b", NodeKind.Blob, Sha1.Hash("Test2")),
                new TreeNode("d", NodeKind.Tree, Sha1.Hash("Test4 Replace")),
                new TreeNode("g", NodeKind.Blob, Sha1.Hash("Test5 Replace")),
                new TreeNode("q", NodeKind.Tree, Sha1.Hash("Test8")),
                new TreeNode("r", NodeKind.Blob, Sha1.Hash("Test9")),
            };

            var list3 = list1.Merge(list2);

            Assert.Equal(9, list3.Count);

            Assert.Equal("a", list3[0].Name);
            Assert.Equal("b", list3[1].Name);
            Assert.Equal("c", list3[2].Name);
            Assert.Equal("d", list3[3].Name);
            Assert.Equal("e", list3[4].Name);
            Assert.Equal("f", list3[5].Name);
            Assert.Equal("g", list3[6].Name);
            Assert.Equal("q", list3[7].Name);
            Assert.Equal("r", list3[8].Name);

            Assert.Equal(list2[1].Sha1, list3[0].Sha1);
            Assert.Equal(list2[2].Sha1, list3[1].Sha1);
            Assert.Equal(list2[0].Sha1, list3[2].Sha1);
            Assert.Equal(list2[3].Sha1, list3[3].Sha1);
            Assert.Equal(list1[1].Sha1, list3[4].Sha1);
            Assert.Equal(list1[2].Sha1, list3[5].Sha1);
            Assert.Equal(list2[4].Sha1, list3[6].Sha1);
            Assert.Equal(list2[5].Sha1, list3[7].Sha1);
            Assert.Equal(list2[6].Sha1, list3[8].Sha1);
        }
    }
}
