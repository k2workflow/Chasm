using Bond;
using Bond.IO.Safe;
using Bond.Protocols;
using SourceCode.Chasm.IO.Bond.Wire;
using SourceCode.Clay.Buffers;
using System;

namespace SourceCode.Chasm.IO.Bond
{
    partial class BondChasmSerializer // .Tree
    {
        #region Constants

        private static readonly Serializer<SimpleBinaryWriter<OutputBuffer>> _treeSerializer = new Serializer<SimpleBinaryWriter<OutputBuffer>>(typeof(TreeWire));
        private static readonly Deserializer<SimpleBinaryReader<InputBuffer>> _treeDeserializer = new Deserializer<SimpleBinaryReader<InputBuffer>>(typeof(TreeWire));

        #endregion

        #region Serialize

        public override BufferSession Serialize(TreeNodeList model)
        {
            var wire = model.Convert();

            var size = wire.EstimateBytes();
            var buffer = BufferSession.RentBuffer(size);

            var buf = new OutputBuffer(buffer);
            var writer = new SimpleBinaryWriter<OutputBuffer>(buf);
            _treeSerializer.Serialize(wire, writer);

            var session = new BufferSession(buffer, buf.Data);
            return session;
        }

        #endregion

        #region Deserialize

        public override TreeNodeList DeserializeTree(ReadOnlyMemory<byte> buffer)
        {
            var buf = new InputBuffer(buffer.ToArray()); // TODO: Perf
            var reader = new SimpleBinaryReader<InputBuffer>(buf);

            var wire = _treeDeserializer.Deserialize<TreeWire>(reader);

            var model = wire.Convert();
            return model;
        }

        public override TreeNodeList DeserializeTree(ArraySegment<byte> segment)
        {
            var buf = new InputBuffer(segment);
            var reader = new SimpleBinaryReader<InputBuffer>(buf);

            var wire = _treeDeserializer.Deserialize<TreeWire>(reader);

            var model = wire.Convert();
            return model;
        }

        #endregion
    }
}
