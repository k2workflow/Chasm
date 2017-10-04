using SourceCode.Clay.Json;
using System.Json;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class CommitRefWireExtensions
    {
        private const string _sha1 = "sha1";

        public static JsonObject Convert(this CommitRef model)
        {
            if (model == CommitRef.Empty) return default;

            var wire = new JsonObject
            {
                [_sha1] = model.CommitId.Sha1.ToString("N")
            };

            return wire;
        }

        public static CommitRef ConvertCommitRef(this JsonObject wire)
        {
            if (wire == null) return default;

            // Sha1
            var jv = wire.GetValue(_sha1, JsonType.String, false);
            var sha1 = Sha1.Parse(jv);

            var commitId = new CommitId(sha1);

            var model = new CommitRef(commitId);
            return model;
        }

        public static CommitRef ParseCommitRef(this string json)
        {
            var wire = json.ParseJsonObject();

            var model = ConvertCommitRef(wire);
            return model;
        }
    }
}
