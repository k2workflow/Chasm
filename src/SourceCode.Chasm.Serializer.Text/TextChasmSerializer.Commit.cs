#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Chasm.IO.Text.Wire;
using SourceCode.Clay.Buffers;
using System;
using System.Text;

namespace SourceCode.Chasm.IO.Text
{
    partial class TextChasmSerializer // .Commit
    {
        #region Serialize

        public BufferSession Serialize(Commit model)
        {
            var wire = model.Convert();

            var maxLen = Encoding.UTF8.GetMaxByteCount(wire.Length); // Utf8 is 1-4 bpc
            var rented = BufferSession.RentBuffer(maxLen);

            var count = Encoding.UTF8.GetBytes(wire, 0, wire.Length, rented, 0);

            var seg = new ArraySegment<byte>(rented, 0, count);
            var session = new BufferSession(seg);
            return session;
        }

        public Commit DeserializeCommit(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) throw new ArgumentNullException(nameof(span));

            string text;
            unsafe
            {
                fixed (byte* ptr = &span.DangerousGetPinnableReference())
                {
                    text = Encoding.UTF8.GetString(ptr, span.Length);
                }
            }

            var model = text.ConvertCommit();
            return model;
        }

        #endregion
    }
}
