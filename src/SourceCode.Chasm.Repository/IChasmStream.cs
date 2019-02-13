using System;
using System.IO;

namespace SourceCode.Chasm.Repository
{
    public interface IChasmStream : IDisposable
    {
        Stream Content { get; }

        ChasmMetadata Metadata { get; }
    }

    internal sealed class ChasmStream : IChasmStream
    {
        public Stream Content { get; }

        public ChasmMetadata Metadata { get; }

        public ChasmStream(Stream stream, ChasmMetadata metadata)
        {
            Content = stream;
            Metadata = metadata;
        }

        #region IDisposable

        private bool _disposed;

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Content.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
            => Dispose(true);

        #endregion
    }
}
