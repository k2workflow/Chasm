using System;
using System.Buffers;
using System.Text;
using SourceCode.Chasm.Serializer.Text.Wire;

namespace SourceCode.Chasm.Serializer.Text
{
    partial class TextChasmSerializer // .Commit
    {
        public Memory<byte> Serialize(Commit model, SessionPool<byte> pool)
        {
            string wire = model.Convert();

            int maxLen = Encoding.UTF8.GetMaxByteCount(wire.Length); // Utf8 is 1-4 bpc
            IMemoryOwner<byte> owner = pool.Rent(maxLen);

            int length = Encoding.UTF8.GetBytes(wire, owner.Memory.Span);
            Memory<byte> mem = owner.Memory.Slice(0, length);

            return mem;
        }

        public Commit DeserializeCommit(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) throw new ArgumentNullException(nameof(span));

            string text = Encoding.UTF8.GetString(span);

            Commit model = text.ConvertCommit();
            return model;
        }
    }
}
