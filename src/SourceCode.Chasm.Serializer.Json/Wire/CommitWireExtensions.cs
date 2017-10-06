#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Json;
using System;
using System.Json;
using System.Xml;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class CommitWireExtensions
    {
        #region Fields

        private const string _parents = "parents";
        private const string _treeId = "treeId";
        private const string _utc = "utc";
        private const string _message = "message";

        #endregion

        #region Methods

        public static JsonObject Convert(this Commit model)
        {
            if (model == Commit.Empty) return default; // null

            // Parents
            JsonArray parents = null;
            if (model.Parents != null)
            {
                switch (model.Parents.Count)
                {
                    case 0:
                        parents = new JsonArray(Array.Empty<JsonValue>());
                        break;

                    case 1:
                        var sha1 = model.Parents[0].Sha1;
                        parents = new JsonArray(new[] { new JsonPrimitive(sha1.ToString("N")) });
                        break;

                    default:
                        {
                            var array = new JsonPrimitive[model.Parents.Count];
                            for (var i = 0; i < array.Length; i++)
                                array[i] = new JsonPrimitive(model.Parents[i].Sha1.ToString("N"));

                            parents = new JsonArray(array);
                        }
                        break;
                }
            }

            // CommitUtc
            var utc = XmlConvert.ToString(model.CommitUtc, XmlDateTimeSerializationMode.Utc);

            // Message
            var msg = model.CommitMessage == null ? null : new JsonPrimitive(model.CommitMessage);

            // TreeId
            var treeId = model.TreeId.Sha1.ToString("N");

            var wire = new JsonObject
            {
                [_parents] = parents,
                [_treeId] = treeId,
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
            var parents = Array.Empty<CommitId>();
            var ja = wire.GetArray(_parents);
            if (ja != null && ja.Count > 0)
            {
                parents = new CommitId[ja.Count];
                for (var i = 0; i < parents.Length; i++)
                {
                    sha1 = Sha1.Parse(ja[i]);
                    parents[i] = new CommitId(sha1);
                }
            }

            // CommitUtc
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

        #endregion
    }
}
