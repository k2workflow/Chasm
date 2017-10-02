using Moq;
using SourceCode.Chasm.IO.Json;
using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static class ChasmRepositoryTests
    {
        //[InlineData(null, null, true)]
        //[InlineData(new byte[0], null, false)]
        //[InlineData(new byte[0], new byte[0], true)]
        //[InlineData(new byte[0], new byte[1] { 0 }, false)]
        //[InlineData(new byte[1] { 0 }, new byte[1] { 0 }, true)]
        //[InlineData(new byte[1] { 0 }, new byte[1] { 1 }, false)]
        //[InlineData(new byte[1] { 0 }, new byte[2] { 0, 1 }, false)]
        //[InlineData(new byte[2] { 0, 0 }, new byte[2] { 0, 1 }, false)]
        //[InlineData(new byte[2] { 0, 1 }, new byte[2] { 0, 1 }, true)]
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Repo_ReadCommitRef))]
        public static void Repo_ReadCommitRef() //byte[] x, byte[] y, bool isEqual)
        {
            var repo = new Mock<IChasmRepository>();
            repo.Setup(r => r.Serializer).Returns(new JsonChasmSerializer());
            repo.Setup(r => r.CompressionLevel).Returns(System.IO.Compression.CompressionLevel.NoCompression);

            repo.Setup(r => r.ReadCommitRef(null, null)).Returns(CommitId.Empty);

            repo.Setup(r => r.ReadCommitRef(string.Empty, null)).Returns(CommitId.Empty);
            repo.Setup(r => r.ReadCommitRef(null, string.Empty)).Returns(CommitId.Empty);
            repo.Setup(r => r.ReadCommitRef(string.Empty, string.Empty)).Returns(CommitId.Empty);

            repo.Setup(r => r.ReadCommitRef("branch", "name")).Returns(new CommitId(Sha1.Hash("branch-name")));
        }
    }
}
