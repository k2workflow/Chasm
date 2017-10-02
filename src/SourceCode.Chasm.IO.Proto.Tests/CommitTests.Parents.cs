using System;
using Xunit;

namespace SourceCode.Chasm.IO.Proto.Tests
{
    public static partial class CommitTests // .Parents
    {
        private static readonly CommitId Parent1 = new CommitId(Sha1.Hash(nameof(Parent1)));
        private static readonly CommitId Parent2 = new CommitId(Sha1.Hash(nameof(Parent2)));
        private static readonly CommitId Parent3 = new CommitId(Sha1.Hash(nameof(Parent3)));

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoChasmSerializer_Roundtrip_Commit_Parents_Null))]
        public static void ProtoChasmSerializer_Roundtrip_Commit_Parents_Null()
        {
            var ser = new ProtoChasmSerializer();

            // Force Commit to be non-default
            var expected = new Commit(null, default, default, "force");
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoChasmSerializer_Roundtrip_Commit_Parents_Empty))]
        public static void ProtoChasmSerializer_Roundtrip_Commit_Parents_Empty()
        {
            var ser = new ProtoChasmSerializer();

            var expected = new Commit(Array.Empty<CommitId>(), default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoChasmSerializer_Roundtrip_Commit_Parents_1_Empty))]
        public static void ProtoChasmSerializer_Roundtrip_Commit_Parents_1_Empty()
        {
            var ser = new ProtoChasmSerializer();

            var expected = new Commit(CommitId.Empty, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoChasmSerializer_Roundtrip_Commit_Parents_1))]
        public static void ProtoChasmSerializer_Roundtrip_Commit_Parents_1()
        {
            var ser = new ProtoChasmSerializer();

            var expected = new Commit(Parent1, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoChasmSerializer_Roundtrip_Commit_Parents_2))]
        public static void ProtoChasmSerializer_Roundtrip_Commit_Parents_2()
        {
            var ser = new ProtoChasmSerializer();

            var parents = new[] { Parent1, Parent2 };

            var expected = new Commit(parents, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoChasmSerializer_Roundtrip_Commit_Parents_3))]
        public static void ProtoChasmSerializer_Roundtrip_Commit_Parents_3()
        {
            var ser = new ProtoChasmSerializer();

            var parents = new[] { Parent1, Parent2, Parent3 };

            var expected = new Commit(parents, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }
    }
}
