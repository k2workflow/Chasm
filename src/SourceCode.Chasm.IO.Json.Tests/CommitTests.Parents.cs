using System;
using System.Linq;
using Xunit;

namespace SourceCode.Chasm.IO.Json.Tests
{
    public static partial class CommitTests // .Parents
    {
        private static readonly CommitId Parent1 = new CommitId(Sha1.Hash(nameof(Parent1)));
        private static readonly CommitId Parent2 = new CommitId(Sha1.Hash(nameof(Parent2)));
        private static readonly CommitId Parent3 = new CommitId(Sha1.Hash(nameof(Parent3)));

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Commit_Parents_Null_Parent_Array))]
        public static void JsonChasmSerializer_Roundtrip_Commit_Parents_Null_Parent_Array()
        {
            var ser = new JsonChasmSerializer();

            // Force Commit to be non-default
            var expected = new Commit(null, default, default, "force");
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Commit_Parents_Empty_Parent_Array))]
        public static void JsonChasmSerializer_Roundtrip_Commit_Parents_Empty_Parent_Array()
        {
            var ser = new JsonChasmSerializer();

            var expected = new Commit(Array.Empty<CommitId>(), default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Commit_Parents_1_Parent_Empty))]
        public static void JsonChasmSerializer_Roundtrip_Commit_Parents_1_Parent_Empty()
        {
            var ser = new JsonChasmSerializer();

            var expected = new Commit(CommitId.Empty, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Commit_Parents_1_Parent))]
        public static void JsonChasmSerializer_Roundtrip_Commit_Parents_1_Parent()
        {
            var ser = new JsonChasmSerializer();

            var expected = new Commit(Parent1, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Commit_Parents_2_Parents_Empty_Duplicated))]
        public static void JsonChasmSerializer_Roundtrip_Commit_Parents_2_Parents_Empty_Duplicated()
        {
            var ser = new JsonChasmSerializer();

            var expected = new Commit(new[] { CommitId.Empty, CommitId.Empty }, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(1, actual.Parents.Count);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Commit_Parents_2_Parents_Duplicated))]
        public static void JsonChasmSerializer_Roundtrip_Commit_Parents_2_Parents_Duplicated()
        {
            var ser = new JsonChasmSerializer();

            var expected = new Commit(new[] { Parent1, Parent1 }, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.NotEqual(expected, actual);
                Assert.Equal(1, actual.Parents.Count);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Commit_Parents_2_Parents_Sorted))]
        public static void JsonChasmSerializer_Roundtrip_Commit_Parents_2_Parents_Sorted()
        {
            var ser = new JsonChasmSerializer();

            // Forward
            var parents = new[] { Parent1, Parent2 }.OrderBy(n => n.Sha1).ToArray();

            var expected = new Commit(parents, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }

            // Reversed
            Array.Reverse(parents);

            expected = new Commit(parents, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Commit_Parents_3_Parents_Empty_Duplicated))]
        public static void JsonChasmSerializer_Roundtrip_Commit_Parents_3_Parents_Empty_Duplicated()
        {
            var ser = new JsonChasmSerializer();

            var expected = new Commit(new[] { CommitId.Empty, CommitId.Empty, CommitId.Empty }, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(1, actual.Parents.Count);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Commit_Parents_3_Parents_Duplicated))]
        public static void JsonChasmSerializer_Roundtrip_Commit_Parents_3_Parents_Duplicated()
        {
            var ser = new JsonChasmSerializer();

            // 3 duplicates
            var expected = new Commit(new[] { Parent1, Parent1, Parent1 }, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.NotEqual(expected, actual);
                Assert.Equal(1, actual.Parents.Count);
            }

            // 2 duplicates
            expected = new Commit(new[] { Parent1, Parent2, Parent1 }, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.NotEqual(expected, actual);
                Assert.Equal(2, actual.Parents.Count);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(JsonChasmSerializer_Roundtrip_Commit_Parents_3_Parents_Sorted))]
        public static void JsonChasmSerializer_Roundtrip_Commit_Parents_3_Parents_Sorted()
        {
            var ser = new JsonChasmSerializer();

            // Forward
            var parents = new[] { Parent1, Parent2, Parent3 }.OrderBy(n => n.Sha1).ToArray();

            var expected = new Commit(parents, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }

            // Reversed
            Array.Reverse(parents);

            expected = new Commit(parents, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }
    }
}
