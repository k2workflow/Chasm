using System;

namespace SourceCode.Chasm.Serializer.Proto.Wire
{
    internal static class AuditWireExtensions
    {
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
            string name = wire.Name ?? string.Empty;

            // DateTime (ticks)
            var dt = new DateTime(wire.DateTime, DateTimeKind.Utc); // Utc

            // Offset (ticks)
            var tz = new TimeSpan(wire.Offset);

            // DateTimeOffset
            DateTimeOffset dto = new DateTimeOffset(dt).ToOffset(tz);

            var model = new Audit(name, dto);
            return model;
        }
    }
}
