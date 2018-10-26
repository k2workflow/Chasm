using System;

namespace SourceCode.Chasm.Serializer
{
    partial interface IChasmSerializer // .CommitId
    {
        Memory<byte> Serialize(CommitId model, SessionPool<byte> pool);

        CommitId DeserializeCommitId(ReadOnlySpan<byte> span);
    }
}
