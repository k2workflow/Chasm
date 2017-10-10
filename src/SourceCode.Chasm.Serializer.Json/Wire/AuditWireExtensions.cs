#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Json;
using System;
using System.Globalization;
using System.Json;

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

        public static JsonObject Convert(this Audit model)
        {
            if (model == Audit.Empty) return default; // null

            // Name
            var name = new JsonPrimitive(model.Name);

            // Time
            var time = model.Timestamp.ToString("o", CultureInfo.InvariantCulture);

            var wire = new JsonObject
            {
                [_name] = name,
                [_time] = time,
            };

            return wire;
        }

        public static Audit ConvertAudit(this JsonObject wire)
        {
            if (wire == null) return default;

            // Name
            string name = null;
            if (wire.TryGetValue(_name, JsonType.String, true, out var jv))
                name = jv;

            // Time
            string str = null;
            if (wire.TryGetValue(_time, JsonType.String, true, out jv))
                str = jv;

            var time = DateTimeOffset.ParseExact(str, "o", CultureInfo.InvariantCulture);

            var model = new Audit(name, time);
            return model;
        }

        #endregion
    }
}
