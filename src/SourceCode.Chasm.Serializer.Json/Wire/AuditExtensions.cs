using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SourceCode.Clay.Json;

namespace SourceCode.Chasm.Serializer.Json.Wire
{
    internal static class AuditExtensions
    {
        private const string _name = "name";
        private const string _time = "time";

        public static Audit ReadAudit(this JsonReader jr)
        {
            Debug.Assert(jr != null);

            string name = default;
            DateTimeOffset time = default;

            jr.ReadObject(n =>
            {
                switch (n)
                {
                    case _name:
                        name = (string)jr.Value;
                        return true;

                    case _time:
                        time = ReadTime();
                        return true;
                }

                return false;
            });

            return (name == null && time == default) ? default : new Audit(name, time);

            // Local functions

            DateTimeOffset ReadTime()
            {
                string str = (string)jr.Value;
                if (string.IsNullOrEmpty(str))
                    return default;

                return DateTimeOffset.ParseExact(str, "o", CultureInfo.InvariantCulture);
            }
        }

        public static void Write(this JsonWriter jw, Audit model)
        {
            Debug.Assert(jw != null);

            if (model == default)
            {
                jw.WriteNull();
                return;
            }

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
    }
}
