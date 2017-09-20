using System;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class CommitTests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_is_empty))]
        public static void Commit_is_empty()
        {
            var noData = new Commit();
            var nullData = new Commit(null, TreeId.Empty, default, null);

            // Parents
            Assert.Null(Commit.Empty.Parents);
            Assert.Null(noData.Parents);
            Assert.Null(nullData.Parents);

            // TreeId
            Assert.Equal(TreeId.Empty, Commit.Empty.TreeId);
            Assert.Equal(TreeId.Empty, noData.TreeId);
            Assert.Equal(TreeId.Empty, nullData.TreeId);

            // DateTime
            Assert.Equal(default, Commit.Empty.CommitUtc);
            Assert.Equal(default, noData.CommitUtc);
            Assert.Equal(default, nullData.CommitUtc);
            Assert.Throws<ArgumentOutOfRangeException>(() => new Commit(null, TreeId.Empty, DateTime.Now, null));

            // Message
            Assert.Null(Commit.Empty.CommitMessage);
            Assert.Null(noData.CommitMessage);
            Assert.Null(nullData.CommitMessage);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_equality))]
        public static void Commit_equality()
        {
            var expected = new Commit(new[] { new CommitId(Sha1.Hash("c1")), new CommitId(Sha1.Hash("c2")) }, new TreeId(Sha1.Hash("abc")), DateTime.UtcNow, "hello");

            // Equal
            var actual = new Commit(expected.Parents, expected.TreeId, expected.CommitUtc, expected.CommitMessage);
            Assert.Equal(expected, actual);

            // Parents
            actual = new Commit(null, expected.TreeId, expected.CommitUtc, expected.CommitMessage);
            Assert.NotEqual(expected, actual);

            actual = new Commit(Array.Empty<CommitId>(), expected.TreeId, expected.CommitUtc, expected.CommitMessage.ToUpperInvariant());
            Assert.NotEqual(expected, actual);

            actual = new Commit(Commit.Orphaned, expected.TreeId, expected.CommitUtc, expected.CommitMessage.ToUpperInvariant());
            Assert.NotEqual(expected, actual);

            actual = new Commit(new[] { expected.Parents[0] }, expected.TreeId, expected.CommitUtc, expected.CommitMessage.ToUpperInvariant());
            Assert.NotEqual(expected, actual);

            actual = new Commit(new[] { expected.Parents[0], expected.Parents[1], new CommitId(Sha1.Hash("c3")) }, expected.TreeId, expected.CommitUtc, expected.CommitMessage.ToUpperInvariant());
            Assert.NotEqual(expected, actual);

            // TreeId
            actual = new Commit(expected.Parents, TreeId.Empty, expected.CommitUtc, expected.CommitMessage);
            Assert.NotEqual(expected, actual);

            actual = new Commit(expected.Parents, new TreeId(Sha1.Hash("def")), expected.CommitUtc, expected.CommitMessage);
            Assert.NotEqual(expected, actual);

            // DateTime
            actual = new Commit(expected.Parents, expected.TreeId, default, expected.CommitMessage);
            Assert.NotEqual(expected, actual);

            actual = new Commit(expected.Parents, expected.TreeId, DateTime.MaxValue.ToUniversalTime(), expected.CommitMessage);
            Assert.NotEqual(expected, actual);

            actual = new Commit(expected.Parents, expected.TreeId, expected.CommitUtc.AddTicks(1), expected.CommitMessage);
            Assert.NotEqual(expected, actual);

            // Message
            actual = new Commit(expected.Parents, expected.TreeId, expected.CommitUtc, null);
            Assert.NotEqual(expected, actual);

            actual = new Commit(expected.Parents, expected.TreeId, expected.CommitUtc, expected.CommitMessage.ToUpperInvariant());
            Assert.NotEqual(expected, actual);
        }
    }
}
