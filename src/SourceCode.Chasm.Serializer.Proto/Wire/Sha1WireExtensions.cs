using System;
using System.Buffers;
using SourceCode.Clay;

namespace SourceCode.Chasm.Serializer.Proto.Wire
{
    internal static class Sha1WireExtensions
    {
        public static Sha1Wire Convert(this Sha1 model)
        {
            Sha1Wire wire;

            byte[] rented = ArrayPool<byte>.Shared.Rent(Sha1.ByteLength);
            {
                model.CopyTo(rented);

                wire = new Sha1Wire
                {
                    Set = true,
                    Data = Google.Protobuf.ByteString.CopyFrom(rented, 0, Sha1.ByteLength)
                };
            }
            ArrayPool<byte>.Shared.Return(rented);

            return wire;
        }

        public static Sha1Wire Convert(this Sha1? model) => model == null ? new Sha1Wire() : Convert(model.Value);

        public static Sha1Wire Convert(this BlobId? model) => Convert(model?.Sha1);

        public static Sha1Wire Convert(this CommitId? model) => Convert(model?.Sha1);

        public static Sha1Wire Convert(this TreeId? model) => Convert(model?.Sha1);

        public static Sha1? Convert(this Sha1Wire wire)
        {
            if (wire == null || !wire.Set) return default;

            Sha1 model;

            byte[] rented = ArrayPool<byte>.Shared.Rent(Sha1.ByteLength);
            {
                wire.Data.CopyTo(rented, 0);

                model = new Sha1(new ReadOnlySpan<byte>(rented));
            }
            ArrayPool<byte>.Shared.Return(rented);

            return model;
        }

        public static BlobId? ConvertBlob(this Sha1Wire wire)
        {
            if (wire == null || !wire.Set) return default;

            Sha1? model = Convert(wire);
            return new BlobId(model.Value);
        }

        public static CommitId? ConvertCommit(this Sha1Wire wire)
        {
            if (wire == null || !wire.Set) return default;

            Sha1? model = Convert(wire);
            return new CommitId(model.Value);
        }

        public static TreeId? ConvertTree(this Sha1Wire wire)
        {
            if (wire == null || !wire.Set) return default;

            Sha1? model = Convert(wire);
            return new TreeId(model.Value);
        }
    }
}
