#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class CommitIdTests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(CommitId_has_empty_sha1))]
        public static void CommitId_has_empty_sha1()
        {
            Assert.Equal(Sha1.Empty, CommitId.Empty.Sha1);
            Assert.Equal(Sha1.Empty.ToString(), CommitId.Empty.ToString());
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(CommitId_equality))]
        public static void CommitId_equality()
        {
            var commitId1 = new CommitId(Sha1.Hash("abc"));
            var commitId2 = new CommitId(Sha1.Hash("abc"));
            var commitId3 = new CommitId(Sha1.Hash("def"));

            Assert.True(commitId1 == commitId2);
            Assert.False(commitId1 != commitId2);
            Assert.True(commitId1.Equals((object)commitId2));

            Assert.Equal(commitId1.Sha1.ToString(), commitId1.ToString());
            Assert.Equal(commitId2.Sha1.ToString(), commitId2.ToString());
            Assert.Equal(commitId3.Sha1.ToString(), commitId3.ToString());

            Assert.Equal(commitId1, commitId2);
            Assert.Equal(commitId1.GetHashCode(), commitId2.GetHashCode());
            Assert.Equal(commitId1.ToString(), commitId2.ToString());

            Assert.NotEqual(CommitId.Empty, commitId1);
            Assert.NotEqual(CommitId.Empty.GetHashCode(), commitId1.GetHashCode());
            Assert.NotEqual(CommitId.Empty.ToString(), commitId1.ToString());

            Assert.NotEqual(commitId3, commitId1);
            Assert.NotEqual(commitId3.GetHashCode(), commitId1.GetHashCode());
            Assert.NotEqual(commitId3.ToString(), commitId1.ToString());
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(CommitId_Compare))]
        public static void CommitId_Compare()
        {
            var comparer = CommitIdComparer.Default;

            var commitId1 = new CommitId(Sha1.Hash("abc"));
            var commitId2 = new CommitId(Sha1.Hash("abc"));
            var commitId3 = new CommitId(Sha1.Hash("def"));
            var list = new[] { commitId1, commitId2, commitId3 };

            Assert.True(CommitId.Empty < commitId1);
            Assert.True(commitId1 > CommitId.Empty);

            Assert.True(commitId1.CompareTo(commitId2) == 0);
            Assert.True(commitId1.CompareTo(commitId3) != 0);

            Array.Sort(list, comparer.Compare);

            Assert.True(list[0] <= list[1]);
            Assert.True(list[2] >= list[1]);
        }

        #endregion
    }
}
