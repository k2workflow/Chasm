using System;
using System.Collections.Generic;
using System.Linq;
using SourceCode.Clay;
using Xunit;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.Tests
{
    public static class TreeNodeMapTests
    {
        private static readonly crypt.SHA1 s_sha1 = crypt.SHA1.Create();

        private static readonly TreeNode Node0 = new TreeNode(nameof(Node0), NodeKind.Tree, s_sha1.HashData(nameof(Node0)));
        private static readonly TreeNode Node0Blob = new TreeNode(nameof(Node0), NodeKind.Blob, s_sha1.HashData(nameof(Node0Blob)));
        private static readonly TreeNode Node1 = new TreeNode(nameof(Node1), NodeKind.Blob, s_sha1.HashData(nameof(Node1)));
        private static readonly TreeNode Node2 = new TreeNode(nameof(Node2), NodeKind.Tree, s_sha1.HashData(nameof(Node2)));
        private static readonly TreeNode Node3 = new TreeNode(nameof(Node3), NodeKind.Blob, s_sha1.HashData(nameof(Node3)));

        private static void AssertEmpty(TreeNodeMap tree)
        {
            Assert.Empty(tree);
            Assert.Equal(TreeNodeMap.Empty, tree); // By design
            Assert.Equal(TreeNodeMap.Empty.GetHashCode(), tree.GetHashCode());
            Assert.Empty(tree.Keys);

            Assert.Throws<IndexOutOfRangeException>(() => tree[0]);
            Assert.Throws<KeyNotFoundException>(() => tree["x"]);
            Assert.False(tree.TryGetValue("x", out _));
            Assert.False(tree.TryGetValue("x", NodeKind.Blob, out _));

            Assert.False(tree.Equals(new object()));
            Assert.Contains("Count: 0", tree.ToString());
            Assert.Equal(-1, tree.IndexOf(Guid.NewGuid().ToString()));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Empty))]
        public static void TreeNodeMap_Empty()
        {
            var noData = new TreeNodeMap();
            AssertEmpty(noData);

            var nullData = new TreeNodeMap(null);
            AssertEmpty(nullData);

            var collData = new TreeNodeMap((IList<TreeNode>)null);
            AssertEmpty(collData);

            var emptyData = new TreeNodeMap(Array.Empty<TreeNode>());
            AssertEmpty(emptyData);

            Assert.Empty(TreeNodeMap.Empty);
            Assert.Equal(default, TreeNodeMap.Empty);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Sorting))]
        public static void TreeNodeMap_Sorting()
        {
            TreeNode[] nodes = new[] { Node0, Node1 };
            var tree0 = new TreeNodeMap(nodes.OrderBy(n => n.Sha1).ToArray());
            var tree1 = new TreeNodeMap(nodes.OrderByDescending(n => n.Sha1).ToList()); // ICollection<T>

            Assert.Equal(tree0[0], tree1[0]);
            Assert.Equal(tree0[1], tree1[1]);

            Assert.True(tree1[Node0.Name] == Node0);
            Assert.True(tree1[Node1.Name] == Node1);
            Assert.False(tree1.ContainsKey("x"));
            Assert.True(tree1.ContainsKey(Node0.Name));
            Assert.True(tree1.ContainsKey(Node1.Name));
            Assert.False(tree1.TryGetValue("x", out _));
            Assert.True(tree1.TryGetValue(Node0.Name, out TreeNode v20) && v20 == Node0);
            Assert.True(tree1.TryGetValue(Node1.Name, out TreeNode v21) && v21 == Node1);
            Assert.False(tree1.TryGetValue(Node0.Name, NodeKind.Blob, out _));
            Assert.True(tree1.TryGetValue(Node0.Name, Node0.Kind, out _));

            nodes = new[] { Node0, Node1, Node2 };
            tree0 = new TreeNodeMap(nodes.OrderBy(n => n.Sha1).ToArray());
            tree1 = new TreeNodeMap(nodes.OrderByDescending(n => n.Sha1).ToList()); // ICollection<T>

            Assert.True(tree1[Node0.Name] == Node0);
            Assert.True(tree1[Node1.Name] == Node1);
            Assert.True(tree1[Node2.Name] == Node2);
            Assert.False(tree1.ContainsKey("x"));
            Assert.True(tree1.ContainsKey(Node0.Name));
            Assert.True(tree1.ContainsKey(Node1.Name));
            Assert.True(tree1.ContainsKey(Node2.Name));
            Assert.False(tree1.TryGetValue("x", out _));
            Assert.True(tree1.TryGetValue(Node0.Name, out TreeNode v30) && v30 == Node0);
            Assert.True(tree1.TryGetValue(Node1.Name, out TreeNode v31) && v31 == Node1);
            Assert.True(tree1.TryGetValue(Node2.Name, out TreeNode v32) && v32 == Node2);

            Assert.Equal(tree0[0], tree1[0]);
            Assert.Equal(tree0[1], tree1[1]);
            Assert.Equal(tree0[2], tree1[2]);

            nodes = new[] { Node0, Node1, Node2, Node3 };
            tree0 = new TreeNodeMap(nodes.OrderBy(n => n.Sha1).ToArray());
            tree1 = new TreeNodeMap(nodes.OrderByDescending(n => n.Sha1).ToList()); // ICollection<T>

            Assert.True(tree1[Node0.Name] == Node0);
            Assert.True(tree1[Node1.Name] == Node1);
            Assert.True(tree1[Node2.Name] == Node2);
            Assert.True(tree1[Node3.Name] == Node3);
            Assert.False(tree1.ContainsKey("x"));
            Assert.True(tree1.ContainsKey(Node0.Name));
            Assert.True(tree1.ContainsKey(Node1.Name));
            Assert.True(tree1.ContainsKey(Node2.Name));
            Assert.True(tree1.ContainsKey(Node3.Name));
            Assert.False(tree1.TryGetValue("x", out _));
            Assert.True(tree1.TryGetValue(Node0.Name, out TreeNode v40) && v40 == Node0);
            Assert.True(tree1.TryGetValue(Node1.Name, out TreeNode v41) && v41 == Node1);
            Assert.True(tree1.TryGetValue(Node2.Name, out TreeNode v42) && v42 == Node2);
            Assert.True(tree1.TryGetValue(Node3.Name, out TreeNode v43) && v43 == Node3);

            Assert.Equal(tree0[0], tree1[0]);
            Assert.Equal(tree0[1], tree1[1]);
            Assert.Equal(tree0[2], tree1[2]);
            Assert.Equal(tree0[3], tree1[3]);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Full_2))]
        public static void TreeNodeMap_Duplicate_Full_2()
        {
            TreeNode[] nodes = new[] { Node0, Node0 };

            var tree = new TreeNodeMap(nodes);
            Assert.Collection<TreeNode>(tree, n => Assert.Equal(n, Node0));

            tree = new TreeNodeMap(nodes.ToList()); // ICollection<T>
            Assert.Collection<TreeNode>(tree, n => Assert.Equal(n, Node0));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Full_3))]
        public static void TreeNodeMap_Duplicate_Full_3()
        {
            TreeNode[] nodes = new[] { Node0, Node1, Node0 }; // Shuffled

            var tree = new TreeNodeMap(nodes);
            Assert.Collection<TreeNode>(tree, n => Assert.Equal(n, Node0), n => Assert.Equal(n, Node1));

            tree = new TreeNodeMap(nodes.ToList()); // ICollection<T>
            Assert.Collection<TreeNode>(tree, n => Assert.Equal(n, Node0), n => Assert.Equal(n, Node1));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Full_2_Exception))]
        public static void TreeNodeMap_Duplicate_Full_2_Exception()
        {
            // Arrange
            TreeNode[] nodes = new[] { Node0, Node0Blob }; // Shuffled

            // Action
            ArgumentException ex = Assert.Throws<ArgumentException>(() => new TreeNodeMap(nodes));

            // Assert
            Assert.Contains(Node0.Name, ex.Message);
            Assert.Contains(Node0.Sha1.ToString(), ex.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Full_3_Exception))]
        public static void TreeNodeMap_Duplicate_Full_3_Exception()
        {
            // Arrange
            TreeNode[] nodes = new[] { Node0, Node0Blob, Node1 }; // Shuffled

            // Action
            ArgumentException ex = Assert.Throws<ArgumentException>(() => new TreeNodeMap(nodes));

            // Assert
            Assert.Contains(Node0.Name, ex.Message);
            Assert.Contains(Node0.Sha1.ToString(), ex.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Full_4))]
        public static void TreeNodeMap_Duplicate_Full_4()
        {
            TreeNode[] nodes = new[] { Node0, Node2, Node1, Node0 }; // Shuffled

            var tree = new TreeNodeMap(nodes);
            Assert.Collection<TreeNode>(tree, n => Assert.Equal(n, Node0), n => Assert.Equal(n, Node1), n => Assert.Equal(n, Node2));

            tree = new TreeNodeMap(nodes.ToList()); // ICollection<T>
            Assert.Collection<TreeNode>(tree, n => Assert.Equal(n, Node0), n => Assert.Equal(n, Node1), n => Assert.Equal(n, Node2));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Full_N))]
        public static void TreeNodeMap_Duplicate_Full_N()
        {
            TreeNode[] nodes = new[] { Node3, Node1, Node2, Node0, Node3, Node0, Node1, Node0, Node1, Node2, Node0, Node3 }; // Shuffled

            var tree = new TreeNodeMap(nodes);
            Assert.Collection<TreeNode>(tree, n => Assert.Equal(n, Node0), n => Assert.Equal(n, Node1), n => Assert.Equal(n, Node2), n => Assert.Equal(n, Node3));

            tree = new TreeNodeMap(nodes.ToList()); // ICollection<T>
            Assert.Collection<TreeNode>(tree, n => Assert.Equal(n, Node0), n => Assert.Equal(n, Node1), n => Assert.Equal(n, Node2), n => Assert.Equal(n, Node3));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Name))]
        public static void TreeNodeMap_Duplicate_Name()
        {
            TreeNode[] nodes = new[] { new TreeNode(Node0.Name, NodeKind.Tree, Node1.Sha1), Node0 }; // Reversed

            Assert.Throws<ArgumentException>(() => new TreeNodeMap(nodes));
            Assert.Throws<ArgumentException>(() => new TreeNodeMap(nodes.ToList())); // ICollection<T>
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Sha1))]
        public static void TreeNodeMap_Duplicate_Sha1()
        {
            TreeNode[] nodes = new[] { new TreeNode(Node1.Name, NodeKind.Tree, Node0.Sha1), Node0 }; // Reversed

            var tree0 = new TreeNodeMap(nodes);
            Assert.Collection<TreeNode>(tree0, n => Assert.Equal(n, Node0), n => Assert.Equal(n, nodes[0]));

            tree0 = new TreeNodeMap(nodes.ToList()); // ICollection<T>
            Assert.Collection<TreeNode>(tree0, n => Assert.Equal(n, Node0), n => Assert.Equal(n, nodes[0]));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Equality))]
        public static void TreeNodeMap_Equality()
        {
            var expected = new TreeNodeMap(new[] { new TreeNode("c1", NodeKind.Blob, s_sha1.HashData("c1")), new TreeNode("c2", NodeKind.Tree, s_sha1.HashData("c2")) });
            var node3 = new TreeNode("c3", NodeKind.Tree, s_sha1.HashData("c3"));

            // Equal
            TreeNodeMap actual = new TreeNodeMap().Merge(expected);
            Assert.Equal(expected, actual);
            Assert.Equal(expected.GetHashCode(), actual.GetHashCode());
            Assert.True(actual.Equals((object)expected));
            Assert.True(expected == actual);
            Assert.False(expected != actual);

            // Less Nodes
            actual = new TreeNodeMap().Add(expected[0]);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());
            Assert.False(actual.Equals((object)expected));
            Assert.False(expected == actual);
            Assert.True(expected != actual);

            // More Nodes
            actual = new TreeNodeMap().Merge(expected).Add(node3);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());
            Assert.False(actual.Equals((object)expected));
            Assert.False(expected == actual);
            Assert.True(expected != actual);

            // Different Nodes
            actual = new TreeNodeMap().Add(expected[0]).Add(node3);
            Assert.NotEqual(expected, actual); // hashcode is the same (node count)
            Assert.False(actual.Equals((object)expected));
            Assert.False(expected == actual);
            Assert.True(expected != actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_IndexOf))]
        public static void TreeNodeMap_IndexOf()
        {
            // Arrange
            var actual = new TreeNodeMap(new[] { Node0, Node1 });

            // Action/Assert
            Assert.Equal(-1, actual.IndexOf(null));
            Assert.True(actual.IndexOf(Guid.NewGuid().ToString()) < 0);
            Assert.Equal(0, actual.IndexOf(Node0.Name));
            Assert.Equal(1, actual.IndexOf(Node1.Name));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Merge_Empty))]
        public static void TreeNodeMap_Merge_Empty()
        {
            var emptyTreeNodeMap = new TreeNodeMap();
            var node = new TreeNode("b", NodeKind.Blob, s_sha1.HashData("Test1"));
            var tree = new TreeNodeMap(node);

            // TreeNodeMap
            TreeNodeMap merged = tree.Merge(emptyTreeNodeMap);
            Assert.Equal(tree, merged);

            merged = emptyTreeNodeMap.Merge(tree);
            Assert.Equal(tree, merged);

            // ICollection
            merged = tree.Merge(Array.Empty<TreeNode>());
            Assert.Equal(tree, merged);

            merged = emptyTreeNodeMap.Merge(tree.Values.ToArray());
            Assert.Equal(tree, merged);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Merge_Null))]
        public static void TreeNodeMap_Merge_Null()
        {
            // Arrange
            var tree = new TreeNodeMap(Node0);

            // Action
            TreeNodeMap merged = tree.Merge(null);

            // Assert
            Assert.Equal(tree, merged);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Merge_Single))]
        public static void TreeNodeMap_Merge_Single()
        {
            var tree = new TreeNodeMap();

            tree = tree.Add(new TreeNode("b", NodeKind.Blob, s_sha1.HashData("Test1")));
            tree = tree.Add(new TreeNode("a", NodeKind.Tree, s_sha1.HashData("Test2")));
            tree = tree.Add(new TreeNode("c", NodeKind.Blob, s_sha1.HashData("Test3")));
            tree = tree.Add(new TreeNode("d", NodeKind.Tree, s_sha1.HashData("Test4")));
            tree = tree.Add(new TreeNode("g", NodeKind.Blob, s_sha1.HashData("Test5")));
            tree = tree.Add(new TreeNode("e", NodeKind.Tree, s_sha1.HashData("Test6")));
            tree = tree.Add(new TreeNode("f", NodeKind.Blob, s_sha1.HashData("Test7")));

            string prev = tree.Keys.First();
            foreach (string cur in tree.Keys.Skip(1))
            {
                Assert.True(string.CompareOrdinal(cur, prev) > 0);
                prev = cur;
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Merge_Single_Exist))]
        public static void TreeNodeMap_Merge_Single_Exist()
        {
            // Arrange
            var tree = new TreeNodeMap();
            string expectedName = Guid.NewGuid().ToString();
            NodeKind expectedKind = NodeKind.Tree;
            Sha1 expectedSha1 = s_sha1.HashData(Guid.NewGuid().ToString());

            tree = tree.Add(new TreeNode(expectedName, NodeKind.Blob, s_sha1.HashData("Test1")));

            // Action
            TreeNodeMap actual = tree.Add(new TreeNode(expectedName, expectedKind, expectedSha1));
            TreeNode actualNode = actual[expectedName];

            // Assert
            Assert.Equal(expectedName, actualNode.Name);
            Assert.Equal(expectedKind, actualNode.Kind);
            Assert.Equal(expectedSha1, actualNode.Sha1);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Merge_TreeNodeMap))]
        public static void TreeNodeMap_Merge_TreeNodeMap()
        {
            var tree1 = new TreeNodeMap();

            tree1 = tree1.Add(new TreeNode("d", NodeKind.Tree, s_sha1.HashData("Test4")));
            tree1 = tree1.Add(new TreeNode("e", NodeKind.Tree, s_sha1.HashData("Test5")));
            tree1 = tree1.Add(new TreeNode("f", NodeKind.Blob, s_sha1.HashData("Test6")));
            tree1 = tree1.Add(new TreeNode("g", NodeKind.Blob, s_sha1.HashData("Test7")));

            var tree2 = new TreeNodeMap();

            tree2 = tree2.Add(new TreeNode("a", NodeKind.Tree, s_sha1.HashData("Test1")));
            tree2 = tree2.Add(new TreeNode("b", NodeKind.Blob, s_sha1.HashData("Test2")));
            tree2 = tree2.Add(new TreeNode("c", NodeKind.Blob, s_sha1.HashData("Test3")));
            tree2 = tree2.Add(new TreeNode("d", NodeKind.Tree, s_sha1.HashData("Test4 Replace")));
            tree2 = tree2.Add(new TreeNode("g", NodeKind.Blob, s_sha1.HashData("Test5 Replace")));
            tree2 = tree2.Add(new TreeNode("q", NodeKind.Tree, s_sha1.HashData("Test8")));
            tree2 = tree2.Add(new TreeNode("r", NodeKind.Blob, s_sha1.HashData("Test9")));

            TreeNodeMap tree3 = tree1.Merge(tree2);

            Assert.Equal(9, tree3.Count);

            Assert.Equal("a", tree3[0].Name);
            Assert.Equal("b", tree3[1].Name);
            Assert.Equal("c", tree3[2].Name);
            Assert.Equal("d", tree3[3].Name);
            Assert.Equal("e", tree3[4].Name);
            Assert.Equal("f", tree3[5].Name);
            Assert.Equal("g", tree3[6].Name);
            Assert.Equal("q", tree3[7].Name);
            Assert.Equal("r", tree3[8].Name);

            Assert.Equal(tree2[0].Sha1, tree3[0].Sha1);
            Assert.Equal(tree2[1].Sha1, tree3[1].Sha1);
            Assert.Equal(tree2[2].Sha1, tree3[2].Sha1);
            Assert.Equal(tree2[3].Sha1, tree3[3].Sha1);
            Assert.Equal(tree1[1].Sha1, tree3[4].Sha1);
            Assert.Equal(tree1[2].Sha1, tree3[5].Sha1);
            Assert.Equal(tree2[4].Sha1, tree3[6].Sha1);
            Assert.Equal(tree2[5].Sha1, tree3[7].Sha1);
            Assert.Equal(tree2[6].Sha1, tree3[8].Sha1);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Merge_Collection))]
        public static void TreeNodeMap_Merge_Collection()
        {
            var tree1 = new TreeNodeMap();

            tree1 = tree1.Add(new TreeNode("d", NodeKind.Tree, s_sha1.HashData("Test4")));
            tree1 = tree1.Add(new TreeNode("e", NodeKind.Tree, s_sha1.HashData("Test5")));
            tree1 = tree1.Add(new TreeNode("f", NodeKind.Blob, s_sha1.HashData("Test6")));
            tree1 = tree1.Add(new TreeNode("g", NodeKind.Blob, s_sha1.HashData("Test7")));

            TreeNode[] tree2 = new[]
            {
                new TreeNode("c", NodeKind.Blob, s_sha1.HashData("Test3")),
                new TreeNode("a", NodeKind.Tree, s_sha1.HashData("Test1")),
                new TreeNode("b", NodeKind.Blob, s_sha1.HashData("Test2")),
                new TreeNode("d", NodeKind.Tree, s_sha1.HashData("Test4 Replace")),
                new TreeNode("g", NodeKind.Blob, s_sha1.HashData("Test5 Replace")),
                new TreeNode("q", NodeKind.Tree, s_sha1.HashData("Test8")),
                new TreeNode("r", NodeKind.Blob, s_sha1.HashData("Test9")),
            };

            TreeNodeMap tree3 = tree1.Merge(tree2);

            Assert.Equal(9, tree3.Count);

            Assert.Equal("a", tree3[0].Name);
            Assert.Equal("b", tree3[1].Name);
            Assert.Equal("c", tree3[2].Name);
            Assert.Equal("d", tree3[3].Name);
            Assert.Equal("e", tree3[4].Name);
            Assert.Equal("f", tree3[5].Name);
            Assert.Equal("g", tree3[6].Name);
            Assert.Equal("q", tree3[7].Name);
            Assert.Equal("r", tree3[8].Name);

            Assert.Equal(tree2[1].Sha1, tree3[0].Sha1);
            Assert.Equal(tree2[2].Sha1, tree3[1].Sha1);
            Assert.Equal(tree2[0].Sha1, tree3[2].Sha1);
            Assert.Equal(tree2[3].Sha1, tree3[3].Sha1);
            Assert.Equal(tree1[1].Sha1, tree3[4].Sha1);
            Assert.Equal(tree1[2].Sha1, tree3[5].Sha1);
            Assert.Equal(tree2[4].Sha1, tree3[6].Sha1);
            Assert.Equal(tree2[5].Sha1, tree3[7].Sha1);
            Assert.Equal(tree2[6].Sha1, tree3[8].Sha1);

            TreeNode[] dupes = new[]
            {
                new TreeNode(tree2[0].Name, tree2[0].Kind, tree2[1].Sha1),
                new TreeNode(tree2[1].Name, tree2[1].Kind, tree2[2].Sha1),
                new TreeNode(tree2[2].Name, tree2[2].Kind, tree2[3].Sha1),
                new TreeNode(tree2[3].Name, tree2[3].Kind, tree2[0].Sha1)
            };

            tree3 = tree3.Merge(dupes);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Delete))]
        public static void TreeNodeMap_Delete()
        {
            var tree = new TreeNodeMap
            (
                new TreeNode("a", NodeKind.Blob, s_sha1.HashData("a")),
                new TreeNode("b", NodeKind.Blob, s_sha1.HashData("b")),
                new TreeNode("c", NodeKind.Blob, s_sha1.HashData("c"))
            );

            TreeNodeMap removed = tree.Delete("a");
            Assert.Equal(2, removed.Count);
            Assert.Equal("b", removed[0].Name);
            Assert.Equal("c", removed[1].Name);

            removed = tree.Delete("b");
            Assert.Equal(2, removed.Count);
            Assert.Equal("a", removed[0].Name);
            Assert.Equal("c", removed[1].Name);

            removed = tree.Delete("c");
            Assert.Equal(2, removed.Count);
            Assert.Equal("a", removed[0].Name);
            Assert.Equal("b", removed[1].Name);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Delete_Predicate))]
        public static void TreeNodeMap_Delete_Predicate()
        {
            var tree = new TreeNodeMap
            (
                new TreeNode("a", NodeKind.Blob, s_sha1.HashData("a")),
                new TreeNode("b", NodeKind.Blob, s_sha1.HashData("b")),
                new TreeNode("c", NodeKind.Blob, s_sha1.HashData("c"))
            );

            var set = new HashSet<string>(StringComparer.Ordinal)
            { "a", "b", "d" };

            TreeNodeMap removed = tree.Delete(set.Contains);
            Assert.Single(removed);
            Assert.Equal("c", removed[0].Name);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_IReadOnlyDictionary_Empty_GetEnumerator))]
        public static void TreeNodeMap_IReadOnlyDictionary_Empty_GetEnumerator()
        {
            // Arrange
            var tree = new TreeNodeMap();
            var readOnlyDictionary = tree as IReadOnlyDictionary<string, TreeNode>;

            // Action
            IEnumerator<KeyValuePair<string, TreeNode>> enumerator = readOnlyDictionary.GetEnumerator();

            // Assert
            Assert.False(enumerator.MoveNext());

            KeyValuePair<string, TreeNode> current = enumerator.Current;
            Assert.Null(current.Key);
            Assert.Equal(TreeNode.Empty, current.Value);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_IReadOnlyDictionary_GetEnumerator))]
        public static void TreeNodeMap_IReadOnlyDictionary_GetEnumerator()
        {
            // Arrange
            TreeNode[] nodes = new[] { Node0, Node1 };
            var tree = new TreeNodeMap(nodes);
            var readOnlyDictionary = tree as IReadOnlyDictionary<string, TreeNode>;

            // Action
            IEnumerator<KeyValuePair<string, TreeNode>> enumerator = readOnlyDictionary.GetEnumerator();

            // Assert
            Assert.True(enumerator.MoveNext());
            Assert.Equal(Node0, enumerator.Current.Value);

            Assert.True(enumerator.MoveNext());
            Assert.Equal(Node1, enumerator.Current.Value);

            Assert.False(enumerator.MoveNext());
            Assert.Equal(Node1, enumerator.Current.Value);
        }
    }
}
