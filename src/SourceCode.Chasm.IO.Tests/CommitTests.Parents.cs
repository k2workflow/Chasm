using System;
using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static partial class CommitTests // .Parents
    {
        private static readonly CommitId Parent1 = new CommitId(Sha1.Hash(nameof(Parent1)));
        private static readonly CommitId Parent2 = new CommitId(Sha1.Hash(nameof(Parent2)));
        private static readonly CommitId Parent3 = new CommitId(Sha1.Hash(nameof(Parent3)));

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Parents_Null))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Parents_Null(IChasmSerializer ser)
        {
            // Force Commit to be non-default
            var expected = new Commit(null, default, default, "force");
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Parents_Empty))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Parents_Empty(IChasmSerializer ser)
        {
            var expected = new Commit(Array.Empty<CommitId>(), default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Parents_1_Empty))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Parents_1_Empty(IChasmSerializer ser)
        {
            var expected = new Commit(CommitId.Empty, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Parents_1))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Parents_1(IChasmSerializer ser)
        {
            var expected = new Commit(Parent1, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Parents_2))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Parents_2(IChasmSerializer ser)
        {
            var parents = new[] { Parent1, Parent2 };

            var expected = new Commit(parents, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Parents_3))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Parents_3(IChasmSerializer ser)
        {
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
