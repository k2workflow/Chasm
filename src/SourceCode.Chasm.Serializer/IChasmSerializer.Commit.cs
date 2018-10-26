using System;

namespace SourceCode.Chasm.Serializer
{
    partial interface IChasmSerializer // .Commit
    {
        Memory<byte> Serialize(Commit model, SessionPool<byte> pool);

        Commit DeserializeCommit(ReadOnlySpan<byte> span);
    }
}
