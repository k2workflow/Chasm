using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static partial class CommitTests
    {
        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Default))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Default(IChasmSerializer ser)
        {
            // All default
            var expected = new Commit(null, default, default, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }
    }
}
