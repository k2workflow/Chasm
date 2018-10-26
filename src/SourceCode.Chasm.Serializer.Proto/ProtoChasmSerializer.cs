using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Google.Protobuf;

namespace SourceCode.Chasm.Serializer.Proto
{
    public sealed partial class ProtoChasmSerializer : IChasmSerializer
    {
        private static unsafe Memory<byte> SerializeImpl<T>(T wire, SessionMemoryPool<byte> pool)
            where T : IMessage<T>
        {
            Debug.Assert(pool != null);

            int length = wire.CalculateSize();
            IMemoryOwner<byte> owner = pool.Rent(length);

            // https://github.com/dotnet/corefx/pull/32669#issuecomment-429579594
            fixed (byte* rf = &MemoryMarshal.GetReference(owner.Memory.Span))
            {
                using (var strm = new UnmanagedMemoryStream(rf, length, owner.Memory.Length, FileAccess.Write))
                {
                    wire.WriteTo(strm);
                }
            }

            Memory<byte> mem = owner.Memory.Slice(0, length);
            return mem;
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
