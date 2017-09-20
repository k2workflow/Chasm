using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class CommitIdTests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(CommitId_has_empty_sha1))]
        public static void CommitId_has_empty_sha1()
        {
            Assert.Equal(Sha1.Empty, CommitId.Empty.Sha1);
            Assert.Equal(default, CommitId.Empty);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(CommitId_equality))]
        public static void CommitId_equality()
        {
            var commitId1 = new CommitId(Sha1.Hash("abc"));
            var commitId2 = new CommitId(Sha1.Hash("abc"));
            var commitId3 = new CommitId(Sha1.Hash("def"));

            Assert.Equal(commitId1, commitId2);
            Assert.NotEqual(CommitId.Empty, commitId1);
            Assert.NotEqual(commitId3, commitId1);
        }
    }
}
