using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using Google.Protobuf;

namespace SourceCode.Chasm.Serializer.Proto
{
    public sealed partial class ProtoChasmSerializer : BaseChasmSerializer
    {
        public ProtoChasmSerializer(int capacity)
            : base(capacity)
        { }

        public ProtoChasmSerializer()
        { }

        private unsafe Memory<byte> SerializeImpl<T>(T wire)
            where T : IMessage<T>
        {
            int length = wire.CalculateSize();
            Memory<byte> rented = Rent(length);

            // https://github.com/dotnet/corefx/pull/32669#issuecomment-429579594
            fixed (byte* rf = &MemoryMarshal.GetReference(rented.Span))
            {
                using (var strm = new UnmanagedMemoryStream(rf, length, rented.Length, FileAccess.Write))
                {
                    wire.WriteTo(strm);
                }
            }

            Memory<byte> slice = rented.Slice(0, length);
            return slice;
        }

        private static unsafe void DeserializeImpl<T>(ReadOnlySpan<byte> span, ref T wire)
            where T : IMessage<T>
        {
            byte[] rented = ArrayPool<byte>.Shared.Rent(span.Length);
            {
                // TODO: Perf
                span.CopyTo(new Span<byte>(rented, 0, span.Length)); // First copy
                wire.MergeFrom(rented, 0, span.Length); // Second copy
            }
            ArrayPool<byte>.Shared.Return(rented);
        }
    }
}
