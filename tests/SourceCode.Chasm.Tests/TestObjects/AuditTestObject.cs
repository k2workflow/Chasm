namespace SourceCode.Chasm.Tests.TestObjects
{
    public static class AuditTestObject
    {
        public static readonly Audit Random = new Audit(
            RandomHelper.String,
            RandomHelper.DateTimeOffset);
    }
}
