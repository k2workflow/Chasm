#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class TreeIdTests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeId_has_empty_sha1))]
        public static void TreeId_has_empty_sha1()
        {
            Assert.Equal(Sha1.Empty, TreeId.Empty.Sha1);
            Assert.Equal(default, TreeId.Empty);
            Assert.Equal(Sha1.Empty.ToString(), TreeId.Empty.ToString());
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(TreeId_equality))]
        public static void TreeId_equality()
        {
            var treeId1 = new TreeId(Sha1.Hash("abc"));
            var treeId2 = new TreeId(Sha1.Hash("abc"));
            var treeId3 = new TreeId(Sha1.Hash("def"));

            Assert.Equal(treeId1.Sha1.ToString(), treeId1.ToString());
            Assert.Equal(treeId2.Sha1.ToString(), treeId2.ToString());
            Assert.Equal(treeId3.Sha1.ToString(), treeId3.ToString());

            Assert.Equal(treeId1, treeId2);
            Assert.Equal(treeId1.GetHashCode(), treeId2.GetHashCode());
            Assert.Equal(treeId1.ToString(), treeId2.ToString());

            Assert.NotEqual(TreeId.Empty, treeId1);
            Assert.NotEqual(TreeId.Empty.GetHashCode(), treeId1.GetHashCode());
            Assert.NotEqual(TreeId.Empty.ToString(), treeId1.ToString());

            Assert.NotEqual(treeId3, treeId1);
            Assert.NotEqual(treeId3.GetHashCode(), treeId1.GetHashCode());
            Assert.NotEqual(treeId3.ToString(), treeId1.ToString());
        }

        #endregion
    }
}
