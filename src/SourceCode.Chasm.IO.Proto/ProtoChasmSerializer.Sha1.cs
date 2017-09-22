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

        public unsafe override Sha1 DeserializeSha1(ReadOnlyMemory<byte> buffer)
        {
            var wire = new Sha1Wire();
            wire.MergeFrom(buffer.ToArray()); // TODO: Perf

            var model = wire.Convert();
            return model;
        }

        public unsafe override Sha1 DeserializeSha1(ArraySegment<byte> segment)
        {
            var wire = new Sha1Wire();
            wire.MergeFrom(segment.ToArray()); // TODO: Perf

            var model = wire.Convert();
            return model;
        }

        #endregion
    }
}
