#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class TreeNodeMapTests
    {
        #region Constants

        private static readonly TreeNode Node0 = new TreeNode(nameof(Node0), NodeKind.Tree, Sha1.Hash(nameof(Node0)));
        private static readonly TreeNode Node0Blob = new TreeNode(nameof(Node0), NodeKind.Blob, Sha1.Hash(nameof(Node0Blob)));
        private static readonly TreeNode Node1 = new TreeNode(nameof(Node1), NodeKind.Blob, Sha1.Hash(nameof(Node1)));
        private static readonly TreeNode Node2 = new TreeNode(nameof(Node2), NodeKind.Tree, Sha1.Hash(nameof(Node2)));
        private static readonly TreeNode Node3 = new TreeNode(nameof(Node3), NodeKind.Blob, Sha1.Hash(nameof(Node3)));

        #endregion

        #region Methods

        private static void AssertEmpty(TreeNodeMap treeNodeMap)
        {
            Assert.Empty(treeNodeMap);
            Assert.Equal(TreeNodeMap.Empty, treeNodeMap); // By design
            Assert.Equal(TreeNodeMap.Empty.GetHashCode(), treeNodeMap.GetHashCode());
            Assert.Empty(treeNodeMap.Keys);

            Assert.Throws<IndexOutOfRangeException>(() => treeNodeMap[0]);
            Assert.Throws<KeyNotFoundException>(() => treeNodeMap["x"]);
            Assert.False(treeNodeMap.TryGetValue("x", out _));
            Assert.False(treeNodeMap.TryGetValue("x", NodeKind.Blob, out _));

            Assert.False(treeNodeMap.Equals(new object()));
            Assert.Contains("Count: 0", treeNodeMap.ToString());
            Assert.Equal(-1, treeNodeMap.IndexOf(Guid.NewGuid().ToString()));
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
            var nodes = new[] { Node0, Node1 };
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
            Assert.True(tree1.TryGetValue(Node0.Name, out var v20) && v20 == Node0);
            Assert.True(tree1.TryGetValue(Node1.Name, out var v21) && v21 == Node1);
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
            Assert.True(tree1.TryGetValue(Node0.Name, out var v30) && v30 == Node0);
            Assert.True(tree1.TryGetValue(Node1.Name, out var v31) && v31 == Node1);
            Assert.True(tree1.TryGetValue(Node2.Name, out var v32) && v32 == Node2);

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
            Assert.True(tree1.TryGetValue(Node0.Name, out var v40) && v40 == Node0);
            Assert.True(tree1.TryGetValue(Node1.Name, out var v41) && v41 == Node1);
            Assert.True(tree1.TryGetValue(Node2.Name, out var v42) && v42 == Node2);
            Assert.True(tree1.TryGetValue(Node3.Name, out var v43) && v43 == Node3);

            Assert.Equal(tree0[0], tree1[0]);
            Assert.Equal(tree0[1], tree1[1]);
            Assert.Equal(tree0[2], tree1[2]);
            Assert.Equal(tree0[3], tree1[3]);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Full_2))]
        public static void TreeNodeMap_Duplicate_Full_2()
        {
            var nodes = new[] { Node0, Node0 };

            var tree = new TreeNodeMap(nodes);
            Assert.Collection<TreeNode>(tree, n => Assert.Equal(n, Node0));

            tree = new TreeNodeMap(nodes.ToList()); // ICollection<T>
            Assert.Collection<TreeNode>(tree, n => Assert.Equal(n, Node0));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Full_3))]
        public static void TreeNodeMap_Duplicate_Full_3()
        {
            var nodes = new[] { Node0, Node1, Node0 }; // Shuffled

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
            var nodes = new[] { Node0, Node0Blob }; // Shuffled

            // Action
            var ex = Assert.Throws<ArgumentException>(() => new TreeNodeMap(nodes));

            // Assert
            Assert.Contains(Node0.Name, ex.Message);
            Assert.Contains(Node0.Sha1.ToString(), ex.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Full_3_Exception))]
        public static void TreeNodeMap_Duplicate_Full_3_Exception()
        {
            // Arrange
            var nodes = new[] { Node0, Node0Blob, Node1 }; // Shuffled

            // Action
            var ex = Assert.Throws<ArgumentException>(() => new TreeNodeMap(nodes));

            // Assert
            Assert.Contains(Node0.Name, ex.Message);
            Assert.Contains(Node0.Sha1.ToString(), ex.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Full_4))]
        public static void TreeNodeMap_Duplicate_Full_4()
        {
            var nodes = new[] { Node0, Node2, Node1, Node0 }; // Shuffled

            var tree = new TreeNodeMap(nodes);
            Assert.Collection<TreeNode>(tree, n => Assert.Equal(n, Node0), n => Assert.Equal(n, Node1), n => Assert.Equal(n, Node2));

            tree = new TreeNodeMap(nodes.ToList()); // ICollection<T>
            Assert.Collection<TreeNode>(tree, n => Assert.Equal(n, Node0), n => Assert.Equal(n, Node1), n => Assert.Equal(n, Node2));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Full_N))]
        public static void TreeNodeMap_Duplicate_Full_N()
        {
            var nodes = new[] { Node3, Node1, Node2, Node0, Node3, Node0, Node1, Node0, Node1, Node2, Node0, Node3 }; // Shuffled

            var tree = new TreeNodeMap(nodes);
            Assert.Collection<TreeNode>(tree, n => Assert.Equal(n, Node0), n => Assert.Equal(n, Node1), n => Assert.Equal(n, Node2), n => Assert.Equal(n, Node3));

            tree = new TreeNodeMap(nodes.ToList()); // ICollection<T>
            Assert.Collection<TreeNode>(tree, n => Assert.Equal(n, Node0), n => Assert.Equal(n, Node1), n => Assert.Equal(n, Node2), n => Assert.Equal(n, Node3));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Name))]
        public static void TreeNodeMap_Duplicate_Name()
        {
            var nodes = new[] { new TreeNode(Node0.Name, NodeKind.Tree, Node1.Sha1), Node0 }; // Reversed

            Assert.Throws<ArgumentException>(() => new TreeNodeMap(nodes));
            Assert.Throws<ArgumentException>(() => new TreeNodeMap(nodes.ToList())); // ICollection<T>
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Duplicate_Sha1))]
        public static void TreeNodeMap_Duplicate_Sha1()
        {
            var nodes = new[] { new TreeNode(Node1.Name, NodeKind.Tree, Node0.Sha1), Node0 }; // Reversed

            var tree0 = new TreeNodeMap(nodes);
            Assert.Collection<TreeNode>(tree0, n => Assert.Equal(n, Node0), n => Assert.Equal(n, nodes[0]));

            tree0 = new TreeNodeMap(nodes.ToList()); // ICollection<T>
            Assert.Collection<TreeNode>(tree0, n => Assert.Equal(n, Node0), n => Assert.Equal(n, nodes[0]));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Equality))]
        public static void TreeNodeMap_Equality()
        {
            var expected = new TreeNodeMap(new[] { new TreeNode("c1", NodeKind.Blob, Sha1.Hash("c1")), new TreeNode("c2", NodeKind.Tree, Sha1.Hash("c2")) });
            var node3 = new TreeNode("c3", NodeKind.Tree, Sha1.Hash("c3"));

            // Equal
            var actual = new TreeNodeMap().Merge(expected);
            Assert.Equal(expected, actual);
            Assert.Equal(expected.GetHashCode(), actual.GetHashCode());
            Assert.True(actual.Equals((object)expected));
            Assert.True(expected == actual);
            Assert.False(expected != actual);

            // Less Nodes
            actual = new TreeNodeMap().Merge(expected[0]);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());
            Assert.False(actual.Equals((object)expected));
            Assert.False(expected == actual);
            Assert.True(expected != actual);

            // More Nodes
            actual = new TreeNodeMap().Merge(expected).Merge(node3);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());
            Assert.False(actual.Equals((object)expected));
            Assert.False(expected == actual);
            Assert.True(expected != actual);

            // Different Nodes
            actual = new TreeNodeMap().Merge(expected[0]).Merge(node3);
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
            var node = new TreeNode("b", NodeKind.Blob, Sha1.Hash("Test1"));
            var list = new TreeNodeMap(node);

            // TreeNodeMap
            var merged = list.Merge(emptyTreeNodeMap);
            Assert.Equal(list, merged);

            merged = emptyTreeNodeMap.Merge(list);
            Assert.Equal(list, merged);

            // ICollection
            merged = list.Merge(Array.Empty<TreeNode>());
            Assert.Equal(list, merged);

            merged = emptyTreeNodeMap.Merge(list.Values.ToArray());
            Assert.Equal(list, merged);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Merge_Null))]
        public static void TreeNodeMap_Merge_Null()
        {
            // Arrange
            var list = new TreeNodeMap(Node0);

            // Action
            var merged = list.Merge(null);

            // Assert
            Assert.Equal(list, merged);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Merge_Single))]
        public static void TreeNodeMap_Merge_Single()
        {
            var list = new TreeNodeMap();

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
                Assert.True(string.CompareOrdinal(cur, prev) > 0);
                prev = cur;
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Merge_Single_Exist))]
        public static void TreeNodeMap_Merge_Single_Exist()
        {
            // Arrange
            var list = new TreeNodeMap();
            var expectedName = Guid.NewGuid().ToString();
            var expectedKind = NodeKind.Tree;
            var expectedSha1 = Sha1.Hash(Guid.NewGuid().ToString());

            list = list.Merge(new TreeNode(expectedName, NodeKind.Blob, Sha1.Hash("Test1")));

            // Action
            var actual = list.Merge(new TreeNode(expectedName, expectedKind, expectedSha1));
            var actualNode = actual[expectedName];

            // Assert
            Assert.Equal(expectedName, actualNode.Name);
            Assert.Equal(expectedKind, actualNode.Kind);
            Assert.Equal(expectedSha1, actualNode.Sha1);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Merge_TreeNodeMap))]
        public static void TreeNodeMap_Merge_TreeNodeMap()
        {
            var list1 = new TreeNodeMap();

            list1 = list1.Merge(new TreeNode("d", NodeKind.Tree, Sha1.Hash("Test4")));
            list1 = list1.Merge(new TreeNode("e", NodeKind.Tree, Sha1.Hash("Test5")));
            list1 = list1.Merge(new TreeNode("f", NodeKind.Blob, Sha1.Hash("Test6")));
            list1 = list1.Merge(new TreeNode("g", NodeKind.Blob, Sha1.Hash("Test7")));

            var list2 = new TreeNodeMap();

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
        [Fact(DisplayName = nameof(TreeNodeMap_Merge_Collection))]
        public static void TreeNodeMap_Merge_Collection()
        {
            var list1 = new TreeNodeMap();

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

            var dupes = new[]
            {
                new TreeNode(list2[0].Name, list2[0].Kind, list2[1].Sha1),
                new TreeNode(list2[1].Name, list2[1].Kind, list2[2].Sha1),
                new TreeNode(list2[2].Name, list2[2].Kind, list2[3].Sha1),
                new TreeNode(list2[3].Name, list2[3].Kind, list2[0].Sha1)
            };

            list3 = list3.Merge(dupes);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Delete))]
        public static void TreeNodeMap_Delete()
        {
            var sut = new TreeNodeMap(
                new TreeNode("a", NodeKind.Blob, Sha1.Hash("a")),
                new TreeNode("b", NodeKind.Blob, Sha1.Hash("b")),
                new TreeNode("c", NodeKind.Blob, Sha1.Hash("c"))
            );

            var removed = sut.Delete("a");
            Assert.Equal(2, removed.Count);
            Assert.Equal("b", removed[0].Name);
            Assert.Equal("c", removed[1].Name);

            removed = sut.Delete("b");
            Assert.Equal(2, removed.Count);
            Assert.Equal("a", removed[0].Name);
            Assert.Equal("c", removed[1].Name);

            removed = sut.Delete("c");
            Assert.Equal(2, removed.Count);
            Assert.Equal("a", removed[0].Name);
            Assert.Equal("b", removed[1].Name);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_Delete_Predicate))]
        public static void TreeNodeMap_Delete_Predicate()
        {
            var sut = new TreeNodeMap(
                new TreeNode("a", NodeKind.Blob, Sha1.Hash("a")),
                new TreeNode("b", NodeKind.Blob, Sha1.Hash("b")),
                new TreeNode("c", NodeKind.Blob, Sha1.Hash("c"))
            );

            var set = new HashSet<string>(StringComparer.Ordinal)
            { "a", "b", "d" };

            var removed = sut.Delete(set.Contains);
            Assert.Single(removed);
            Assert.Equal("c", removed[0].Name);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_IReadOnlyDictionary_Empty_GetEnumerator))]
        public static void TreeNodeMap_IReadOnlyDictionary_Empty_GetEnumerator()
        {
            // Arrange
            var treeNodeMap = new TreeNodeMap();
            var readOnlyDictionary = treeNodeMap as IReadOnlyDictionary<string, TreeNode>;

            // Action
            var enumerator = readOnlyDictionary.GetEnumerator();

            // Assert
            Assert.False(enumerator.MoveNext());

            var current = enumerator.Current;
            Assert.Null(current.Key);
            Assert.Equal(TreeNode.Empty, current.Value);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeMap_IReadOnlyDictionary_GetEnumerator))]
        public static void TreeNodeMap_IReadOnlyDictionary_GetEnumerator()
        {
            // Arrange
            var nodes = new[] { Node0, Node1 };
            var treeNodeMap = new TreeNodeMap(nodes);
            var readOnlyDictionary = treeNodeMap as IReadOnlyDictionary<string, TreeNode>;

            // Action
            var enumerator = readOnlyDictionary.GetEnumerator();

            // Assert
            Assert.True(enumerator.MoveNext());
            Assert.Equal(Node0, enumerator.Current.Value);

            Assert.True(enumerator.MoveNext());
            Assert.Equal(Node1, enumerator.Current.Value);

            Assert.False(enumerator.MoveNext());
            Assert.Equal(Node1, enumerator.Current.Value);
        }

        #endregion
    }
}
