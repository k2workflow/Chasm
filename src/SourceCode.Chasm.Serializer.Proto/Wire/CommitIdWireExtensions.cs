namespace SourceCode.Chasm.Serializer.Proto.Wire
{
    internal static class CommitIdWireExtensions
    {
        public static CommitIdWire Convert(this CommitId model) => new CommitIdWire
        {
            Id = model.Sha1.Convert()
        };

        public static CommitId? Convert(this CommitIdWire wire)
        {
            if (wire == null) return default;

            Clay.Sha1? sha1 = wire.Id.Convert();
            if (sha1 == null) return default;

            return new CommitId(sha1.Value);
        }
    }
}
