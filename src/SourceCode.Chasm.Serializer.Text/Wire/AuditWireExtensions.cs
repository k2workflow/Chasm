#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Globalization;

namespace SourceCode.Chasm.IO.Text.Wire
{
    internal static class AuditWireExtensions
    {
        #region Methods

        public static string Convert(this Audit model)
        {
            if (model == Audit.Empty) return default; // null

            var utc = model.Timestamp.ToUniversalTime();
            var ms = utc.ToUnixTimeMilliseconds();

            var tz = model.Timestamp.Offset.TotalMinutes;

            var wire = $"{model.Name} {ms} {tz:0000}";
            return wire;
        }

        public static Audit ConvertAudit(this string wire)
        {
            if (wire == null) return default;

            DateTimeOffset utc = default;
            TimeSpan offset = default;

            var len = wire.Length;
            var foundFirst = false;
            var ix = len - 1;
            for (; ix >= 0; ix--)
            {
                if (wire[ix] != ' ') continue;

                if (foundFirst)
                {
                    var str = wire.Substring(ix, len - ix).TrimEnd();
                    var ms = long.Parse(str);
                    utc = DateTimeOffset.FromUnixTimeMilliseconds(ms);
                    break;
                }

                var stt = wire.Substring(ix, len - ix).TrimEnd();
                var minutes = int.Parse(stt);
                offset = TimeSpan.FromMinutes(minutes);
                foundFirst = true;
                len = ix;
            }

            var name = wire.Substring(0, ix);
            utc = utc.ToOffset(offset);
            return new Audit(name, utc);
        }

        #endregion
    }
}
