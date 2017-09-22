using SourceCode.Clay.Buffers;
using System;

namespace SourceCode.Chasm.IO
{
    partial class ChasmSerializer // .Sha1
    {
        public abstract BufferSession Serialize(Sha1 model);

        public abstract Sha1 DeserializeSha1(ReadOnlySpan<byte> span);
    }
}
