using Google.Protobuf;
using SourceCode.Clay.Buffers;
using SourceCode.Mamba.CasRepo.IO.Proto.Wire;
using System;

namespace SourceCode.Mamba.CasRepo.IO.Proto
{
    partial class ProtoCasSerializer // .Commit
    {
        #region Serialize

        public override BufferSession Serialize(Commit model)
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

        public override Commit DeserializeCommit(ReadOnlyBuffer<byte> buffer)
        {
            var wire = new CommitWire();
            wire.MergeFrom(buffer.ToArray()); // TODO: Perf

            var model = wire.Convert();
            return model;
        }

        public override Commit DeserializeCommit(ArraySegment<byte> segment)
        {
            var wire = new CommitWire();
            wire.MergeFrom(segment.ToArray()); // TODO: Perf

            var model = wire.Convert();
            return model;
        }

        #endregion
    }
}
