using Moq;
using SourceCode.Chasm.Serializer.Json;
using Xunit;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.Repository.Azure.Tests
{
    public static class ChasmRepositoryTests
    {
        private static readonly crypt.SHA1 s_hasher = crypt.SHA1.Create();

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
        [Fact]
        public static void Repo_ReadCommitRef() //byte[] x, byte[] y, bool isEqual)
        {
            var repo = new Mock<IChasmRepository>();

            repo.Setup(r => r.Serializer).Returns(new JsonChasmSerializer());

            //repo.Setup(r => r.ReadCommitRefAsync(null, null, null, CancellationToken.None)).Returns(new ValueTask<CommitRef?>(CommitRef.Empty));
            //repo.Setup(r => r.ReadCommitRefAsync(string.Empty, null, null, CancellationToken.None)).Returns(new ValueTask<CommitRef?>(CommitRef.Empty));
            //repo.Setup(r => r.ReadCommitRefAsync(null, string.Empty, null, CancellationToken.None)).Returns(new ValueTask<CommitRef?>(CommitRef.Empty));
            //repo.Setup(r => r.ReadCommitRefAsync(string.Empty, string.Empty, null, CancellationToken.None)).Returns(new ValueTask<CommitRef?>(CommitRef.Empty));

            //repo.Setup(r => r.ReadCommitRefAsync("branch", "name", null, CancellationToken.None)).Returns(new ValueTask<CommitRef?>(new CommitRef("branch-name", new CommitId(s_hasher.HashData("branch-name")))));
        }
    }
}
