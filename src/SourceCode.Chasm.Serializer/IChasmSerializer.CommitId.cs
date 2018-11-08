using System;

namespace SourceCode.Chasm.Serializer
{
    partial interface IChasmSerializer // .CommitId
    {
        Memory<byte> Serialize(CommitId model);

        CommitId DeserializeCommitId(ReadOnlySpan<byte> span);
    }
}
