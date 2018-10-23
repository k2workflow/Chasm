using SourceCode.Chasm.Tests.Helpers;

namespace SourceCode.Chasm.Tests.TestObjects
{
    public static class CommitRefTestObject
    {
        public static readonly CommitRef Random = new CommitRef(
            RandomHelper.String,
            CommitIdTestObject.Random);
    }
}
