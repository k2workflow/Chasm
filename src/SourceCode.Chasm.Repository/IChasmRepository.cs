using System.IO.Compression;
using SourceCode.Chasm.Serializer;

namespace SourceCode.Chasm.Repository
{
    public partial interface IChasmRepository
    {
        IChasmSerializer Serializer { get; }

        CompressionLevel CompressionLevel { get; }

        int MaxDop { get; }

        System.Security.Cryptography.SHA1 Hasher { get; }
    }
}
