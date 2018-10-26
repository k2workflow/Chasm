using System;
using System.Buffers;
using System.Text;
using SourceCode.Chasm.Serializer.Text.Wire;

namespace SourceCode.Chasm.Serializer.Text
{
    partial class TextChasmSerializer // .Tree
    {
        public Memory<byte> Serialize(TreeNodeMap model, SessionPool<byte> pool)
        {
            string wire = model.Convert();

            int maxLen = Encoding.UTF8.GetMaxByteCount(wire.Length); // Utf8 is 1-4 bpc
            IMemoryOwner<byte> owner = pool.Rent(maxLen);

            int length = Encoding.UTF8.GetBytes(wire, owner.Memory.Span);
            Memory<byte> mem = owner.Memory.Slice(0, length);

            return mem;
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
