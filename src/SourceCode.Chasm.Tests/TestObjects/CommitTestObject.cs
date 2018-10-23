using SourceCode.Chasm.Tests.Helpers;

namespace SourceCode.Chasm.Tests.TestObjects
{
    public static class CommitTestObject
    {
        public static readonly Commit Random = new Commit(
            CommitIdTestObject.Random,
            TreeIdTestObject.Random,
            AuditTestObject.Random,
            AuditTestObject.Random,
            RandomHelper.String);
    }
}
