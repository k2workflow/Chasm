#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System.Buffers;

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class Sha1WireExtensions
    {
        #region Methods

        public static Sha1Wire Convert(this Sha1 model)
        {
            Sha1Wire wire;

            var array = ArrayPool<byte>.Shared.Rent(Sha1.ByteLen);
            {
                model.CopyTo(array);

                wire = new Sha1Wire
                {
                    Set = true,
                    Data = Google.Protobuf.ByteString.CopyFrom(array, 0, Sha1.ByteLen)
                };
            }
            ArrayPool<byte>.Shared.Return(array);

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

            var array = ArrayPool<byte>.Shared.Rent(Sha1.ByteLen);
            {
                wire.Data.CopyTo(array, 0);

                model = new Sha1(array);
            }
            ArrayPool<byte>.Shared.Return(array);

            return model;
        }

        public static BlobId? ConvertBlob(this Sha1Wire wire)
        {
            if (wire == null || !wire.Set) return default;

            var model = Convert(wire);
            return new BlobId(model.Value);
        }

        public static CommitId? ConvertCommit(this Sha1Wire wire)
        {
            if (wire == null || !wire.Set) return default;

            var model = Convert(wire);
            return new CommitId(model.Value);
        }

        public static TreeId? ConvertTree(this Sha1Wire wire)
        {
            if (wire == null || !wire.Set) return default;

            var model = Convert(wire);
            return new TreeId(model.Value);
        }

        #endregion
    }
}
