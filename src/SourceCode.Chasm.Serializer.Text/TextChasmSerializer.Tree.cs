using System;
using System.Buffers;
using System.Text;
using SourceCode.Chasm.Serializer.Text.Wire;
using SourceCode.Clay.Buffers;

namespace SourceCode.Chasm.Serializer.Text
{
    partial class TextChasmSerializer // .Tree
    {
        public IMemoryOwner<byte> Serialize(TreeNodeMap model)
        {
            string wire = model.Convert();
            int length = Encoding.UTF8.GetMaxByteCount(wire.Length); // Utf8 is 1-4 bpc

            IMemoryOwner<byte> rented = _pool.Rent(length);

            unsafe
            {
                fixed (char* wa = wire)
                {
                    MemoryHandle p = rented.Memory.Pin();
                    length = Encoding.UTF8.GetBytes(wa, wire.Length, (byte*)p.Pointer, length);
                }
            }

            IMemoryOwner<byte> slice = rented.WrapSlice(0, length);
            return slice;
        }

        public TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) return default;

            string text;
            unsafe
            {
                fixed (byte* ba = span)
                {
                    text = Encoding.UTF8.GetString(ba, span.Length);
                }
            }

            TreeNodeMap model = text.ConvertTree();
            return model;
        }
    }
}
