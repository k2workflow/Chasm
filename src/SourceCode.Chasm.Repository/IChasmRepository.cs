using System.IO.Compression;
using SourceCode.Chasm.Serializer;

namespace SourceCode.Chasm.Repository
{
    public partial interface IChasmRepository
    {
        IChasmSerializer Serializer { get; }

        CompressionLevel CompressionLevel { get; }

        int MaxDop { get; }
    }
}
