using System;
using System.Buffers;
using Google.Protobuf;
using SourceCode.Chasm.Serializer.Proto.Wire;

namespace SourceCode.Chasm.Serializer.Proto
{
    partial class ProtoChasmSerializer // .CommitId
    {
        public IMemoryOwner<byte> Serialize(CommitId model, out int length)
        {
            CommitIdWire wire = model.Convert();

            IMemoryOwner<byte> owner = SerializeImpl(wire, out length);
            return owner;
        }

        public CommitId DeserializeCommitId(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) return default;

            var wire = new CommitIdWire();
            wire.MergeFrom(span.ToArray()); // TODO: Perf

            CommitId? model = wire.Convert();
            return model.Value;
        }
    }
}
