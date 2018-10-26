using System;
using SourceCode.Clay.Buffers;

namespace SourceCode.Chasm.Serializer
{
    partial interface IChasmSerializer // .Commit
    {
        Memory<byte> Serialize(Commit model, ArenaMemoryPool<byte> pool);

        Commit DeserializeCommit(ReadOnlySpan<byte> span);
    }
}
