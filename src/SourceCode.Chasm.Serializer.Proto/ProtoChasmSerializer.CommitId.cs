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

            length = wire.CalculateSize();
            IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(length);

            Memory<byte> mem = owner.Memory.Slice(0, length);
            using (var cos = new CodedOutputStream(mem.ToArray())) // TODO: Perf
            {
                wire.WriteTo(cos);

                return owner;
            }
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
