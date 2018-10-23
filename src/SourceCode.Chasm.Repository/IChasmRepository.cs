using SourceCode.Chasm.Serializer;
using System.IO.Compression;

namespace SourceCode.Chasm.Repository
{
    public partial interface IChasmRepository
    {
        IChasmSerializer Serializer { get; }

        CompressionLevel CompressionLevel { get; }

        int MaxDop { get; }
    }
}
