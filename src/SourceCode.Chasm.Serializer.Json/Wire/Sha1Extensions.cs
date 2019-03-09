using System.Diagnostics;
using Newtonsoft.Json;
using SourceCode.Clay;

namespace SourceCode.Chasm.Serializer.Json.Wire
{
    internal static class Sha1Extensions
    {
        /// <summary>
        /// Reads a <see cref="string"/> and, if not <see langword=Constants.JsonNull/>, parses it as a nullable <see cref="Sha1"/>.
        /// </summary>
        /// <param name="jr"></param>
        /// <returns>The Sha1 value or null.</returns>
        public static Sha1? ReadSha1Nullable(this JsonReader jr)
        {
            Debug.Assert(jr != null);

            // Caller decides how to handle null
            if (jr.TokenType == JsonToken.Null) return null;

            string str = (string)jr.Value;
            if (string.IsNullOrEmpty(str))
                return null;

            return Sha1.Parse(str);
        }

        /// <summary>
        /// Reads a <see cref="string"/> and, if not <see langword=Constants.JsonNull/>, parses it as a <see cref="Sha1"/>.
        /// </summary>
        /// <param name="jr"></param>
        /// <returns>The Sha1 value.</returns>
        public static Sha1 ReadSha1(this JsonReader jr)
        {
            Debug.Assert(jr != null);

            string str = (string)jr.Value;
            return Sha1.Parse(str);
        }
    }
}
