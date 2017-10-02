using Google.Protobuf;
using SourceCode.Chasm.IO.Proto.Wire;
using SourceCode.Clay.Buffers;
using System;

namespace SourceCode.Chasm.IO.Proto
{
    partial class ProtoChasmSerializer // .Tree
    {
        #region Serialize

        public override BufferSession Serialize(TreeNodeList model)
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

        public override TreeNodeList DeserializeTree(ReadOnlySpan<byte> span)
        {
            var wire = new TreeWire();
            wire.MergeFrom(span.ToArray()); // TODO: Perf

            var model = wire.Convert();
            return model;
        }

        #endregion
    }
}
