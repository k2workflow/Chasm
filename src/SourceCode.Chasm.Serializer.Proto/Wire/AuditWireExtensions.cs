#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class AuditWireExtensions
    {
        #region Methods

        public static AuditWire Convert(this Audit model)
        {
            var wire = new AuditWire
            {
                // Name
                Name = model.Name,

                // DateTime (ticks)
                DateTime = model.Timestamp.UtcDateTime.Ticks, // Convert to Utc

                // Offset (ticks)
                Offset = model.Timestamp.Offset.Ticks
            };

            return wire;
        }

        public static Audit Convert(this AuditWire wire)
        {
            // Name
            var name = wire.Name ?? string.Empty;

            // DateTime (ticks)
            var dt = new DateTime(wire.DateTime, DateTimeKind.Utc); // Utc

            // Offset (ticks)
            var tz = new TimeSpan(wire.Offset);

            // DateTimeOffset
            var dto = new DateTimeOffset(dt).ToOffset(tz);

            var model = new Audit(name, dto);
            return model;
        }

        #endregion
    }
}
