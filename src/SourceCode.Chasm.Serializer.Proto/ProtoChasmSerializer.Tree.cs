using Google.Protobuf;
using SourceCode.Chasm.Serializer.Proto.Wire;
using System;
using System.Buffers;

namespace SourceCode.Chasm.Serializer.Proto
{
    partial class ProtoChasmSerializer // .Tree
    {
        public IMemoryOwner<byte> Serialize(TreeNodeMap model, out int length)
        {
            TreeWire wire = model.Convert();

            length = wire.CalculateSize();
            IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(length);

            using (var cos = new CodedOutputStream(owner.Memory.ToArray()))
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
