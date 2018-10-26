using System;

namespace SourceCode.Chasm.Serializer
{
    partial interface IChasmSerializer // .Tree
    {
        Memory<byte> Serialize(TreeNodeMap model, SessionPool<byte> pool);

        TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span);
    }
}
