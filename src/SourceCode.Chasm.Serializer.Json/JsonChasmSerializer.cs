using System;

namespace SourceCode.Chasm.Serializer.Json
{
    public sealed partial class JsonChasmSerializer : IChasmSerializer, IDisposable
    {
        private readonly OwnerTrackedPool<byte> _pool;

        public JsonChasmSerializer(int capacity)
        {
            _pool = new OwnerTrackedPool<byte>(capacity);
        }

        public JsonChasmSerializer()
        {
            _pool = new OwnerTrackedPool<byte>();
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
