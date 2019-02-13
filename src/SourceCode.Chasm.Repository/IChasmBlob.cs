using System;

namespace SourceCode.Chasm.Repository
{
    public interface IChasmBlob
    {
        ReadOnlyMemory<byte> Content { get; }

        Metadata Metadata { get; }
    }

    internal sealed class ChasmBlob : IChasmBlob
    {
        public ReadOnlyMemory<byte> Content { get; }

        public Metadata Metadata { get; }

        public ChasmBlob(ReadOnlyMemory<byte> content, Metadata metadata)
        {
            Content = content;
            Metadata = metadata;
        }
    }
}
