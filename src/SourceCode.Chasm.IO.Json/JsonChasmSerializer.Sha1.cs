using SourceCode.Clay.Buffers;
using System;
using System.Text;

namespace SourceCode.Chasm.IO.Json
{
    partial class JsonChasmSerializer // .Sha1
    {
        #region Serialize

        public override BufferSession Serialize(Sha1 model)
        {
            var json = model.ToString("N");

            var rented = BufferSession.RentBuffer(json.Length * 3); // Utf8 is 1-3 bpc
            var count = Encoding.UTF8.GetBytes(json, 0, json.Length, rented, 0);

            var seg = new ArraySegment<byte>(rented, 0, count);
            var session = new BufferSession(seg);
            return session;
        }

        #endregion

        #region Deserialize

        public override Sha1 DeserializeSha1(ReadOnlySpan<byte> span)
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

            var model = Sha1.Parse(json);
            return model;
        }

        #endregion
    }
}
