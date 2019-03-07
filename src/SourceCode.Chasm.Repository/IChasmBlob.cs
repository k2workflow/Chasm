using System;
using System.Buffers;

namespace SourceCode.Chasm.Repository
{
    public interface IChasmBlob : IDisposable
    {
        ReadOnlyMemory<byte> Content { get; }

        ChasmMetadata Metadata { get; }
    }

    internal sealed class ChasmBlob : IChasmBlob
    {
        private IMemoryOwner<byte> _owner;

        public ReadOnlyMemory<byte> Content { get; }

        public ChasmMetadata Metadata { get; }

        public ChasmBlob(IMemoryOwner<byte> owner, ChasmMetadata metadata)
        {
            _owner = owner;
            if (_owner != null)
                Content = _owner.Memory;
            Metadata = metadata;
        }

        public ChasmBlob(ReadOnlyMemory<byte> content, ChasmMetadata metadata)
        {
            _owner = null;
            Content = content;
            Metadata = metadata;
        }

        private bool _disposed;

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _owner?.Dispose();
                    _owner = null;
                }

                _disposed = true;
            }
        }

        public void Dispose()
            => Dispose(true);
    }
}
