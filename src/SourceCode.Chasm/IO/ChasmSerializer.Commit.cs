using SourceCode.Clay.Buffers;
using System;

namespace SourceCode.Chasm.IO
{
    partial class ChasmSerializer // .Commit
    {
        public abstract BufferSession Serialize(Commit model);

        public abstract Commit DeserializeCommit(ReadOnlyMemory<byte> buffer);

        public abstract Commit DeserializeCommit(ArraySegment<byte> segment);
    }
}
