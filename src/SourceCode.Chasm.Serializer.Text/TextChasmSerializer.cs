using System;

namespace SourceCode.Chasm.Serializer.Text
{
    public sealed partial class TextChasmSerializer : IChasmSerializer, IDisposable
    {
        private readonly OwnerTrackedPool<byte> _pool;

        public TextChasmSerializer(int capacity)
        {
            _pool = new OwnerTrackedPool<byte>(capacity);
        }

        public TextChasmSerializer()
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
