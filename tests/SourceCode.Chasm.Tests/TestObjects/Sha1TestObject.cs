using SourceCode.Chasm.Tests.Helpers;
using SourceCode.Clay;

namespace SourceCode.Chasm.Tests.TestObjects
{
    public static class Sha1TestObject
    {
        public static readonly Sha1 Random = Sha1.Hash(RandomHelper.String);
    }
}
