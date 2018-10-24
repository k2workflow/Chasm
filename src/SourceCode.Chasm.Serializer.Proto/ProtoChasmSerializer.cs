using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using Google.Protobuf;

namespace SourceCode.Chasm.Serializer.Proto
{
    public sealed partial class ProtoChasmSerializer : IChasmSerializer
    {
        private static unsafe IMemoryOwner<byte> SerializeImpl<T>(T wire, out int length)
            where T : IMessage<T>
        {
            length = wire.CalculateSize();

            IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(length);

            // https://github.com/dotnet/corefx/pull/32669#issuecomment-429579594
            fixed (byte* rf = &MemoryMarshal.GetReference(owner.Memory.Span))
            {
                using (var strm = new UnmanagedMemoryStream(rf, length, owner.Memory.Length, FileAccess.Write))
                {
                    wire.WriteTo(strm);
                }
            }

            return owner;
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
