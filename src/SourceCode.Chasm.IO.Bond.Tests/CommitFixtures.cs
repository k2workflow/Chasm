using SourceCode.Clay;
using System;
using Xunit;

namespace SourceCode.Chasm.IO.Bond.Units
{
    public static class CommitFixtures
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(BondCasSerializer_WriteRead_Commit))]
        public static void BondCasSerializer_WriteRead_Commit()
        {
            var ser = new BondChasmSerializer();

            var parent = new CommitId(Sha1.Hash("abc"));
            var treeId = new TreeId(Sha1.Hash("def"));
            var expected = new Commit(parent, treeId, DateTime.UtcNow, "Updating Northwind.");

            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }
    }
}
