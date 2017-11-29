#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class CommitIRefTests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(CommitRef_equality))]
        public static void CommitRef_equality()
        {
            var commitRef1 = new CommitRef("abc", new CommitId(Sha1.Hash("abc")));
            var commitRef2 = new CommitRef("abc", new CommitId(Sha1.Hash("abc")));
            var commitRef3 = new CommitRef("def", new CommitId(Sha1.Hash("def")));

            Assert.True(commitRef1 == commitRef2);
            Assert.False(commitRef1 != commitRef2);
            Assert.True(commitRef1.Equals((object)commitRef2));

            Assert.Equal(commitRef1, commitRef2);
            Assert.Equal(commitRef1.GetHashCode(), commitRef2.GetHashCode());
            Assert.Equal(commitRef1.ToString(), commitRef2.ToString());

            Assert.NotEqual(CommitRef.Empty, commitRef1);
            Assert.NotEqual(CommitRef.Empty.GetHashCode(), commitRef1.GetHashCode());

            Assert.NotEqual(commitRef3, commitRef1);
            Assert.NotEqual(commitRef3.GetHashCode(), commitRef1.GetHashCode());
            Assert.NotEqual(commitRef3.ToString(), commitRef1.ToString());
        }

        #endregion
    }
}
