using SourceCode.Clay.Json;
using System;
using System.Json;
using System.Linq;
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
            JsonArray parents = null;
            if (model.Parents != null)
            {
                switch (model.Parents.Count)
                {
                    case 0:
                        {
                            parents = new JsonArray(Array.Empty<JsonValue>());
                        }
                        break;

                    case 1:
                        {
                            var sha1 = model.Parents[0].Sha1;
                            parents = new JsonArray(new[] { new JsonPrimitive(sha1.ToString("N")) });
                        }
                        break;

                    case 2:
                        {
                            var sha0 = model.Parents[0].Sha1;
                            var sha1 = model.Parents[1].Sha1;
                            var cmp = sha0.CompareTo(sha1);

                            if (cmp == 0)
                                parents = new JsonArray(new JsonPrimitive(sha0.ToString("N")));
                            else
                                parents = new JsonArray(new JsonPrimitive(sha0.ToString("N")), new JsonPrimitive(sha1.ToString("N")));
                        }
                        break;

                    default:
                        {
                            var sorted = new Sha1[model.Parents.Count];
                            for (var i = 0; i < sorted.Length; i++)
                                sorted[i] = model.Parents[i].Sha1;
                            Array.Sort(sorted, Sha1.Comparison);

                            var distinct = sorted.Distinct().Select(n => new JsonPrimitive(n.ToString("N")));
                            parents = new JsonArray(distinct);
                        }
                        break;
                }
            }

            // CommitUtc
            var utc = XmlConvert.ToString(model.CommitUtc, XmlDateTimeSerializationMode.Utc);

            // Message
            var msg = model.CommitMessage == null ? null : new JsonPrimitive(model.CommitMessage);

            var wire = new JsonObject
            {
                [_parents] = parents,
                [_treeId] = model.TreeId.Sha1.ToString("N"),
                [_utc] = utc,
                [_message] = msg
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
            CommitId[] parents = null;
            var ja = wire.GetArray(_parents);
            if (ja != null)
            {
                if (ja.Count == 0)
                {
                    parents = Array.Empty<CommitId>();
                }
                else
                {
                    parents = new CommitId[ja.Count];
                    for (var i = 0; i < parents.Length; i++)
                    {
                        sha1 = Sha1.Parse(ja[i]);
                        parents[i] = new CommitId(sha1);
                    }
                }
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
