using System;
using System.Buffers;
using System.Text;
using SourceCode.Chasm.Serializer.Text.Wire;

namespace SourceCode.Chasm.Serializer.Text
{
    partial class TextChasmSerializer // .CommitId
    {
        public IMemoryOwner<byte> Serialize(CommitId model, out int length)
        {
            string wire = model.Convert();

            int maxLen = Encoding.UTF8.GetMaxByteCount(wire.Length); // Utf8 is 1-4 bpc
            IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(maxLen);

            length = Encoding.UTF8.GetBytes(wire, owner.Memory.Span);

            return owner;
        }

        public CommitId DeserializeCommitId(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) throw new ArgumentNullException(nameof(span));

            string text = Encoding.UTF8.GetString(span);

            CommitId model = text.ConvertCommitId();
            return model;
        }
    }
}
