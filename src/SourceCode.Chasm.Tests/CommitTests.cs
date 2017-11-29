#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay;
using System;
using System.Linq;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class CommitTests
    {
        #region Constants

        private static readonly CommitId Parent1 = new CommitId(Sha1.Hash(nameof(Parent1)));
        private static readonly CommitId Parent2 = new CommitId(Sha1.Hash(nameof(Parent2)));
        private static readonly CommitId Parent3 = new CommitId(Sha1.Hash(nameof(Parent3)));

        #endregion

        #region Methods

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Empty))]
        public static void Commit_Empty()
        {
            var noData = new Commit();
            var nullData = new Commit(new CommitId?(), default, default, default, null);

            Assert.True(default == Commit.Empty);
            Assert.False(default != Commit.Empty);
            Assert.True(Commit.Empty.Equals((object)Commit.Empty));

            // Parents
            Assert.Empty(Commit.Empty.Parents); // Default ctor (implicit)
            Assert.Empty(noData.Parents); // Default ctor (explicit)
            Assert.Empty(nullData.Parents); // Custom ctor (with null)

            // TreeId
            Assert.Equal(default, Commit.Empty.TreeId);
            Assert.Equal(default, noData.TreeId);
            Assert.Equal(default, nullData.TreeId);

            // DateTime
            Assert.Equal(default, Commit.Empty.Author.Timestamp);
            Assert.Equal(default, noData.Author.Timestamp);
            Assert.Equal(default, nullData.Author.Timestamp);

            // Message
            Assert.Equal(string.Empty, Commit.Empty.Message);
            Assert.Equal(string.Empty, noData.Message);
            Assert.Equal(string.Empty, nullData.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Equality))]
        public static void Commit_Equality()
        {
            var expected = new Commit(new[] { new CommitId(Sha1.Hash("c1")), new CommitId(Sha1.Hash("c2")) }, new TreeId(Sha1.Hash("abc")), new Audit("bob", DateTimeOffset.Now), new Audit("mary", DateTimeOffset.Now), "hello");

            // Equal
            var actual = new Commit(expected.Parents, expected.TreeId, expected.Author, expected.Committer, expected.Message);
            Assert.Equal(expected, actual);
            Assert.Equal(expected.ToString(), actual.ToString());
            Assert.Equal(expected.GetHashCode(), actual.GetHashCode());

            // Parents
            actual = new Commit(new CommitId?(), expected.TreeId, expected.Author, expected.Committer, expected.Message);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(Array.Empty<CommitId>(), expected.TreeId, expected.Author, expected.Committer, expected.Message.ToUpperInvariant());
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(Commit.Orphaned, expected.TreeId, expected.Author, expected.Committer, expected.Message.ToUpperInvariant());
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(new[] { expected.Parents[0] }, expected.TreeId, expected.Author, expected.Committer, expected.Message.ToUpperInvariant());
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(new[] { expected.Parents[0], expected.Parents[1], new CommitId(Sha1.Hash("c3")) }, expected.TreeId, expected.Author, expected.Committer, expected.Message.ToUpperInvariant());
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            // TreeId
            actual = new Commit(expected.Parents, default, expected.Author, expected.Committer, expected.Message);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(expected.Parents, new TreeId(Sha1.Hash("def")), expected.Author, expected.Committer, expected.Message);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            // DateTime
            actual = new Commit(expected.Parents, expected.TreeId, default, default, expected.Message);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(expected.Parents, expected.TreeId, new Audit("bob", DateTimeOffset.MaxValue), new Audit("mary", DateTimeOffset.MaxValue), expected.Message);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(expected.Parents, expected.TreeId, new Audit(expected.Author.Name, expected.Author.Timestamp.AddTicks(1)), new Audit(expected.Committer.Name, expected.Committer.Timestamp.AddTicks(1)), expected.Message);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            // Message
            actual = new Commit(expected.Parents, expected.TreeId, expected.Author, expected.Committer, null);
            Assert.NotEqual(expected, actual);
            Assert.NotEqual(expected.GetHashCode(), actual.GetHashCode());

            actual = new Commit(expected.Parents, expected.TreeId, expected.Author, expected.Committer, expected.Message.ToUpperInvariant());
            Assert.NotEqual(expected, actual); // hashcode is the same for upper/lower string
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_Null))]
        public static void Commit_Parents_Null()
        {
            // Force Commit to be non-default
            var actual = new Commit(new CommitId?(), default, default, default, "force");
            Assert.Empty(actual.Parents);

            // Force Commit to be non-default
            actual = new Commit(new CommitId?(), default, default, default, "force");
            Assert.Empty(actual.Parents);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_Empty))]
        public static void Commit_Parents_Empty()
        {
            var actual = new Commit(Array.Empty<CommitId>(), default, default, default, null);
            Assert.Empty(actual.Parents);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_1_Empty))]
        public static void Commit_Parents_1_Empty()
        {
            var actual = new Commit(new CommitId(), default, default, default, null);
            Assert.Collection(actual.Parents, n => Assert.Equal(default, n));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_1))]
        public static void Commit_Parents_1()
        {
            var actual = new Commit(Parent2, default, default, default, null);
            Assert.Collection(actual.Parents, n => Assert.Equal(n, Parent2));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_2_Empty_Duplicated))]
        public static void Commit_Parents_2_Empty_Duplicated()
        {
            var actual = new Commit(new[] { new CommitId(), new CommitId() }, default, default, default, null);
            Assert.Collection(actual.Parents, n => Assert.Equal(n, new CommitId()));
            Assert.Equal(new CommitId(), actual.Parents[0]);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_2_Duplicated))]
        public static void Commit_Parents_2_Duplicated()
        {
            var actual = new Commit(new[] { Parent2, Parent2 }, default, default, default, null);
            Assert.Collection(actual.Parents, n => Assert.Equal(n, Parent2));
            Assert.Equal(Parent2, actual.Parents[0]);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_2_Sorted))]
        public static void Commit_Parents_2_Sorted()
        {
            // Forward
            var parents = new[] { Parent1, Parent2 }.OrderByDescending(n => n.Sha1).ToArray();

            var actual = new Commit(parents, default, default, default, null);
            Assert.Collection(actual.Parents, n => Assert.Equal(n, Parent1), n => Assert.Equal(n, Parent2));

            // Reversed
            Array.Reverse(parents);

            var actual2 = new Commit(parents, default, default, default, null);
            Assert.Collection(actual.Parents, n => Assert.Equal(n, Parent1), n => Assert.Equal(n, Parent2));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_3_Empty_Duplicated))]
        public static void Commit_Parents_3_Empty_Duplicated()
        {
            var actual = new Commit(new[] { new CommitId(), new CommitId(), new CommitId() }, default, default, default, null);
            Assert.Collection(actual.Parents, n => Assert.Equal(default, n));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_3_Duplicated))]
        public static void Commit_Parents_3_Duplicated()
        {
            // 3 duplicates
            var actual = new Commit(new[] { Parent1, Parent1, Parent1 }, default, default, default, null);
            Assert.Collection(actual.Parents, n => Assert.Equal(n, Parent1));

            // 2 duplicates
            actual = new Commit(new[] { Parent1, Parent2, Parent1 }, default, default, default, null);
            Assert.Collection(actual.Parents, n => Assert.Equal(n, Parent1), n => Assert.Equal(n, Parent2));

            // 2x2 duplicates
            actual = new Commit(new[] { Parent1, Parent2, Parent1, Parent2 }, default, default, default, null);
            Assert.Collection(actual.Parents, n => Assert.Equal(n, Parent1), n => Assert.Equal(n, Parent2));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_3_Sorted))]
        public static void Commit_Parents_3_Sorted()
        {
            // Forward
            var parents = new[] { Parent1, Parent2, Parent3 }.OrderByDescending(n => n.Sha1).ToArray();

            var actual = new Commit(parents, default, default, default, null);
            Assert.Collection(actual.Parents, n => Assert.Equal(n, Parent3), n => Assert.Equal(n, Parent1), n => Assert.Equal(n, Parent2));

            // Reversed
            Array.Reverse(parents);

            var actual2 = new Commit(parents, default, default, default, null);
            Assert.Collection(actual2.Parents, n => Assert.Equal(n, Parent3), n => Assert.Equal(n, Parent1), n => Assert.Equal(n, Parent2));
            Assert.Equal(actual, actual2);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Parents_N_Duplicated))]
        public static void Commit_Parents_N_Duplicated()
        {
            // Forward
            var parents = new[] { Parent2, Parent1, Parent3, Parent2, Parent3, Parent1, Parent2, Parent3, Parent3, Parent1 };

            var actual = new Commit(parents, default, default, default, null);
            Assert.Collection(actual.Parents, n => Assert.Equal(n, Parent3), n => Assert.Equal(n, Parent1), n => Assert.Equal(n, Parent2));

            // Reversed
            Array.Reverse(parents);

            var actual2 = new Commit(parents, default, default, default, null);
            Assert.Collection(actual2.Parents, n => Assert.Equal(n, Parent3), n => Assert.Equal(n, Parent1), n => Assert.Equal(n, Parent2));
            Assert.Equal(actual, actual2);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Commit_Deconstruct))]
        public static void Commit_Deconstruct()
        {
            var expected = new Commit(new[] { new CommitId(Sha1.Hash("c1")), new CommitId(Sha1.Hash("c2")) }, new TreeId(Sha1.Hash("abc")), Audit.Empty, Audit.Empty, "hello");

            var (parents, treeId, author, committer, message) = expected;
            var actual = new Commit(parents, treeId, author, committer, message);

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
