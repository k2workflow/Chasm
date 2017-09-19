using System;
using System.Runtime.CompilerServices;

namespace SourceCode.Chasm.IO.Bond.Wire
{
    public static partial class BondTypeAliasConverter // .DateTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Convert(DateTime value, long unused)
            => value.Ticks;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime Convert(long value, DateTime unused)
            => new DateTime(value, DateTimeKind.Utc);
    }
}
