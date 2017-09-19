using System.IO.Compression;

namespace SourceCode.Chasm.IO
{
    public partial interface IChasmRepository
    {
        IChasmSerializer Serializer { get; }

        CompressionLevel CompressionLevel { get; }

        int MaxDop { get; }
    }
}
