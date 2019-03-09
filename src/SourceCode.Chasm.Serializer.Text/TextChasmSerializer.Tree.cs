using System;
using System.Buffers;
using SourceCode.Chasm.Serializer.Text.Wire;

namespace SourceCode.Chasm.Serializer.Text
{
    partial class TextChasmSerializer // .Tree
    {
        public IMemoryOwner<byte> Serialize(TreeNodeMap model)
        {
            string text = model.Convert();
            return ToOwnedUtf8(text);
        }

        public TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) return default;

            string text = GetUtf8(span);
            return text.ConvertTree();
        }
    }
}
