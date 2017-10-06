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
                Blit0 = model.Blit0,
                Blit1 = model.Blit1,
                Blit2 = model.Blit2
            };

            return wire;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Sha1 Convert(this Sha1Wire wire)
        {
            if (wire == null) return default;

            var model = new Sha1(wire.Blit0, wire.Blit1, wire.Blit2);
            return model;
        }

        #endregion
    }
}
