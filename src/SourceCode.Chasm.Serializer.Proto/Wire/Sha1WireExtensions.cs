#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System.Runtime.CompilerServices;

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class Sha1WireExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Sha1Wire Convert(this Sha1 model)
        {
            var wire = new Sha1Wire
            {
                Set = true,
                Blit0 = model.Blit0,
                Blit1 = model.Blit1,
                Blit2 = model.Blit2
            };

            return wire;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Sha1Wire Convert(this Sha1? model)
        {
            if (model == null) return new Sha1Wire();

            var wire = new Sha1Wire
            {
                Set = true,
                Blit0 = model.Value.Blit0,
                Blit1 = model.Value.Blit1,
                Blit2 = model.Value.Blit2
            };

            return wire;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Sha1Wire Convert(this BlobId? model) => Convert(model?.Sha1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Sha1Wire Convert(this CommitId? model) => Convert(model?.Sha1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Sha1Wire Convert(this TreeId? model) => Convert(model?.Sha1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Sha1? Convert(this Sha1Wire wire)
        {
            if (wire == null) return default;
            if (!wire.Set) return default;

            var model = new Sha1(wire.Blit0, wire.Blit1, wire.Blit2);
            return model;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlobId? ConvertBlob(this Sha1Wire wire)
        {
            if (wire == null) return default;
            if (!wire.Set) return default;

            var model = new Sha1(wire.Blit0, wire.Blit1, wire.Blit2);
            return new BlobId(model);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommitId? ConvertCommit(this Sha1Wire wire)
        {
            if (wire == null) return default;
            if (!wire.Set) return default;

            var model = new Sha1(wire.Blit0, wire.Blit1, wire.Blit2);
            return new CommitId(model);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TreeId? ConvertTree(this Sha1Wire wire)
        {
            if (wire == null) return default;
            if (!wire.Set) return default;

            var model = new Sha1(wire.Blit0, wire.Blit1, wire.Blit2);
            return new TreeId(model);
        }

        #endregion
    }
}
