using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;
using SourceCode.Clay.Buffers;

namespace SourceCode.Chasm.Serializer.Text
{
    public sealed partial class TextChasmSerializer : IChasmSerializer, IDisposable
    {
        private readonly MemoryPool<byte> _pool;

        public TextChasmSerializer(MemoryPool<byte> pool = null)
        {
            _pool = pool ?? MemoryPool<byte>.Shared;
        }

        private IMemoryOwner<byte> GetBytes(string text)
        {
            Debug.Assert(text != null);

            int length = Encoding.UTF8.GetMaxByteCount(text.Length); // Utf8 is 1-4 bpc

            IMemoryOwner<byte> rented = _pool.Rent(length);

            length = Encoding.UTF8.GetBytes(text, rented.Memory.Span);

            return rented.Slice(0, length);
        }

        private static string GetoString(ReadOnlySpan<byte> utf8)
            => Encoding.UTF8.GetString(utf8);

        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                // This is a no-op for MemoryPool.Shared
                // See https://github.com/dotnet/corefx/blob/master/src/System.Memory/src/System/Buffers/ArrayMemoryPool.cs
                _pool.Dispose();
            }
        }
    }
}
