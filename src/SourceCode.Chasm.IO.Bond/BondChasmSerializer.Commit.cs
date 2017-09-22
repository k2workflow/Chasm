using Bond;
using Bond.IO.Safe;
using Bond.Protocols;
using SourceCode.Chasm.IO.Bond.Wire;
using SourceCode.Clay.Buffers;
using System;

namespace SourceCode.Chasm.IO.Bond
{
    partial class BondChasmSerializer // .Commit
    {
        #region Constants

        private static readonly Serializer<SimpleBinaryWriter<OutputBuffer>> _commitSerializer = new Serializer<SimpleBinaryWriter<OutputBuffer>>(typeof(CommitWire));
        private static readonly Deserializer<SimpleBinaryReader<InputBuffer>> _commitDeserializer = new Deserializer<SimpleBinaryReader<InputBuffer>>(typeof(CommitWire));

        #endregion

        #region Serialize

        public override BufferSession Serialize(Commit model)
        {
            var wire = model.Convert();

            var size = wire.EstimateBytes();
            var buffer = BufferSession.RentBuffer(size);

            var buf = new OutputBuffer(buffer);
            var writer = new SimpleBinaryWriter<OutputBuffer>(buf);
            _commitSerializer.Serialize(wire, writer);

            var session = new BufferSession(buffer, buf.Data);
            return session;
        }

        #endregion

        #region Deserialize

        public override Commit DeserializeCommit(ReadOnlyMemory<byte> buffer)
        {
            var buf = new InputBuffer(buffer.ToArray()); // TODO: Perf
            var reader = new SimpleBinaryReader<InputBuffer>(buf);

            var wire = _commitDeserializer.Deserialize<CommitWire>(reader);

            var model = wire.Convert();
            return model;
        }

        public override Commit DeserializeCommit(ArraySegment<byte> segment)
        {
            var buf = new InputBuffer(segment);
            var reader = new SimpleBinaryReader<InputBuffer>(buf);

            var wire = _commitDeserializer.Deserialize<CommitWire>(reader);

            var model = wire.Convert();
            return model;
        }

        #endregion
    }
}
