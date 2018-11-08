using System;
using System.Buffers;

namespace SourceCode.Chasm.Serializer
{
    public abstract class BaseChasmSerializer : IChasmSerializer, IDisposable
    {
        private readonly OwnerTrackingBytePool _pool;

        protected BaseChasmSerializer(int capacity)
        {
            _pool = new OwnerTrackingBytePool(capacity);
        }

        protected BaseChasmSerializer()
        {
            _pool = new OwnerTrackingBytePool();
        }

        protected Memory<byte> Rent(int minBufferSize)
        {
            IMemoryOwner<byte> owner = _pool.Rent(minBufferSize);
            return owner.Memory;
        }

        public abstract Memory<byte> Serialize(Commit model);
        public abstract Commit DeserializeCommit(ReadOnlySpan<byte> span);

        public abstract Memory<byte> Serialize(CommitId model);
        public abstract CommitId DeserializeCommitId(ReadOnlySpan<byte> span);

        public abstract Memory<byte> Serialize(TreeNodeMap model);
        public abstract TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span);

        #region IDisposable

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
