namespace SourceCode.Chasm.Serializer.Proto.Wire
{
    internal static class CommitIdWireExtensions
    {
        public static CommitIdWire Convert(this CommitId model)
        {
            var wire = new CommitIdWire
            {
                Id = model.Sha1.Convert()
            };

            return wire;
        }

        public static CommitId? Convert(this CommitIdWire wire)
        {
            if (wire == null) return default;

            Clay.Sha1? sha1 = wire.Id.Convert();
            if (sha1 == null) return default;

            var model = new CommitId(sha1.Value);
            return model;
        }
    }
}
