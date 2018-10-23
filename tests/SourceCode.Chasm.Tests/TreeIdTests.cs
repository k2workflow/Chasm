using SourceCode.Clay;
using System;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class TreeIdTests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeId_equality))]
        public static void TreeId_equality()
        {
            var treeId1 = new TreeId(Sha1.Hash("abc"));
            var treeId2 = new TreeId(Sha1.Hash("abc"));
            var treeId3 = new TreeId(Sha1.Hash("def"));

            Assert.True(treeId1 == treeId2);
            Assert.False(treeId1 != treeId2);
            Assert.True(treeId1.Equals((object)treeId2));

            Assert.Equal(treeId1.Sha1.ToString(), treeId1.ToString());
            Assert.Equal(treeId2.Sha1.ToString(), treeId2.ToString());
            Assert.Equal(treeId3.Sha1.ToString(), treeId3.ToString());

            Assert.Equal(treeId1, treeId2);
            Assert.Equal(treeId1.GetHashCode(), treeId2.GetHashCode());
            Assert.Equal(treeId1.ToString(), treeId2.ToString());

            Assert.NotEqual(treeId3, treeId1);
            Assert.NotEqual(treeId3.GetHashCode(), treeId1.GetHashCode());
            Assert.NotEqual(treeId3.ToString(), treeId1.ToString());
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeId_Compare))]
        public static void TreeId_Compare()
        {
            TreeIdComparer comparer = TreeIdComparer.Default;

            var treeId1 = new TreeId(Sha1.Hash("abc"));
            var treeId2 = new TreeId(Sha1.Hash("abc"));
            var treeId3 = new TreeId(Sha1.Hash("def"));
            TreeId[] list = new[] { treeId1, treeId2, treeId3 };

            Assert.True(treeId1.CompareTo(treeId2) == 0);
            Assert.True(treeId1.CompareTo(treeId3) != 0);

            Array.Sort(list, comparer.Compare);

            Assert.True(list[0] <= list[1]);
            Assert.True(list[2] >= list[1]);
        }
    }
}
