using System;
using System.Globalization;

namespace SourceCode.Chasm.Serializer.Text.Wire
{
    internal static class AuditExtensions
    {
        public static string Convert(this Audit model)
        {
            // Convert System.DateTimeOffset to Unix ms

            // DateTime (ticks)
            long dt = model.Timestamp.UtcDateTime.Ticks; // Convert to Utc

            // Offset (ticks)
            long tz = model.Timestamp.Offset.Ticks;

            string wire = $"{model.Name} {dt} {tz}";
            return wire;
        }

        public static Audit ConvertAudit(this string wire)
        {
            if (wire == null) return default;

            DateTime dt = default;
            TimeSpan tz = default;

            int len = wire.Length;
            bool foundOffset = false;

            int ix = len - 1;
            for (; ix >= 0; ix--)
            {
                if (wire[ix] != ' ') continue;

                if (foundOffset)
                {
                    // DateTime (ticks)
                    string str = wire.Substring(ix, len - ix).TrimEnd();
                    long tcks = long.Parse(str, CultureInfo.InvariantCulture);
                    dt = new DateTime(tcks, DateTimeKind.Utc);

                    break;
                }

                // Offset (ticks)
                string stt = wire.Substring(ix, len - ix).TrimEnd();
                long ticks = long.Parse(stt, CultureInfo.InvariantCulture);
                tz = new TimeSpan(ticks);

                foundOffset = true;
                len = ix;
            }

            // DateTimeOffset
            DateTimeOffset dto = new DateTimeOffset(dt).ToOffset(tz);

            // Name
            string name = wire.Substring(0, ix);

            return new Audit(name, dto);
        }
    }
}
