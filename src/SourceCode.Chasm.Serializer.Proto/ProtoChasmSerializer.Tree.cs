using System;
using System.Buffers;
using Google.Protobuf;
using SourceCode.Chasm.Serializer.Proto.Wire;

namespace SourceCode.Chasm.Serializer.Proto
{
    partial class ProtoChasmSerializer // .Tree
    {
        public IMemoryOwner<byte> Serialize(TreeNodeMap model, out int length)
        {
            TreeWire wire = model.Convert();

            IMemoryOwner<byte> owner = SerializeImpl(wire, out length);
            return owner;
        }

        public TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) return default;

            var wire = new TreeWire();
            wire.MergeFrom(span.ToArray()); // TODO: Perf

            TreeNodeMap model = wire.Convert();
            return model;
        }
    }
}
