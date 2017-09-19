﻿using SourceCode.Chasm.IO.Json.Wire;
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

        public override TreeNodeList DeserializeTree(ReadOnlyBuffer<byte> buffer)
        {
            if (buffer.IsEmpty) throw new ArgumentNullException(nameof(buffer));

            string json;
            unsafe
            {
                fixed (byte* ptr = &buffer.Span.DangerousGetPinnableReference())
                {
                    json = Encoding.UTF8.GetString(ptr, buffer.Length);
                }
            }

            var model = json.ParseTree();
            return model;
        }

        public override TreeNodeList DeserializeTree(ArraySegment<byte> segment)
        {
            var json = Encoding.UTF8.GetString(segment.Array, segment.Offset, segment.Count);

            var model = json.ParseTree();
            return model;
        }

        #endregion
    }
}