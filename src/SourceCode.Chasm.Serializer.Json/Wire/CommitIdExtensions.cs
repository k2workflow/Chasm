using System;
using System.IO;
using Newtonsoft.Json;
using SourceCode.Clay;
using SourceCode.Clay.Json;

namespace SourceCode.Chasm.Serializer.Json.Wire
{
    internal static class CommitIdExtensions
    {
        private const string _id = "id";

        public static CommitId ReadCommitId(this JsonReader jr)
        {
            if (jr == null) throw new ArgumentNullException(nameof(jr));

            Sha1 sha1 = default;

            // Switch
            return jr.ReadObject(n =>
            {
                switch (n)
                {
                    case _id:
                        sha1 = jr.ReadSha1() ?? default;
                        return true;
                }

                return false;
            },

            // Factory
            () => sha1 == default ? default : new CommitId(sha1));
        }

        public static CommitId ReadCommitId(this string json)
        {
            if (json == null || json == JsonConstants.JsonNull) return default;

            using (var tr = new StringReader(json))
            using (var jr = new JsonTextReader(tr))
            {
                jr.DateParseHandling = DateParseHandling.None;

                CommitId model = ReadCommitId(jr);
                return model;
            }
        }

        public static string Write(this CommitId model)
        {
            // Perf: No need to use JsonWriter for a simple scalar
            string json = "{ \"" + _id + "\": \"" + model.Sha1.ToString("n") + "\" }";
            return json;
        }
    }
}
