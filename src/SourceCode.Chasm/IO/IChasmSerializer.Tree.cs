using SourceCode.Clay.Buffers;
using System;

namespace SourceCode.Chasm.IO
{
    partial interface IChasmSerializer // .Tree
    {
        BufferSession Serialize(TreeNodeList model);

        TreeNodeList DeserializeTree(ReadOnlyBuffer<byte> buffer);

        TreeNodeList DeserializeTree(ArraySegment<byte> segment);
    }
}
