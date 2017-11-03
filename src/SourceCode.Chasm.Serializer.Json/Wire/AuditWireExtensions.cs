#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Newtonsoft.Json.Linq;
using System;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class AuditWireExtensions
    {
        #region Constants

        // Naming follows convention in ProtoSerializer

        private const string _name = "name";
        private const string _time = "time";

        #endregion

        #region Methods

        public static JObject Convert(this Audit model)
        {
            if (model == Audit.Empty) return default; // null

            // Name
            var name = new JValue(model.Name);

            // Time
            var time = new JValue(model.Timestamp.UtcDateTime);

            var wire = new JObject
            {
                [_name] = name,
                [_time] = time,
            };

            return wire;
        }

        public static Audit ConvertAudit(this JObject wire)
        {
            if (wire == null) return default;

            // Name
            string name = null;
            if (wire.TryGetValue(_name, out var jv)
                && jv != null)
            {
                name = (string)jv;
            }

            // Time
            DateTime time = default;
            if (wire.TryGetValue(_time, out jv)
                && jv != null)
            {
                // TODO: Newtonsoft is very opinionated on DateTimeOffset formatting, I can't get it to roundtrip
                var dt = (DateTime)jv;
                time = new DateTime(dt.Ticks, DateTimeKind.Utc);
            }

            if (name == null && time == default) return default;

            var model = new Audit(name, new DateTimeOffset(time));
            return model;
        }

        #endregion
    }
}
