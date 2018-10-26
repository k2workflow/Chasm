using System;
using SourceCode.Clay.Buffers;

namespace SourceCode.Chasm.Serializer
{
    partial interface IChasmSerializer // .Tree
    {
        Memory<byte> Serialize(TreeNodeMap model, ArenaMemoryPool<byte> pool);

        TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span);
    }
}
