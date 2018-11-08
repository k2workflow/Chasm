using System;
using System.Text;
using SourceCode.Chasm.Serializer.Text.Wire;

namespace SourceCode.Chasm.Serializer.Text
{
    partial class TextChasmSerializer // .Tree
    {
        public override Memory<byte> Serialize(TreeNodeMap model)
        {
            string wire = model.Convert();

            int length = Encoding.UTF8.GetMaxByteCount(wire.Length); // Utf8 is 1-4 bpc

            Memory<byte> rented = Rent(length);
            length = Encoding.UTF8.GetBytes(wire, rented.Span);

            Memory<byte> slice = rented.Slice(0, length);
            return slice;
        }

        public override TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) return default;

            string text = Encoding.UTF8.GetString(span);

            TreeNodeMap model = text.ConvertTree();
            return model;
        }
    }
}
