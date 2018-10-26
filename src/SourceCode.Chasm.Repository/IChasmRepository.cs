using System.IO.Compression;
using SourceCode.Chasm.Serializer;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.Repository
{
    public partial interface IChasmRepository
    {
        IChasmSerializer Serializer { get; }

        CompressionLevel CompressionLevel { get; }

        int MaxDop { get; }

        crypt.SHA1 Hasher { get; }
    }
}
