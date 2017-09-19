using Bond;
using Bond.IO.Safe;
using Bond.Protocols;
using SourceCode.Chasm.IO.Bond.Wire;
using SourceCode.Clay.Buffers;
using System;

namespace SourceCode.Chasm.IO.Bond
{
    partial class BondChasmSerializer // .Sha1
    {
        #region Constants

        private static readonly Serializer<SimpleBinaryWriter<OutputBuffer>> _sha1Serializer = new Serializer<SimpleBinaryWriter<OutputBuffer>>(typeof(Sha1Wire));
        private static readonly Deserializer<SimpleBinaryReader<InputBuffer>> _sha1Deserializer = new Deserializer<SimpleBinaryReader<InputBuffer>>(typeof(Sha1Wire));

        #endregion

        #region Serialize

        public override BufferSession Serialize(Sha1 model)
        {
            var wire = model.Convert();

            var size = wire.EstimateBytes();
            var buffer = BufferSession.RentBuffer(size);

            var buf = new OutputBuffer(buffer);
            var writer = new SimpleBinaryWriter<OutputBuffer>(buf);
            _sha1Serializer.Serialize(wire, writer);

            var session = new BufferSession(buffer, buf.Data);
            return session;
        }

        #endregion

        #region Deserialize

        public override Sha1 DeserializeSha1(ReadOnlyBuffer<byte> buffer)
        {
            var buf = new InputBuffer(buffer.ToArray()); // TODO: Perf
            var reader = new SimpleBinaryReader<InputBuffer>(buf);

            var wire = _sha1Deserializer.Deserialize<Sha1Wire>(reader);

            var model = wire.Convert();
            return model;
        }

        public override Sha1 DeserializeSha1(ArraySegment<byte> segment)
        {
            var buf = new InputBuffer(segment);
            var reader = new SimpleBinaryReader<InputBuffer>(buf);

            var wire = _sha1Deserializer.Deserialize<Sha1Wire>(reader);

            var model = wire.Convert();
            return model;
        }

        #endregion
    }
}
