using System;
using System.Buffers;
using System.Text;
using SourceCode.Chasm.Serializer.Json.Wire;
using SourceCode.Clay.Buffers;

namespace SourceCode.Chasm.Serializer.Json
{
    partial class JsonChasmSerializer // .Tree
    {
        public IMemoryOwner<byte> Serialize(TreeNodeMap model)
        {
            string json = model.Write();
            int length = Encoding.UTF8.GetMaxByteCount(json.Length); // Utf8 is 1-4 bpc

            IMemoryOwner<byte> rented = _pool.Rent(length);
#if !NETSTANDARD2_0
            length = Encoding.UTF8.GetBytes(json, rented.Memory.Span);
#else
            unsafe
            {
                fixed (char* wa = json)
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
            if (span.Length == 0) throw new ArgumentNullException(nameof(span));

            string json;
#if !NETSTANDARD2_0
            json = Encoding.UTF8.GetString(span);
#else
            unsafe
            {
                fixed (byte* ba = span)
                {
                    json = Encoding.UTF8.GetString(ba, span.Length);
                }
            }
#endif

            TreeNodeMap model = json.ReadTreeNodeMap();
            return model;
        }
    }
}
