using System;
using SourceCode.Chasm.Serializer.Proto.Wire;

namespace SourceCode.Chasm.Serializer.Proto
{
    partial class ProtoChasmSerializer // .CommitId
    {
        public override Memory<byte> Serialize(CommitId model)
        {
            CommitIdWire wire = model.Convert();

            Memory<byte> slice = SerializeImpl(wire);
            return slice;
        }

        public override CommitId DeserializeCommitId(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) return default;

            var wire = new CommitIdWire();
            DeserializeImpl(span, ref wire);

            CommitId? model = wire.Convert();
            return model.Value;
        }
    }
}
