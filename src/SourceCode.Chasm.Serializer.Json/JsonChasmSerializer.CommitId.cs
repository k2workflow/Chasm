using System;
using System.Buffers;
using System.Text;
using SourceCode.Chasm.Serializer.Json.Wire;

namespace SourceCode.Chasm.Serializer.Json
{
    partial class JsonChasmSerializer // .CommitId
    {
        public IMemoryOwner<byte> Serialize(CommitId model, out int length)
        {
            string json = model.Write();

            int maxLen = Encoding.UTF8.GetMaxByteCount(json.Length); // Utf8 is 1-4 bpc
            IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(maxLen);

            length = Encoding.UTF8.GetBytes(json, owner.Memory.Span);

            return owner;
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
