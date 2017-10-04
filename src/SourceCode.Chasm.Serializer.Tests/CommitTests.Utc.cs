using System;
using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static partial class CommitTests // .Utc
    {
        private static readonly DateTime Utc1 = new DateTime(new DateTime(2000, 1, 1).Ticks, DateTimeKind.Utc);

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Utc))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Utc(IChasmSerializer ser)
        {
            var expected = new Commit(null, default, Utc1, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }
    }
}
