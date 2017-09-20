using System;
using System.Linq;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class TreeNodeListTests
    {
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
