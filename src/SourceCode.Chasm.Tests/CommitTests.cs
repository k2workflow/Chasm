using System;
using System.Linq;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class CommitTests
    {
        private static readonly CommitId Parent1 = new CommitId(Sha1.Hash(nameof(Parent1)));
        private static readonly CommitId Parent2 = new CommitId(Sha1.Hash(nameof(Parent2)));
        private static readonly CommitId Parent3 = new CommitId(Sha1.Hash(nameof(Parent3)));

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Empty))]
        public static void Commit_Empty()
        {
            var noData = new Commit();
            var nullData = new Commit(null, default, default, null);

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
            Assert.Throws<ArgumentOutOfRangeException>(() => new Commit(null, TreeId.Empty, DateTime.Now, null)); // Non-Utc

            // Message
            Assert.Null(Commit.Empty.CommitMessage);
            Assert.Null(noData.CommitMessage);
            Assert.Null(nullData.CommitMessage);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Equality))]
        public static void Commit_Equality()
        {
            var expected = new Commit(new[] { new CommitId(Sha1.Hash("c1")), new CommitId(Sha1.Hash("c2")) }, new TreeId(Sha1.Hash("abc")), DateTime.UtcNow, "hello");

            // Equal
            var actual = new Commit(expected.Parents, expected.TreeId, expected.CommitUtc, expected.CommitMessage);
            Assert.Equal(expected, actual);
            Assert.Equal(expected.GetHashCode(), actual.GetHashCode());

            // Parents
            actual = new Commit(null, expected.TreeId, expected.CommitUtc, expected.CommitMessage);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(Array.Empty<CommitId>(), expected.TreeId, expected.CommitUtc, expected.CommitMessage.ToUpperInvariant());
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(Commit.Orphaned, expected.TreeId, expected.CommitUtc, expected.CommitMessage.ToUpperInvariant());
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(new[] { expected.Parents[0] }, expected.TreeId, expected.CommitUtc, expected.CommitMessage.ToUpperInvariant());
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(new[] { expected.Parents[0], expected.Parents[1], new CommitId(Sha1.Hash("c3")) }, expected.TreeId, expected.CommitUtc, expected.CommitMessage.ToUpperInvariant());
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            // TreeId
            actual = new Commit(expected.Parents, TreeId.Empty, expected.CommitUtc, expected.CommitMessage);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(expected.Parents, new TreeId(Sha1.Hash("def")), expected.CommitUtc, expected.CommitMessage);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            // DateTime
            actual = new Commit(expected.Parents, expected.TreeId, default, expected.CommitMessage);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(expected.Parents, expected.TreeId, DateTime.MaxValue.ToUniversalTime(), expected.CommitMessage);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(expected.Parents, expected.TreeId, expected.CommitUtc.AddTicks(1), expected.CommitMessage);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            // Message
            actual = new Commit(expected.Parents, expected.TreeId, expected.CommitUtc, null);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(expected.Parents, expected.TreeId, expected.CommitUtc, expected.CommitMessage.ToUpperInvariant());
            Assert.NotEqual(expected, actual); // hashcode is the same for upper/lower string
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_Null))]
        public static void Commit_Parents_Null()
        {
            // Force Commit to be non-default
            var actual = new Commit(null, default, default, null);
            Assert.Null(actual.Parents);

            // Force Commit to be non-default
            actual = new Commit(null, default, default, "force");
            Assert.Null(actual.Parents);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_Empty))]
        public static void Commit_Parents_Empty()
        {
            var actual = new Commit(Array.Empty<CommitId>(), default, default, null);
            Assert.Empty(actual.Parents);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_1_Empty))]
        public static void Commit_Parents_1_Empty()
        {
            var actual = new Commit(CommitId.Empty, default, default, null);
            Assert.Equal(CommitId.Empty, actual.Parents[0]);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_1))]
        public static void Commit_Parents_1()
        {
            var actual = new Commit(Parent1, default, default, null);
            Assert.Equal(Parent1, actual.Parents[0]);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_2_Empty_Duplicated))]
        public static void Commit_Parents_2_Empty_Duplicated()
        {
            var actual = new Commit(new[] { CommitId.Empty, CommitId.Empty }, default, default, null);
            Assert.Equal(1, actual.Parents.Count);
            Assert.Equal(CommitId.Empty, actual.Parents[0]);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_2_Duplicated))]
        public static void Commit_Parents_2_Duplicated()
        {
            var actual = new Commit(new[] { Parent1, Parent1 }, default, default, null);
            Assert.Equal(1, actual.Parents.Count);
            Assert.Equal(Parent1, actual.Parents[0]);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_2_Sorted))]
        public static void Commit_Parents_2_Sorted()
        {
            // Forward
            var parents = new[] { Parent1, Parent2 }.OrderByDescending(n => n.Sha1).ToArray();

            var actual = new Commit(parents, default, default, null);
            Assert.Equal(2, actual.Parents.Count);

            // Reversed
            Array.Reverse(parents);

            var actual2 = new Commit(parents, default, default, null);
            Assert.Equal(actual, actual2);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_3_Empty_Duplicated))]
        public static void Commit_Parents_3_Empty_Duplicated()
        {
            var actual = new Commit(new[] { CommitId.Empty, CommitId.Empty, CommitId.Empty }, default, default, null);
            Assert.Equal(1, actual.Parents.Count);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_3_Duplicated))]
        public static void Commit_Parents_3_Duplicated()
        {
            // 3 duplicates
            var actual = new Commit(new[] { Parent1, Parent1, Parent1 }, default, default, null);
            Assert.Equal(1, actual.Parents.Count);

            // 2 duplicates
            actual = new Commit(new[] { Parent1, Parent2, Parent1 }, default, default, null);
            Assert.Equal(2, actual.Parents.Count);

            // 2x2 duplicates
            actual = new Commit(new[] { Parent1, Parent2, Parent1, Parent2 }, default, default, null);
            Assert.Equal(2, actual.Parents.Count);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_3_Sorted))]
        public static void Commit_Parents_3_Sorted()
        {
            // Forward
            var parents = new[] { Parent1, Parent2, Parent3 }.OrderByDescending(n => n.Sha1).ToArray();

            var actual = new Commit(parents, default, default, null);
            Assert.Equal(3, actual.Parents.Count);

            // Reversed
            Array.Reverse(parents);

            var actual2 = new Commit(parents, default, default, null);
            Assert.Equal(actual, actual2);
        }
    }
}
