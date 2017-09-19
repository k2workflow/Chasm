using SourceCode.Clay;
using SourceCode.Clay.Json;
using System;
using System.Json;
using System.Xml;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class CommitWireExtensions
    {
        private const string _parents = "parents";
        private const string _treeId = "treeId";
        private const string _utc = "utc";
        private const string _message = "message";

        public static JsonObject Convert(this Commit model)
        {
            if (model == Commit.Empty) return default;

            // Parents
            var parents = Array.Empty<JsonValue>();
            if (model.Parents != null && model.Parents.Count > 0)
            {
                parents = new JsonValue[model.Parents.Count];

                for (var i = 0; i < parents.Length; i++)
                    parents[i] = model.Parents[i].Sha1.ToString("N");
            }

            // CommitUtc
            var utc = XmlConvert.ToString(model.CommitUtc, XmlDateTimeSerializationMode.Utc);

            var wire = new JsonObject
            {
                [_parents] = new JsonArray(parents),
                [_treeId] = model.TreeId.Sha1.ToString("N"),
                [_utc] = utc,
                [_message] = model.CommitMessage
            };

            return wire;
        }

        public static Commit ConvertCommit(this JsonObject wire)
        {
            if (wire == null) return default;

            // TreeId
            var jv = wire.GetValue(_treeId, JsonType.String, false);
            var sha1 = Sha1.Parse(jv);
            var treeId = new TreeId(sha1);

            // Parents
            var ja = wire.GetArray(_parents);
            var parents = new CommitId[ja.Count];
            for (var i = 0; i < parents.Length; i++)
            {
                sha1 = Sha1.Parse(ja[i]);
                parents[i] = new CommitId(sha1);
            }

            // Utc
            jv = wire.GetValue(_utc, JsonType.String, false);
            var utc = XmlConvert.ToDateTime(jv, XmlDateTimeSerializationMode.Utc);

            // Message
            string message = null;
            if (wire.TryGetValue(_message, JsonType.String, true, out jv))
                message = jv;

            var model = new Commit(parents, treeId, utc, message);
            return model;
        }

        public static Commit ParseCommit(this string json)
        {
            var wire = json.ParseJsonObject();

            var model = wire.ConvertCommit();
            return model;
        }
    }
}
