using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using Google.Protobuf;

namespace SourceCode.Chasm.Serializer.Proto
{
    public sealed partial class ProtoChasmSerializer : IChasmSerializer, IDisposable
    {
        private readonly OwnerTrackedPool<byte> _pool;

        public ProtoChasmSerializer(int capacity)
        {
            _pool = new OwnerTrackedPool<byte>(capacity);
        }

        public ProtoChasmSerializer()
        {
            _pool = new OwnerTrackedPool<byte>();
        }

        private unsafe Memory<byte> SerializeImpl<T>(T wire)
            where T : IMessage<T>
        {
            int length = wire.CalculateSize();
            Memory<byte> rented = _pool.Rent(length);

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

        #region IDisposable

        private bool _disposed;

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _pool.Dispose();
                }

                _disposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
