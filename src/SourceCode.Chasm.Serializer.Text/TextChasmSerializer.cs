using System;
using System.Buffers;

namespace SourceCode.Chasm.Serializer.Text
{
    public sealed partial class TextChasmSerializer : IChasmSerializer, IDisposable
    {
        private readonly MemoryPool<byte> _pool;

        public TextChasmSerializer(MemoryPool<byte> pool = null)
        {
            _pool = pool ?? MemoryPool<byte>.Shared;
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
