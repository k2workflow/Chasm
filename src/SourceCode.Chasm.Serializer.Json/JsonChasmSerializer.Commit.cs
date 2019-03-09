using System;
using System.Buffers;
using SourceCode.Chasm.Serializer.Json.Wire;

namespace SourceCode.Chasm.Serializer.Json
{
    partial class JsonChasmSerializer // .Commit
    {
        public IMemoryOwner<byte> Serialize(Commit model)
        {
            string json = model.Write();
            return ToUtf8(json);
        }

        public Commit DeserializeCommit(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) throw new ArgumentNullException(nameof(span));

            string json = GetUtf8(span);
            return json.ReadCommit();
        }
    }
}
