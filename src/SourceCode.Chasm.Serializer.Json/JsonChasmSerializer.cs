using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;
using SourceCode.Clay.Buffers;

namespace SourceCode.Chasm.Serializer.Json
{
    public sealed partial class JsonChasmSerializer : IChasmSerializer, IDisposable
    {
        private readonly MemoryPool<byte> _pool;

        public JsonChasmSerializer(MemoryPool<byte> pool = null)
        {
            _pool = pool ?? MemoryPool<byte>.Shared;
        }

        private IMemoryOwner<byte> ToUtf8(string json)
        {
            Debug.Assert(json != null);

            int length = Encoding.UTF8.GetMaxByteCount(json.Length); // Utf8 is 1-4 bpc

            IMemoryOwner<byte> rented = _pool.Rent(length);

            length = Encoding.UTF8.GetBytes(json, rented.Memory.Span);

            return rented.Slice(0, length);
        }

        private static unsafe string GetUtf8(ReadOnlySpan<byte> span)
            => Encoding.UTF8.GetString(span);

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
    }
}
