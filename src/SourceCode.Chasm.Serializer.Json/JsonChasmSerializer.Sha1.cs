#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Buffers;
using System;
using System.Diagnostics;
using System.Text;

namespace SourceCode.Chasm.IO.Json
{
    partial class JsonChasmSerializer // .Sha1
    {
        #region Serialize

        public BufferSession Serialize(Sha1 model)
        {
            // Do not add braces or other formatting. This is a concise representation.

            var json = model.ToString("N"); // Most concise format specifier

            // Sha1 => 20 bytes => 40 chars => 40 utf8 codepoints, all within ASCII range => 40 x 1bpc => 40 bytes
            var rented = BufferSession.RentBuffer(Sha1.CharLen); // 40
            var count = Encoding.UTF8.GetBytes(json, 0, json.Length, rented, 0);

            var seg = new ArraySegment<byte>(rented, 0, count);
            var session = new BufferSession(seg);
            return session;
        }

        #endregion

        #region Deserialize

        public Sha1 DeserializeSha1(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty) throw new ArgumentNullException(nameof(span));
            Debug.Assert(span.Length >= Sha1.CharLen); // 40

            string json;
            unsafe
            {
                fixed (byte* ptr = &span.DangerousGetPinnableReference())
                {
                    // Sha1 => 20 bytes => 40 chars => 40 utf8 codepoints, all within ASCII range => 40 x 1bpc => 40 bytes
                    json = Encoding.UTF8.GetString(ptr, Sha1.CharLen); // 40
                }
            }

            var model = Sha1.Parse(json);
            return model;
        }

        #endregion
    }
}
