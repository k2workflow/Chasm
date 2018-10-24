using System;
using System.Buffers;
using Google.Protobuf;
using SourceCode.Chasm.Serializer.Proto.Wire;

namespace SourceCode.Chasm.Serializer.Proto
{
    public partial class ProtoChasmSerializer // .Commit
    {
        public IMemoryOwner<byte> Serialize(Commit model, out int length)
        {
            CommitWire wire = model.Convert();

            IMemoryOwner<byte> owner = SerializeImpl(wire, out length);
            return owner;
        }

        public Commit DeserializeCommit(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) return default;

            var wire = new CommitWire();
            wire.MergeFrom(span.ToArray()); // TODO: Perf

            Commit model = wire.Convert();
            return model;
        }
    }
}
