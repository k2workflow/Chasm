using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class CommitIRefTests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(CommitRef_has_empty_commitId))]
        public static void CommitRef_has_empty_commitId()
        {
            Assert.Equal(CommitId.Empty, CommitRef.Empty.CommitId);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(CommitRef_equality))]
        public static void CommitRef_equality()
        {
            var commitRef1 = new CommitRef(new CommitId(Sha1.Hash("abc")));
            var commitRef2 = new CommitRef(new CommitId(Sha1.Hash("abc")));
            var commitRef3 = new CommitRef(new CommitId(Sha1.Hash("def")));

            Assert.Equal(commitRef1, commitRef2);
            Assert.NotEqual(CommitRef.Empty, commitRef1);
            Assert.NotEqual(commitRef3, commitRef1);
        }
    }
}
