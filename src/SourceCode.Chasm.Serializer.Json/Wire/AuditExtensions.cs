using System;
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
            if (jr == null) throw new ArgumentNullException(nameof(jr));

            string name = default;
            DateTimeOffset time = default;

            // Switch
            return jr.ReadObject(n =>
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
            },

            // Factory
            () => name == null && time == default ? default : new Audit(name, time));

            // Property

            DateTimeOffset ReadTime()
            {
                string str = (string)jr.Value;
                if (string.IsNullOrEmpty(str))
                    return default;

                return DateTimeOffset.ParseExact(str, "o", CultureInfo.InvariantCulture);
            }
        }

        public static Audit ReadAudit(this string json)
        {
            if (json == null || json == JsonConstants.JsonNull) return default;

            using (var tr = new StringReader(json))
            using (var jr = new JsonTextReader(tr))
            {
                jr.DateParseHandling = DateParseHandling.None;

                Audit model = ReadAudit(jr);
                return model;
            }
        }

        public static void Write(this JsonWriter jw, Audit model)
        {
            if (jw == null) throw new ArgumentNullException(nameof(jw));

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

        public static string Write(this Audit model)
        {
            if (model == default) return JsonConstants.JsonNull;

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var jw = new JsonTextWriter(sw))
            {
                Write(jw, model);
            }

            return sb.ToString();
        }
    }
}
