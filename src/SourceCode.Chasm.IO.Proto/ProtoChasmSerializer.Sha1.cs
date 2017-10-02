using Google.Protobuf;
using SourceCode.Chasm.IO.Proto.Wire;
using SourceCode.Clay.Buffers;
using System;

namespace SourceCode.Chasm.IO.Proto
{
    partial class ProtoChasmSerializer // .Sha1
    {
        #region Serialize

        public override BufferSession Serialize(Sha1 model)
        {
            var wire = model.Convert();
            if (wire == null)
                return new BufferSession(Array.Empty<byte>(), new ArraySegment<byte>(Array.Empty<byte>(), 0, 0));

            var size = wire.CalculateSize();
            var buffer = BufferSession.RentBuffer(size);

            using (var cos = new CodedOutputStream(buffer))
            {
                wire.WriteTo(cos);

                var segment = new ArraySegment<byte>(buffer, 0, (int)cos.Position);

                var session = new BufferSession(buffer, segment);
                return session;
            }
        }

        #endregion

        #region Deserialize

        public unsafe override Sha1 DeserializeSha1(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty) return default;

            var wire = new Sha1Wire();
            wire.MergeFrom(span.ToArray()); // TODO: Perf

            var model = wire.Convert();
            return model;
        }

        #endregion
    }
}
