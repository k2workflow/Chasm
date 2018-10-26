using System;
using SourceCode.Chasm.Serializer.Proto.Wire;
using SourceCode.Clay.Buffers;

namespace SourceCode.Chasm.Serializer.Proto
{
    partial class ProtoChasmSerializer // .Tree
    {
        public Memory<byte> Serialize(TreeNodeMap model, ArenaMemoryPool<byte> pool)
        {
            TreeWire wire = model.Convert();

            Memory<byte> mem = SerializeImpl(wire, pool);
            return mem;
        }

        public TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) return default;

            var wire = new TreeWire();
            DeserializeImpl(span, ref wire);

            TreeNodeMap model = wire.Convert();
            return model;
        }
    }
}
