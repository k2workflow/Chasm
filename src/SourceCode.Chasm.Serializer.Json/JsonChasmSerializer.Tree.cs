#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Chasm.IO.Json.Wire;
using SourceCode.Clay.Buffers;
using System;
using System.Text;

namespace SourceCode.Chasm.IO.Json
{
    partial class JsonChasmSerializer // .Tree
    {
        #region Serialize

        public BufferSession Serialize(TreeNodeMap model)
        {
            var json = model.Write();

            var maxLen = Encoding.UTF8.GetMaxByteCount(json.Length); // Utf8 is 1-4 bpc
            var rented = BufferSession.Rent(maxLen).Result;

            var count = Encoding.UTF8.GetBytes(json, 0, json.Length, rented.Array, 0);

            var session = BufferSession.Rented(rented.Slice(0, count));
            return session;
        }

        #endregion

        #region Deserialize

        public TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) throw new ArgumentNullException(nameof(span));

            string json;
            unsafe
            {
                fixed (byte* ptr = &span.DangerousGetPinnableReference())
                {
                    json = Encoding.UTF8.GetString(ptr, span.Length);
                }
            }

            var model = json.ReadTreeNodeMap();
            return model;
        }

        #endregion
    }
}
