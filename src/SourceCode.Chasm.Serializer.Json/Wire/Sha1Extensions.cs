using System.Diagnostics;
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
            Debug.Assert(jr != null);

            // Caller decides how to handle null
            if (jr.TokenType == JsonToken.Null) return null;

            string str = (string)jr.Value;
            if (string.IsNullOrEmpty(str))
                return null;

            var sha1 = Sha1.Parse(str);
            return sha1;
        }

        public static Sha1? ParseSha1(this string json)
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
