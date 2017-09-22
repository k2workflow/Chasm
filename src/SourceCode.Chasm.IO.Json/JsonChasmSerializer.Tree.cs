using SourceCode.Chasm.IO.Json.Wire;
using SourceCode.Clay.Buffers;
using System;
using System.Text;

namespace SourceCode.Chasm.IO.Json
{
    partial class JsonChasmSerializer // .Tree
    {
        #region Serialize

        public override BufferSession Serialize(TreeNodeList model)
        {
            var wire = model.Convert();
            var json = wire?.ToString() ?? "null";

            var utf8 = Encoding.UTF8.GetBytes(json);

            var session = new BufferSession(new ArraySegment<byte>(utf8));
            return session;
        }

        #endregion

        #region Deserialize

        public override TreeNodeList DeserializeTree(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty) throw new ArgumentNullException(nameof(span));

            string json;
            unsafe
            {
                fixed (byte* ptr = &span.DangerousGetPinnableReference())
                {
                    json = Encoding.UTF8.GetString(ptr, span.Length);
                }
            }

            var model = json.ParseTree();
            return model;
        }

        #endregion
    }
}
