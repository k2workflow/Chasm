using System;
using Xunit;

namespace SourceCode.Chasm.IO.Proto.Tests
{
    public static partial class CommitTests // .Utc
    {
        private static readonly DateTime Utc1 = new DateTime(new DateTime(2000, 1, 1).Ticks, DateTimeKind.Utc);

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ProtoChasmSerializer_Roundtrip_Commit_Utc))]
        public static void ProtoChasmSerializer_Roundtrip_Commit_Utc()
        {
            var ser = new ProtoChasmSerializer();

            var expected = new Commit(null, default, Utc1, null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }
    }
}
