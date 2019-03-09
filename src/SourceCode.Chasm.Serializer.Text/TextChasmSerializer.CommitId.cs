using System;
using System.Buffers;
using SourceCode.Chasm.Serializer.Text.Wire;

namespace SourceCode.Chasm.Serializer.Text
{
    partial class TextChasmSerializer // .CommitId
    {
        public IMemoryOwner<byte> Serialize(CommitId model)
        {
            string text = model.Convert();
            return ToOwnedUtf8(text);
        }

        public CommitId DeserializeCommitId(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) throw new ArgumentNullException(nameof(span));

            string text = GetUtf8(span);
            return text.ConvertCommitId();
        }
    }
}
