#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class TreeMapIdTests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMapId_equality))]
        public static void TreeMapId_equality()
        {
            var treeMapId1 = new TreeMapId(Sha1.Hash("abc"));
            var treeMapId2 = new TreeMapId(Sha1.Hash("abc"));
            var treeMapId3 = new TreeMapId(Sha1.Hash("def"));

            Assert.True(treeMapId1 == treeMapId2);
            Assert.False(treeMapId1 != treeMapId2);
            Assert.True(treeMapId1.Equals((object)treeMapId2));

            Assert.Equal(treeMapId1.Sha1.ToString(), treeMapId1.ToString());
            Assert.Equal(treeMapId2.Sha1.ToString(), treeMapId2.ToString());
            Assert.Equal(treeMapId3.Sha1.ToString(), treeMapId3.ToString());

            Assert.Equal(treeMapId1, treeMapId2);
            Assert.Equal(treeMapId1.GetHashCode(), treeMapId2.GetHashCode());
            Assert.Equal(treeMapId1.ToString(), treeMapId2.ToString());

            Assert.NotEqual(treeMapId3, treeMapId1);
            Assert.NotEqual(treeMapId3.GetHashCode(), treeMapId1.GetHashCode());
            Assert.NotEqual(treeMapId3.ToString(), treeMapId1.ToString());
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeMapId_Compare))]
        public static void TreeMapId_Compare()
        {
            var comparer = TreeMapIdComparer.Default;

            var treeMapId1 = new TreeMapId(Sha1.Hash("abc"));
            var treeMapId2 = new TreeMapId(Sha1.Hash("abc"));
            var treeMapId3 = new TreeMapId(Sha1.Hash("def"));
            var list = new[] { treeMapId1, treeMapId2, treeMapId3 };

            Assert.True(treeMapId1.CompareTo(treeMapId2) == 0);
            Assert.True(treeMapId1.CompareTo(treeMapId3) != 0);

            Array.Sort(list, comparer.Compare);

            Assert.True(list[0] <= list[1]);
            Assert.True(list[2] >= list[1]);
        }

        #endregion
    }
}
