#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Json;
using System;
using System.Json;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class CommitWireExtensions
    {
        #region Constants

        // Naming follows convention in ProtoSerializer

        private const string _parents = "parents";
        private const string _treeId = "treeId";
        private const string _author = "author";
        private const string _committer = "committer";
        private const string _message = "message";

        #endregion

        #region Methods

        public static JsonObject Convert(this Commit model)
        {
            if (model == Commit.Empty) return default; // null

            // TreeId
            var treeId = model.TreeId.Sha1.ToString("N");

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

            // Author
            var author = model.Author.Convert();

            // Committer
            var committer = model.Committer.Convert();

            // Message
            var msg = model.Message == null ? null : new JsonPrimitive(model.Message);

            var wire = new JsonObject
            {
                [_parents] = parents,
                [_treeId] = treeId,
                [_author] = author,
                [_committer] = committer,
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

            // Author
            Audit author = default;
            if (wire.TryGetObject(_author, out var jo))
            {
                author = jo.ConvertAudit();
            }

            // Committer
            Audit committer = default;
            if (wire.TryGetObject(_committer, out jo))
            {
                committer = jo.ConvertAudit();
            }

            // Message
            string message = null;
            if (wire.TryGetValue(_message, JsonType.String, true, out jv))
                message = jv;

            var model = new Commit(parents, treeId, author, committer, message);
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
