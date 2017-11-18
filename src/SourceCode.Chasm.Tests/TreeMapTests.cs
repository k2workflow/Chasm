#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using TreePair = System.Collections.Generic.KeyValuePair<string, SourceCode.Chasm.TreeNode>;

namespace SourceCode.Chasm.Tests
{
    public static class TreeMapTests
    {
        #region Constants

        private static readonly TreePair Node0 = new TreePair(nameof(Node0), new TreeMapId(Sha1.Hash(nameof(Node0))));
        private static readonly TreePair Node0Blob = new TreePair(nameof(Node0), new BlobId(Sha1.Hash(nameof(Node0Blob))));
        private static readonly TreePair Node1 = new TreePair(nameof(Node1), new BlobId(Sha1.Hash(nameof(Node1))));
        private static readonly TreePair Node2 = new TreePair(nameof(Node2), new TreeMapId(Sha1.Hash(nameof(Node2))));
        private static readonly TreePair Node3 = new TreePair(nameof(Node3), new BlobId(Sha1.Hash(nameof(Node3))));

        #endregion

        #region Methods

        private static void AssertEmpty(TreeMap tree)
        {
            Assert.Empty(tree);
            Assert.Equal(TreeMap.Empty, tree); // By design
            Assert.Equal(TreeMap.Empty.GetHashCode(), tree.GetHashCode());
            Assert.Empty(tree.Keys);

            Assert.Throws<IndexOutOfRangeException>(() => tree[0]);
            Assert.Throws<KeyNotFoundException>(() => tree["x"]);
            Assert.False(tree.TryGetValue("x", out _));
            Assert.False(tree.TryGetBlobId("x", out _));

            Assert.False(tree.Equals(new object()));
            Assert.Contains("Count: 0", tree.ToString());
            Assert.Equal(-1, tree.IndexOf(Guid.NewGuid().ToString()));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Empty))]
        public static void TreeMap_Empty()
        {
            var noData = new TreeMap();
            AssertEmpty(noData);

            var nullData = new TreeMap(null);
            AssertEmpty(nullData);

            var collData = new TreeMap((IList<TreePair>)null);
            AssertEmpty(collData);

            var emptyData = new TreeMap(Array.Empty<TreePair>());
            AssertEmpty(emptyData);

            Assert.Empty(TreeMap.Empty);
            Assert.Equal(default, TreeMap.Empty);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Sorting))]
        public static void TreeMap_Sorting()
        {
            var nodes = new[] { Node0, Node1 };
            var tree0 = new TreeMap(nodes.OrderBy(n => n.Value.Sha1).ToArray());
            var tree1 = new TreeMap(nodes.OrderByDescending(n => n.Value.Sha1).ToList()); // ICollection<T>

            Assert.Equal(tree0[0], tree1[0]);
            Assert.Equal(tree0[1], tree1[1]);

            Assert.True(tree1[Node0.Key] == Node0.Value);
            Assert.True(tree1[Node1.Key] == Node1.Value);
            Assert.False(tree1.ContainsKey("x"));
            Assert.True(tree1.ContainsKey(Node0.Key));
            Assert.True(tree1.ContainsKey(Node1.Key));
            Assert.False(tree1.TryGetValue("x", out _));
            Assert.True(tree1.TryGetValue(Node0.Key, out var v20) && v20 == Node0.Value);
            Assert.True(tree1.TryGetValue(Node1.Key, out var v21) && v21 == Node1.Value);
            Assert.False(tree1.TryGetValue(Node0.Key, NodeKind.Blob, out _));
            Assert.True(tree1.TryGetValue(Node0.Key, Node0.Value.Kind, out _));

            nodes = new[] { Node0, Node1, Node2 };
            tree0 = new TreeMap(nodes.OrderBy(n => n.Value.Sha1).ToArray());
            tree1 = new TreeMap(nodes.OrderByDescending(n => n.Value.Sha1).ToList()); // ICollection<T>

            Assert.True(tree1[Node0.Key] == Node0.Value);
            Assert.True(tree1[Node1.Key] == Node1.Value);
            Assert.True(tree1[Node2.Key] == Node2.Value);
            Assert.False(tree1.ContainsKey("x"));
            Assert.True(tree1.ContainsKey(Node0.Key));
            Assert.True(tree1.ContainsKey(Node1.Key));
            Assert.True(tree1.ContainsKey(Node2.Key));
            Assert.False(tree1.TryGetValue("x", out _));
            Assert.True(tree1.TryGetValue(Node0.Key, out var v30) && v30 == Node0.Value);
            Assert.True(tree1.TryGetValue(Node1.Key, out var v31) && v31 == Node1.Value);
            Assert.True(tree1.TryGetValue(Node2.Key, out var v32) && v32 == Node2.Value);

            Assert.Equal(tree0[0], tree1[0]);
            Assert.Equal(tree0[1], tree1[1]);
            Assert.Equal(tree0[2], tree1[2]);

            nodes = new[] { Node0, Node1, Node2, Node3 };
            tree0 = new TreeMap(nodes.OrderBy(n => n.Value.Sha1).ToArray());
            tree1 = new TreeMap(nodes.OrderByDescending(n => n.Value.Sha1).ToList()); // ICollection<T>

            Assert.True(tree1[Node0.Key] == Node0.Value);
            Assert.True(tree1[Node1.Key] == Node1.Value);
            Assert.True(tree1[Node2.Key] == Node2.Value);
            Assert.True(tree1[Node3.Key] == Node3.Value);
            Assert.False(tree1.ContainsKey("x"));
            Assert.True(tree1.ContainsKey(Node0.Key));
            Assert.True(tree1.ContainsKey(Node1.Key));
            Assert.True(tree1.ContainsKey(Node2.Key));
            Assert.True(tree1.ContainsKey(Node3.Key));
            Assert.False(tree1.TryGetValue("x", out _));
            Assert.True(tree1.TryGetValue(Node0.Key, out var v40) && v40 == Node0.Value);
            Assert.True(tree1.TryGetValue(Node1.Key, out var v41) && v41 == Node1.Value);
            Assert.True(tree1.TryGetValue(Node2.Key, out var v42) && v42 == Node2.Value);
            Assert.True(tree1.TryGetValue(Node3.Key, out var v43) && v43 == Node3.Value);

            Assert.Equal(tree0[0], tree1[0]);
            Assert.Equal(tree0[1], tree1[1]);
            Assert.Equal(tree0[2], tree1[2]);
            Assert.Equal(tree0[3], tree1[3]);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Duplicate_Full_2))]
        public static void TreeMap_Duplicate_Full_2()
        {
            var nodes = new[] { Node0, Node0 };

            var tree = new TreeMap(nodes);
            Assert.Collection<TreeNode>(tree,
                n => Assert.Equal(n, Node0.Value));

            tree = new TreeMap(nodes.ToList()); // ICollection<T>
            Assert.Collection<TreeNode>(tree,
                n => Assert.Equal(n, Node0.Value));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Duplicate_Full_3))]
        public static void TreeMap_Duplicate_Full_3()
        {
            var nodes = new[] { Node0, Node1, Node0 }; // Shuffled

            var tree = new TreeMap(nodes);
            Assert.Collection<TreeNode>(tree,
                n => Assert.Equal(n, Node0.Value),
                n => Assert.Equal(n, Node1.Value));

            tree = new TreeMap(nodes.ToList()); // ICollection<T>
            Assert.Collection<TreeNode>(tree,
                n => Assert.Equal(n, Node0.Value),
                n => Assert.Equal(n, Node1.Value));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Duplicate_Full_2_Exception))]
        public static void TreeMap_Duplicate_Full_2_Exception()
        {
            // Arrange
            var nodes = new[] { Node0, Node0Blob }; // Shuffled

            // Action
            var ex = Assert.Throws<ArgumentException>(() => new TreeMap(nodes));

            // Assert
            Assert.Contains(Node0.Key, ex.Message);
            Assert.Contains(Node0.Value.Sha1.ToString(), ex.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Duplicate_Full_3_Exception))]
        public static void TreeMap_Duplicate_Full_3_Exception()
        {
            // Arrange
            var nodes = new[] { Node0, Node0Blob, Node1 }; // Shuffled

            // Action
            var ex = Assert.Throws<ArgumentException>(() => new TreeMap(nodes));

            // Assert
            Assert.Contains(Node0.Key, ex.Message);
            Assert.Contains(Node0.Value.Sha1.ToString(), ex.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Duplicate_Full_4))]
        public static void TreeMap_Duplicate_Full_4()
        {
            var nodes = new[] { Node0, Node2, Node1, Node0 }; // Shuffled

            var tree = new TreeMap(nodes);
            Assert.Collection<TreeNode>(tree,
                n => Assert.Equal(n, Node0.Value),
                n => Assert.Equal(n, Node1.Value),
                n => Assert.Equal(n, Node2.Value));

            tree = new TreeMap(nodes.ToList()); // ICollection<T>
            Assert.Collection<TreeNode>(tree,
                n => Assert.Equal(n, Node0.Value),
                n => Assert.Equal(n, Node1.Value),
                n => Assert.Equal(n, Node2.Value));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Duplicate_Full_N))]
        public static void TreeMap_Duplicate_Full_N()
        {
            var nodes = new[] { Node3, Node1, Node2, Node0, Node3, Node0, Node1, Node0, Node1, Node2, Node0, Node3 }; // Shuffled

            var tree = new TreeMap(nodes);
            Assert.Collection<TreeNode>(tree,
                n => Assert.Equal(n, Node0.Value),
                n => Assert.Equal(n, Node1.Value),
                n => Assert.Equal(n, Node2.Value),
                n => Assert.Equal(n, Node3.Value));

            tree = new TreeMap(nodes.ToList()); // ICollection<T>
            Assert.Collection<TreeNode>(tree,
                n => Assert.Equal(n, Node0.Value),
                n => Assert.Equal(n, Node1.Value),
                n => Assert.Equal(n, Node2.Value),
                n => Assert.Equal(n, Node3.Value));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Duplicate_Name))]
        public static void TreeMap_Duplicate_Name()
        {
            var nodes = new TreePair[] { new TreeMapId(Node1.Value.Sha1).CreateMap(Node0.Key), Node0 }; // Reversed

            Assert.Throws<ArgumentException>(() => new TreeMap(nodes));
            Assert.Throws<ArgumentException>(() => new TreeMap(nodes.ToList())); // ICollection<T>
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Duplicate_Sha1))]
        public static void TreeMap_Duplicate_Sha1()
        {
            var nodes = new[] { new TreeMapId(Node0.Value.Sha1).CreateMap(Node1.Key), Node0 }; // Reversed

            var tree0 = new TreeMap(nodes);
            Assert.Collection<TreeNode>(tree0,
                n => Assert.Equal(n, Node0.Value),
                n => Assert.Equal(n, nodes[0].Value));

            tree0 = new TreeMap(nodes.ToList()); // ICollection<T>
            Assert.Collection<TreeNode>(tree0,
                n => Assert.Equal(n, Node0.Value),
                n => Assert.Equal(n, nodes[0].Value));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Equality))]
        public static void TreeMap_Equality()
        {
            var expected = new TreeMap(new[] {
                new BlobId(Sha1.Hash("c1")).CreateMap("c1"),
                new TreeMapId(Sha1.Hash("c2")).CreateMap("c2")
            });
            var node3 = new TreeMapId(Sha1.Hash("c3")).CreateMap("c3");

            // Equal
            var actual = new TreeMap().Merge(expected);
            Assert.Equal(expected, actual);
            Assert.Equal(expected.GetHashCode(), actual.GetHashCode());
            Assert.True(actual.Equals((object)expected));
            Assert.True(expected == actual);
            Assert.False(expected != actual);

            // Less Nodes
            actual = new TreeMap().Merge(expected[0]);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());
            Assert.False(actual.Equals((object)expected));
            Assert.False(expected == actual);
            Assert.True(expected != actual);

            // More Nodes
            actual = new TreeMap().Merge(expected).Merge(node3);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());
            Assert.False(actual.Equals((object)expected));
            Assert.False(expected == actual);
            Assert.True(expected != actual);

            // Different Nodes
            actual = new TreeMap().Merge(expected[0]).Merge(node3);
            Assert.NotEqual(expected, actual); // hashcode is the same (node count)
            Assert.False(actual.Equals((object)expected));
            Assert.False(expected == actual);
            Assert.True(expected != actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_IndexOf))]
        public static void TreeMap_IndexOf()
        {
            // Arrange
            var actual = new TreeMap(new[] { Node0, Node1 });

            // Action/Assert
            Assert.Equal(-1, actual.IndexOf(null));
            Assert.True(actual.IndexOf(Guid.NewGuid().ToString()) < 0);
            Assert.Equal(0, actual.IndexOf(Node0.Key));
            Assert.Equal(1, actual.IndexOf(Node1.Key));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Merge_Empty))]
        public static void TreeMap_Merge_Empty()
        {
            var emptyTreeMap = new TreeMap();
            var node = new BlobId(Sha1.Hash("Test1")).CreateMap("b");
            var tree = new TreeMap(node);

            // TreeMap
            var merged = tree.Merge(emptyTreeMap);
            Assert.Equal(tree, merged);

            merged = emptyTreeMap.Merge(tree);
            Assert.Equal(tree, merged);

            // ICollection
            merged = tree.Merge(Array.Empty<KeyValuePair<string, TreeNode>>());
            Assert.Equal(tree, merged);

            merged = emptyTreeMap.Merge(tree);
            Assert.Equal(tree, merged);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Merge_Null))]
        public static void TreeMap_Merge_Null()
        {
            // Arrange
            var tree = new TreeMap(Node0);

            // Action
            var merged = tree.Merge((IEnumerable<KeyValuePair<string, TreeNode>>)null);

            // Assert
            Assert.Equal(tree, merged);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Merge_Single))]
        public static void TreeMap_Merge_Single()
        {
            var tree = new TreeMap();

            tree = tree.Merge(new TreeNode(NodeKind.Blob, Sha1.Hash("Test1")).CreateMap("b"));
            tree = tree.Merge(new TreeNode(NodeKind.Map, Sha1.Hash("Test2")).CreateMap("a"));
            tree = tree.Merge(new TreeNode(NodeKind.Blob, Sha1.Hash("Test3")).CreateMap("c"));
            tree = tree.Merge(new TreeNode(NodeKind.Map, Sha1.Hash("Test4")).CreateMap("d"));
            tree = tree.Merge(new TreeNode(NodeKind.Blob, Sha1.Hash("Test5")).CreateMap("g"));
            tree = tree.Merge(new TreeNode(NodeKind.Map, Sha1.Hash("Test6")).CreateMap("e"));
            tree = tree.Merge(new TreeNode(NodeKind.Blob, Sha1.Hash("Test7")).CreateMap("f"));

            var prev = tree.Keys.First();
            foreach (var cur in tree.Keys.Skip(1))
            {
                Assert.True(string.CompareOrdinal(cur, prev) > 0);
                prev = cur;
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Merge_Single_Exist))]
        public static void TreeMap_Merge_Single_Exist()
        {
            // Arrange
            var tree = new TreeMap();
            var expectedName = Guid.NewGuid().ToString();
            var expectedKind = NodeKind.Map;
            var expectedSha1 = Sha1.Hash(Guid.NewGuid().ToString());

            tree = tree.Merge(new TreeNode(NodeKind.Blob, Sha1.Hash("Test1")).CreateMap(expectedName));

            // Action
            var actual = tree.Merge(new TreeNode(expectedKind, expectedSha1).CreateMap(expectedName));
            var actualNode = actual[expectedName];

            // Assert
            Assert.Equal(expectedKind, actualNode.Kind);
            Assert.Equal(expectedSha1, actualNode.Sha1);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Merge_TreeMap))]
        public static void TreeMap_Merge_TreeMap()
        {
            var tree1 = new TreeMap();

            tree1 = tree1.Merge(new TreeNode(NodeKind.Map, Sha1.Hash("Test4")).CreateMap("d"));
            tree1 = tree1.Merge(new TreeNode(NodeKind.Map, Sha1.Hash("Test5")).CreateMap("e"));
            tree1 = tree1.Merge(new TreeNode(NodeKind.Blob, Sha1.Hash("Test6")).CreateMap("f"));
            tree1 = tree1.Merge(new TreeNode(NodeKind.Blob, Sha1.Hash("Test7")).CreateMap("g"));

            var tree2 = new TreeMap();

            tree2 = tree2.Merge(new TreeNode(NodeKind.Map, Sha1.Hash("Test1")).CreateMap("a"));
            tree2 = tree2.Merge(new TreeNode(NodeKind.Blob, Sha1.Hash("Test2")).CreateMap("b"));
            tree2 = tree2.Merge(new TreeNode(NodeKind.Blob, Sha1.Hash("Test3")).CreateMap("c"));
            tree2 = tree2.Merge(new TreeNode(NodeKind.Map, Sha1.Hash("Test4 Replace")).CreateMap("d"));
            tree2 = tree2.Merge(new TreeNode(NodeKind.Blob, Sha1.Hash("Test5 Replace")).CreateMap("g"));
            tree2 = tree2.Merge(new TreeNode(NodeKind.Map, Sha1.Hash("Test8")).CreateMap("q"));
            tree2 = tree2.Merge(new TreeNode(NodeKind.Blob, Sha1.Hash("Test9")).CreateMap("r"));

            var tree3 = tree1.Merge(tree2);

            Assert.Equal(9, tree3.Count);

            Assert.Equal("a", tree3[0].Key);
            Assert.Equal("b", tree3[1].Key);
            Assert.Equal("c", tree3[2].Key);
            Assert.Equal("d", tree3[3].Key);
            Assert.Equal("e", tree3[4].Key);
            Assert.Equal("f", tree3[5].Key);
            Assert.Equal("g", tree3[6].Key);
            Assert.Equal("q", tree3[7].Key);
            Assert.Equal("r", tree3[8].Key);

            Assert.Equal(tree2[0].Value.Sha1, tree3[0].Value.Sha1);
            Assert.Equal(tree2[1].Value.Sha1, tree3[1].Value.Sha1);
            Assert.Equal(tree2[2].Value.Sha1, tree3[2].Value.Sha1);
            Assert.Equal(tree2[3].Value.Sha1, tree3[3].Value.Sha1);
            Assert.Equal(tree1[1].Value.Sha1, tree3[4].Value.Sha1);
            Assert.Equal(tree1[2].Value.Sha1, tree3[5].Value.Sha1);
            Assert.Equal(tree2[4].Value.Sha1, tree3[6].Value.Sha1);
            Assert.Equal(tree2[5].Value.Sha1, tree3[7].Value.Sha1);
            Assert.Equal(tree2[6].Value.Sha1, tree3[8].Value.Sha1);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Merge_Collection))]
        public static void TreeMap_Merge_Collection()
        {
            var tree1 = new TreeMap();

            tree1 = tree1.Merge(new TreeNode(NodeKind.Map, Sha1.Hash("Test4")).CreateMap("d"));
            tree1 = tree1.Merge(new TreeNode(NodeKind.Map, Sha1.Hash("Test5")).CreateMap("e"));
            tree1 = tree1.Merge(new TreeNode(NodeKind.Blob, Sha1.Hash("Test6")).CreateMap("f"));
            tree1 = tree1.Merge(new TreeNode(NodeKind.Blob, Sha1.Hash("Test7")).CreateMap("g"));

            var tree2 = new[]
            {
                new TreeNode(NodeKind.Blob, Sha1.Hash("Test3")).CreateMap("c"),
                new TreeNode(NodeKind.Map, Sha1.Hash("Test1")).CreateMap("a"),
                new TreeNode(NodeKind.Blob, Sha1.Hash("Test2")).CreateMap("b"),
                new TreeNode(NodeKind.Map, Sha1.Hash("Test4 Replace")).CreateMap("d"),
                new TreeNode(NodeKind.Blob, Sha1.Hash("Test5 Replace")).CreateMap("g"),
                new TreeNode(NodeKind.Map, Sha1.Hash("Test8")).CreateMap("q"),
                new TreeNode(NodeKind.Blob, Sha1.Hash("Test9")).CreateMap("r"),
            };

            var tree3 = tree1.Merge(tree2);

            Assert.Equal(9, tree3.Count);

            Assert.Equal("a", tree3[0].Key);
            Assert.Equal("b", tree3[1].Key);
            Assert.Equal("c", tree3[2].Key);
            Assert.Equal("d", tree3[3].Key);
            Assert.Equal("e", tree3[4].Key);
            Assert.Equal("f", tree3[5].Key);
            Assert.Equal("g", tree3[6].Key);
            Assert.Equal("q", tree3[7].Key);
            Assert.Equal("r", tree3[8].Key);

            Assert.Equal(tree2[1].Value.Sha1, tree3[0].Value.Sha1);
            Assert.Equal(tree2[2].Value.Sha1, tree3[1].Value.Sha1);
            Assert.Equal(tree2[0].Value.Sha1, tree3[2].Value.Sha1);
            Assert.Equal(tree2[3].Value.Sha1, tree3[3].Value.Sha1);
            Assert.Equal(tree1[1].Value.Sha1, tree3[4].Value.Sha1);
            Assert.Equal(tree1[2].Value.Sha1, tree3[5].Value.Sha1);
            Assert.Equal(tree2[4].Value.Sha1, tree3[6].Value.Sha1);
            Assert.Equal(tree2[5].Value.Sha1, tree3[7].Value.Sha1);
            Assert.Equal(tree2[6].Value.Sha1, tree3[8].Value.Sha1);

            var dupes = new[]
            {
                new TreeNode(tree2[0].Value.Kind, tree2[1].Value.Sha1).CreateMap(tree2[0].Key),
                new TreeNode(tree2[1].Value.Kind, tree2[2].Value.Sha1).CreateMap(tree2[1].Key),
                new TreeNode(tree2[2].Value.Kind, tree2[3].Value.Sha1).CreateMap(tree2[2].Key),
                new TreeNode(tree2[3].Value.Kind, tree2[0].Value.Sha1).CreateMap(tree2[3].Key)
            };

            tree3 = tree3.Merge(dupes);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Delete))]
        public static void TreeMap_Delete()
        {
            var tree = new TreeMap
            (
                new TreeNode(NodeKind.Blob, Sha1.Hash("a")).CreateMap("a"),
                new TreeNode(NodeKind.Blob, Sha1.Hash("b")).CreateMap("b"),
                new TreeNode(NodeKind.Blob, Sha1.Hash("c")).CreateMap("c")
            );

            var removed = tree.Delete("a");
            Assert.Equal(2, removed.Count);
            Assert.Equal("b", removed[0].Key);
            Assert.Equal("c", removed[1].Key);

            removed = tree.Delete("b");
            Assert.Equal(2, removed.Count);
            Assert.Equal("a", removed[0].Key);
            Assert.Equal("c", removed[1].Key);

            removed = tree.Delete("c");
            Assert.Equal(2, removed.Count);
            Assert.Equal("a", removed[0].Key);
            Assert.Equal("b", removed[1].Key);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_Delete_Predicate))]
        public static void TreeMap_Delete_Predicate()
        {
            var tree = new TreeMap
            (
                new TreeNode(NodeKind.Blob, Sha1.Hash("a")).CreateMap("a"),
                new TreeNode(NodeKind.Blob, Sha1.Hash("b")).CreateMap("b"),
                new TreeNode(NodeKind.Blob, Sha1.Hash("c")).CreateMap("c")
            );

            var set = new HashSet<string>(StringComparer.Ordinal)
            { "a", "b", "d" };

            var removed = tree.Delete(set.Contains);
            Assert.Single(removed);
            Assert.Equal("c", removed[0].Key);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_IReadOnlyDictionary_Empty_GetEnumerator))]
        public static void TreeMap_IReadOnlyDictionary_Empty_GetEnumerator()
        {
            // Arrange
            var tree = new TreeMap();
            var readOnlyDictionary = tree as IReadOnlyDictionary<string, TreeNode>;

            // Action
            var enumerator = readOnlyDictionary.GetEnumerator();

            // Assert
            Assert.False(enumerator.MoveNext());

            var current = enumerator.Current;
            Assert.Null(current.Key);
            Assert.Equal(default, current.Value);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMap_IReadOnlyDictionary_GetEnumerator))]
        public static void TreeMap_IReadOnlyDictionary_GetEnumerator()
        {
            // Arrange
            var nodes = new[] { Node0, Node1 };
            var tree = new TreeMap(nodes);
            var readOnlyDictionary = tree as IReadOnlyDictionary<string, TreeNode>;

            // Action
            var enumerator = readOnlyDictionary.GetEnumerator();

            // Assert
            Assert.True(enumerator.MoveNext());
            Assert.Equal(Node0.Value, enumerator.Current.Value);

            Assert.True(enumerator.MoveNext());
            Assert.Equal(Node1.Value, enumerator.Current.Value);

            Assert.False(enumerator.MoveNext());
            Assert.Equal(Node1.Value, enumerator.Current.Value);
        }

        #endregion
    }
}
