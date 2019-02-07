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
#if !NETSTANDARD2_0
            length = Encoding.UTF8.GetBytes(wire, rented.Memory.Span);
#else
            unsafe
            {
                fixed (char* wa = wire)
                {
                    MemoryHandle p = rented.Memory.Pin();
                    length = Encoding.UTF8.GetBytes(wa, 0, (byte*)p.Pointer, 0);
                }
            }
#endif

            IMemoryOwner<byte> slice = rented.WrapSlice(0, length);
            return slice;
        }

        public TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) return default;

            string text;
#if !NETSTANDARD2_0
            text = Encoding.UTF8.GetString(span);
#else
            unsafe
            {
                fixed (byte* ba = span)
                {
                    text = Encoding.UTF8.GetString(ba, span.Length);
                }
            }
#endif

            TreeNodeMap model = text.ConvertTree();
            return model;
        }
    }
}
