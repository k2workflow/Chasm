using System;
using SourceCode.Clay.Buffers;

namespace SourceCode.Chasm.Serializer
{
    partial interface IChasmSerializer // .CommitId
    {
        Memory<byte> Serialize(CommitId model, ArenaMemoryPool<byte> pool);

        CommitId DeserializeCommitId(ReadOnlySpan<byte> span);
    }
}
