#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Google.Protobuf.WellKnownTypes;
using System;

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class AuditWireExtensions
    {
        #region Constants

        // Windows epoch 1601-01-01T00:00:00Z is 11,644,473,600 seconds before Unix epoch 1970-01-01T00:00:00Z
        private const long epochOffset = 11_644_473_600;

        #endregion

        #region Methods

        public static AuditWire Convert(this Audit model)
        {
            if (model == Audit.Empty)
                return new AuditWire() { Name = string.Empty };

            var utc = model.Timestamp.ToUniversalTime();

            var wire = new AuditWire
            {
                // Convert System.DateTimeOffset to Google.Protobuf.WellKnownTypes.Timestamp

                // Time
                Time = new Timestamp
                {
                    Seconds = (utc.Ticks / TimeSpan.TicksPerSecond) - epochOffset,
                    Nanos = (int)(utc.Ticks % TimeSpan.TicksPerSecond) * 100 // Windows tick is 100 nanoseconds
                },

                // Offset
                Offset = (int)model.Timestamp.Offset.TotalMinutes,

                // Name
                Name = model.Name
            };

            return wire;
        }

        public static Audit Convert(this AuditWire wire)
        {
            if (wire == null) return default;

            // Convert Google.Protobuf.WellKnownTypes.Timestamp to System.DateTimeOffset

            // Time
            long ticks = 0;
            if (wire.Time != null)
            {
                ticks = (wire.Time.Seconds + epochOffset) * TimeSpan.TicksPerSecond;
                ticks += wire.Time.Nanos / 100; // Windows tick is 100 nanoseconds
            }

            // Offset
            var offset = TimeSpan.FromMinutes(wire.Offset);

            var time = new DateTimeOffset(new DateTime(ticks, DateTimeKind.Utc));
            time = time.ToOffset(offset);

            var model = new Audit(wire.Name, time);
            return model;
        }

        #endregion
    }
}
