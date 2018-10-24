using System;
using System.IO;
using Newtonsoft.Json;
using SourceCode.Clay;

namespace SourceCode.Chasm.Serializer.Json.Wire
{
    internal static class Sha1Extensions
    {
        /// <summary>
        /// Reads a <see cref="string"/> and, if not <see langword=Constants.JsonNull/>, parses it as a <see cref="Sha1"/>.
        /// </summary>
        /// <param name="jr"></param>
        /// <returns></returns>
        public static Sha1? ReadSha1(this JsonReader jr)
        {
            if (jr == null) throw new ArgumentNullException(nameof(jr));

            string str = (string)jr.Value;
            if (string.IsNullOrEmpty(str))
                return null; // Caller decides how to handle null

            var sha1 = Sha1.Parse(str);
            return sha1;
        }

        public static Sha1? ReadSha1(this string json)
        {
            if (json == null || json == JsonConstants.JsonNull) return null;

            using (var tr = new StringReader(json))
            using (var jr = new JsonTextReader(tr))
            {
                jr.DateParseHandling = DateParseHandling.None;

                Sha1? model = ReadSha1(jr);
                return model;
            }
        }
    }
}
