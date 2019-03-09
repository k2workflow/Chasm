using System;
using System.Buffers;
using SourceCode.Chasm.Serializer.Json.Wire;

namespace SourceCode.Chasm.Serializer.Json
{
    partial class JsonChasmSerializer // .Tree
    {
        public IMemoryOwner<byte> Serialize(TreeNodeMap model)
        {
            string json = model.Write();
            return ToUtf8(json);
        }

        public TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) throw new ArgumentNullException(nameof(span));

            string json = GetUtf8(span);
            return json.ReadTreeNodeMap();
        }
    }
}
