using System;
using System.Buffers;
using SourceCode.Chasm.Serializer.Text.Wire;

namespace SourceCode.Chasm.Serializer.Text
{
    partial class TextChasmSerializer // .Commit
    {
        public IMemoryOwner<byte> Serialize(Commit model)
        {
            string text = model.Convert();
            return ToOwnedUtf8(text);
        }

        public Commit DeserializeCommit(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) throw new ArgumentNullException(nameof(span));

            string text = GetUtf8(span);
            return text.ConvertCommit();
        }
    }
}
