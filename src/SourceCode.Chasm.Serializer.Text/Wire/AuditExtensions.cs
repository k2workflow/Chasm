#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Globalization;

namespace SourceCode.Chasm.IO.Text.Wire
{
    internal static class AuditExtensions
    {
        #region Methods

        public static string Convert(this Audit model)
        {
            // Convert System.DateTimeOffset to Unix ms

            // DateTime (ticks)
            var dt = model.Timestamp.UtcDateTime.Ticks; // Convert to Utc

            // Offset (ticks)
            var tz = model.Timestamp.Offset.Ticks;

            var wire = $"{model.Name} {dt} {tz}";
            return wire;
        }

        public static Audit ConvertAudit(this string wire)
        {
            if (wire == null) return default;

            DateTime dt = default;
            TimeSpan tz = default;

            var len = wire.Length;
            var foundOffset = false;

            var ix = len - 1;
            for (; ix >= 0; ix--)
            {
                if (wire[ix] != ' ') continue;

                if (foundOffset)
                {
                    // DateTime (ticks)
                    var str = wire.Substring(ix, len - ix).TrimEnd();
                    var tcks = long.Parse(str, CultureInfo.InvariantCulture);
                    dt = new DateTime(tcks, DateTimeKind.Utc);

                    break;
                }

                // Offset (ticks)
                var stt = wire.Substring(ix, len - ix).TrimEnd();
                var ticks = long.Parse(stt, CultureInfo.InvariantCulture);
                tz = new TimeSpan(ticks);

                foundOffset = true;
                len = ix;
            }

            // DateTimeOffset
            var dto = new DateTimeOffset(dt).ToOffset(tz);

            // Name
            var name = wire.Substring(0, ix);

            return new Audit(name, dto);
        }

        #endregion
    }
}
