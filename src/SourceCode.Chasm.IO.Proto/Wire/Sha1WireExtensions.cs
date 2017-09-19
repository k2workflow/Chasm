using SourceCode.Clay;
using System.Runtime.CompilerServices;

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class Sha1WireExtensions
    {
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
            if (wire == null) return Sha1.Empty;

            var model = new Sha1(wire.Blit0, wire.Blit1, wire.Blit2);
            return model;
        }
    }
}
