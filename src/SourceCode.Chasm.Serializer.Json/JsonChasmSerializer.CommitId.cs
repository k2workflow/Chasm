using System;
using System.Buffers;
using System.Text;
using SourceCode.Chasm.Serializer.Json.Wire;
using SourceCode.Clay.Buffers;

namespace SourceCode.Chasm.Serializer.Json
{
    partial class JsonChasmSerializer // .CommitId
    {
        public IMemoryOwner<byte> Serialize(CommitId model)
        {
            string json = model.Write();
            int length = Encoding.UTF8.GetMaxByteCount(json.Length); // Utf8 is 1-4 bpc

            IMemoryOwner<byte> rented = _pool.Rent(length);

            unsafe
            {
                fixed (char* wa = json)
                {
                    MemoryHandle p = rented.Memory.Pin();
                    length = Encoding.UTF8.GetBytes(wa, json.Length, (byte*)p.Pointer, length);
                }
            }

            IMemoryOwner<byte> slice = rented.WrapSlice(0, length);
            return slice;
        }

        public CommitId DeserializeCommitId(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) throw new ArgumentNullException(nameof(span));

            string json;
            unsafe
            {
                fixed (byte* ba = span)
                {
                    json = Encoding.UTF8.GetString(ba, span.Length);
                }
            }

            CommitId model = json.ReadCommitId();
            return model;
        }
    }
}
