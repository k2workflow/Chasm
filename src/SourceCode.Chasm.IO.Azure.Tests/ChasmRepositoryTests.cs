using Moq;
using SourceCode.Chasm.IO.Json;
using System.Threading;
using Xunit;

namespace SourceCode.Chasm.IO.Azure.Tests
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

            repo.Setup(r => r.ReadCommitRefAsync(null, null, CancellationToken.None).Result).Returns(CommitId.Empty);

            repo.Setup(r => r.ReadCommitRefAsync(string.Empty, null, CancellationToken.None).Result).Returns(CommitId.Empty);
            repo.Setup(r => r.ReadCommitRefAsync(null, string.Empty, CancellationToken.None).Result).Returns(CommitId.Empty);
            repo.Setup(r => r.ReadCommitRefAsync(string.Empty, string.Empty, CancellationToken.None).Result).Returns(CommitId.Empty);

            repo.Setup(r => r.ReadCommitRefAsync("branch", "name", CancellationToken.None).Result).Returns(new CommitId(Sha1.Hash("branch-name")));
        }
    }
}
