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

            length = wire.CalculateSize();
            IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(length);

            Memory<byte> mem = owner.Memory.Slice(0, length);
            using (var cos = new CodedOutputStream(mem.ToArray())) // TODO: Perf
            {
                wire.WriteTo(cos);

                return owner;
            }
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
