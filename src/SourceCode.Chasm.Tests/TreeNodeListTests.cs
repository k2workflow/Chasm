using System;
using System.Linq;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class TreeNodeListTests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeNodeList_Add_Unsorted))]
        public static void TreeNodeList_Add_Unsorted()
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
    }
}
