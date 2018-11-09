using System;
using System.Text;
using SourceCode.Chasm.Serializer.Text.Wire;

namespace SourceCode.Chasm.Serializer.Text
{
    partial class TextChasmSerializer // .CommitId
    {
        public Memory<byte> Serialize(CommitId model)
        {
            string wire = model.Convert();

            int length = Encoding.UTF8.GetMaxByteCount(wire.Length); // Utf8 is 1-4 bpc

            Memory<byte> rented = _pool.Rent(length);
            length = Encoding.UTF8.GetBytes(wire, rented.Span);

            Memory<byte> slice = rented.Slice(0, length);
            return slice;
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
