using Xunit;

namespace SourceCode.Chasm.IO.Proto.Tests
{
    public static partial class CommitTests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoChasmSerializer_Roundtrip_Commit_Default))]
        public static void ProtoChasmSerializer_Roundtrip_Commit_Default()
        {
            var ser = new ProtoChasmSerializer();

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
