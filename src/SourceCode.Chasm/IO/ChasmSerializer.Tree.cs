using SourceCode.Clay.Buffers;
using System;

namespace SourceCode.Chasm.IO
{
    partial class ChasmSerializer // .Tree
    {
        public abstract BufferSession Serialize(TreeNodeList model);

        public abstract TreeNodeList DeserializeTree(ReadOnlySpan<byte> span);
    }
}
