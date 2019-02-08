using System;
using System.Buffers;
using System.Text;
using SourceCode.Chasm.Serializer.Text.Wire;
using SourceCode.Clay.Buffers;

namespace SourceCode.Chasm.Serializer.Text
{
    partial class TextChasmSerializer // .Commit
    {
        public IMemoryOwner<byte> Serialize(Commit model)
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

        public Commit DeserializeCommit(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) throw new ArgumentNullException(nameof(span));

            string text;
            unsafe
            {
                fixed (byte* ba = span)
                {
                    text = Encoding.UTF8.GetString(ba, span.Length);
                }
            }

            Commit model = text.ConvertCommit();
            return model;
        }
    }
}
