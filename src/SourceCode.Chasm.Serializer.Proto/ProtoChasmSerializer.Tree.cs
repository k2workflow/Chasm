using System;
using SourceCode.Chasm.Serializer.Proto.Wire;

namespace SourceCode.Chasm.Serializer.Proto
{
    partial class ProtoChasmSerializer // .Tree
    {
        public override Memory<byte> Serialize(TreeNodeMap model)
        {
            TreeWire wire = model.Convert();

            Memory<byte> slice = SerializeImpl(wire);
            return slice;
        }

        public override TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) return default;

            var wire = new TreeWire();
            DeserializeImpl(span, ref wire);

            TreeNodeMap model = wire.Convert();
            return model;
        }
    }
}
