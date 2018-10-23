namespace SourceCode.Chasm.Tests.TestObjects
{
    public static class CommitIdTestObject
    {
        public static readonly CommitId Random = new CommitId(Sha1TestObject.Random);
    }
}
