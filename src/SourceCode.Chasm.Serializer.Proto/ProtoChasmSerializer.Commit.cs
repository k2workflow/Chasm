using Google.Protobuf;
using SourceCode.Chasm.Serializer.Proto.Wire;
using System;
using System.Buffers;

namespace SourceCode.Chasm.Serializer.Proto
{
    public partial class ProtoChasmSerializer // .Commit
    {
        public IMemoryOwner<byte> Serialize(Commit model, out int length)
        {
            CommitWire wire = model.Convert();

            length = wire.CalculateSize();
            IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(length);

            using (var cos = new CodedOutputStream(owner.Memory.ToArray()))
            {
                wire.WriteTo(cos);

                return owner;
            }
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
