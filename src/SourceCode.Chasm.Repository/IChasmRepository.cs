using SourceCode.Chasm.Serializer;

namespace SourceCode.Chasm.Repository
{
    public partial interface IChasmRepository
    {
        IChasmSerializer Serializer { get; }

        int MaxDop { get; }
    }
}
