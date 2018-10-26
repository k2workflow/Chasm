using System;
using SourceCode.Chasm.Serializer.Proto.Wire;
using SourceCode.Clay.Buffers;

namespace SourceCode.Chasm.Serializer.Proto
{
    public partial class ProtoChasmSerializer // .Commit
    {
        public Memory<byte> Serialize(Commit model, ArenaMemoryPool<byte> pool)
        {
            CommitWire wire = model.Convert();

            Memory<byte> mem = SerializeImpl(wire, pool);
            return mem;
        }

        public Commit DeserializeCommit(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) return default;

            var wire = new CommitWire();
            DeserializeImpl(span, ref wire);

            Commit model = wire.Convert();
            return model;
        }
    }
}
