using System;
using System.Buffers;

namespace SourceCode.Chasm.Serializer
{
    partial interface IChasmSerializer // .Tree
    {
        IMemoryOwner<byte> Serialize(TreeNodeMap model, out int length);
  
        TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span);
    }
}
