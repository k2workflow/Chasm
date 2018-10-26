using SourceCode.Chasm.Tests.Helpers;
using SourceCode.Clay;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.Tests.TestObjects
{
    public static class Sha1TestObject
    {
        private static readonly crypt.SHA1 s_sha1 = crypt.SHA1.Create();

        public static readonly Sha1 Random = s_sha1.HashData(RandomHelper.String);
    }
}
