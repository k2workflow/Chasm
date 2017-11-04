#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Newtonsoft.Json;
using SourceCode.Clay.Json;
using System;
using System.Globalization;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class AuditExtensions
    {
        #region Constants

        // Naming follows convention in ProtoSerializer

        private const string _name = "name";
        private const string _time = "time";

        #endregion

        #region Read

        public static Audit ReadAudit(this JsonReader jr)
        {
            string name = default;
            DateTimeOffset time = default;

            // Switch
            return jr.ReadObject(n =>
            {
                switch (n)
                {
                    case _name:
                        name = (string)jr.Value;
                        break;

                    case _time:
                        time = ReadTime();
                        break;
                }
            },

            // Factory
            () => name == null && time == default ? default : new Audit(name, time));

            // Property

            DateTimeOffset ReadTime()
            {
                var str = (string)jr.Value;
                if (string.IsNullOrEmpty(str))
                    return default;

                return DateTimeOffset.ParseExact(str, "o", CultureInfo.InvariantCulture);
            }
        }

        #endregion

        #region Write

        public static void Write(this JsonWriter jw, Audit model)
        {
            if (model == Audit.Empty) return; // null

            jw.WriteStartObject();
            {
                // Name
                jw.WritePropertyName(_name);
                jw.WriteValue(model.Name);

                // Time
                jw.WritePropertyName(_time);
                jw.WriteValue(model.Timestamp.ToString("o", CultureInfo.InvariantCulture));
            }
            jw.WriteEndObject();
        }

        #endregion
    }
}
