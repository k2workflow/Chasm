using System;
using System.Buffers;
using System.Text;
using SourceCode.Chasm.Serializer.Text.Wire;

namespace SourceCode.Chasm.Serializer.Text
{
    partial class TextChasmSerializer // .Tree
    {
        public IMemoryOwner<byte> Serialize(TreeNodeMap model, out int length)
        {
            string wire = model.Convert();

            int maxLen = Encoding.UTF8.GetMaxByteCount(wire.Length); // Utf8 is 1-4 bpc
            IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(maxLen);

            length = Encoding.UTF8.GetBytes(wire, owner.Memory.Span);

            return owner;
        }

        public TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) return default;

            string text = Encoding.UTF8.GetString(span);

            TreeNodeMap model = text.ConvertTree();
            return model;
        }
    }
}
