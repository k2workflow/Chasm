using System;
using System.Buffers;
using System.Text;
using SourceCode.Chasm.Serializer.Json.Wire;

namespace SourceCode.Chasm.Serializer.Json
{
    partial class JsonChasmSerializer // .CommitId
    {
        public Memory<byte> Serialize(CommitId model, SessionPool<byte> pool)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));

            string json = model.Write();

            int maxLen = Encoding.UTF8.GetMaxByteCount(json.Length); // Utf8 is 1-4 bpc
            IMemoryOwner<byte> owner = pool.Rent(maxLen);

            int length = Encoding.UTF8.GetBytes(json, owner.Memory.Span);
            Memory<byte> mem = owner.Memory.Slice(0, length);

            return mem;
        }

        public CommitId DeserializeCommitId(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) throw new ArgumentNullException(nameof(span));

            string json = Encoding.UTF8.GetString(span);

            CommitId model = json.ReadCommitId();
            return model;
        }
    }
}
