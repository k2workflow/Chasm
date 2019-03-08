using System;

namespace SourceCode.Chasm.Repository
{
    public interface IChasmBlob
    {
        ReadOnlyMemory<byte> Content { get; }

        ChasmMetadata Metadata { get; }
    }

    internal sealed class ChasmBlob : IChasmBlob
    {
        public ReadOnlyMemory<byte> Content { get; }

        public ChasmMetadata Metadata { get; }

        public ChasmBlob(ReadOnlyMemory<byte> content, ChasmMetadata metadata)
        {
            Content = content;
            Metadata = metadata;
        }
    }
}
