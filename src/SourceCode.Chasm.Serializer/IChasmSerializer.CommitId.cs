using System;
using System.Buffers;

namespace SourceCode.Chasm.Serializer
{
    partial interface IChasmSerializer // .CommitId
    {
        IMemoryOwner<byte> Serialize(CommitId model);

        CommitId DeserializeCommitId(ReadOnlySpan<byte> span);
    }
}
