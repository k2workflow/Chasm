using Google.Protobuf;
using SourceCode.Chasm.Serializer.Proto.Wire;
using System;
using System.Buffers;

namespace SourceCode.Chasm.Serializer.Proto
{
    partial class ProtoChasmSerializer // .CommitId
    {
        public IMemoryOwner<byte> Serialize(CommitId model, out int length)
        {
            CommitIdWire wire = model.Convert();

            length = wire.CalculateSize();
            IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(length);

            using (var cos = new CodedOutputStream(owner.Memory.ToArray()))
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
