using SourceCode.Clay;
using Xunit;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.Tests
{
    public static class CommitIRefTests
    {
        private static readonly crypt.SHA1 s_sha1 = crypt.SHA1.Create();

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(CommitRef_equality))]
        public static void CommitRef_equality()
        {
            var commitRef1 = new CommitRef("abc", new CommitId(s_sha1.HashData("abc")));
            var commitRef2 = new CommitRef("abc", new CommitId(s_sha1.HashData("abc")));
            var commitRef3 = new CommitRef("def", new CommitId(s_sha1.HashData("def")));

            Assert.True(commitRef1 == commitRef2);
            Assert.False(commitRef1 != commitRef2);
            Assert.True(commitRef1.Equals((object)commitRef2));

            Assert.Equal(commitRef1, commitRef2);
            Assert.Equal(commitRef1.GetHashCode(), commitRef2.GetHashCode());
            Assert.Equal(commitRef1.ToString(), commitRef2.ToString());

            Assert.NotEqual(CommitRef.Empty, commitRef1);
            Assert.NotEqual(CommitRef.Empty.GetHashCode(), commitRef1.GetHashCode());

            Assert.NotEqual(commitRef3, commitRef1);
            Assert.NotEqual(commitRef3.GetHashCode(), commitRef1.GetHashCode());
            Assert.NotEqual(commitRef3.ToString(), commitRef1.ToString());
        }
    }
}
