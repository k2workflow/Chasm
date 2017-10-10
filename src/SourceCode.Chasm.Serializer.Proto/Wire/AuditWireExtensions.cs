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
            if (model == Audit.Empty)
                return new AuditWire() { Name = string.Empty };

            // Convert System.DateTimeOffset to Unix ms
            var ms = model.Timestamp.ToUniversalTime().ToUnixTimeMilliseconds();

            var wire = new AuditWire
            {
                // Time (milliseconds)
                Time = ms,

                // Offset (minutes)
                Offset = (int)model.Timestamp.Offset.TotalMinutes,

                // Name
                Name = model.Name
            };

            return wire;
        }

        public static Audit Convert(this AuditWire wire)
        {
            if (wire == null) return default;

            // Time (milliseconds)
            var time = DateTimeOffset.FromUnixTimeMilliseconds(wire.Time);

            // Offset (minutes)
            var offset = TimeSpan.FromMinutes(wire.Offset);

            // Convert Unix ms to System.DateTimeOffset
            time = time.ToOffset(offset);

            var model = new Audit(wire.Name, time);
            return model;
        }

        #endregion
    }
}
