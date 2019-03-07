using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using Google.Protobuf;
using SourceCode.Clay.Buffers;

namespace SourceCode.Chasm.Serializer.Proto
{
    public sealed partial class ProtoChasmSerializer : IChasmSerializer, IDisposable
    {
        private readonly MemoryPool<byte> _pool;

        public ProtoChasmSerializer(MemoryPool<byte> pool = null)
        {
            _pool = pool ?? MemoryPool<byte>.Shared;
        }

        private unsafe IMemoryOwner<byte> SerializeImpl<T>(T wire)
            where T : IMessage<T>
        {
            int length = wire.CalculateSize();
            IMemoryOwner<byte> rented = _pool.RentExact(length);

            // https://github.com/dotnet/corefx/pull/32669#issuecomment-429579594
            fixed (byte* ba = &MemoryMarshal.GetReference(rented.Memory.Span))
            {
                using (var strm = new UnmanagedMemoryStream(ba, length, rented.Memory.Length, FileAccess.Write))
                {
                    wire.WriteTo(strm);
                }
            }

            return rented;
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

        #region IDisposable

        private bool _disposed;

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // This is a no-op for MemoryPool.Shared
                    // See https://github.com/dotnet/corefx/blob/master/src/System.Memory/src/System/Buffers/ArrayMemoryPool.cs
                    _pool.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
