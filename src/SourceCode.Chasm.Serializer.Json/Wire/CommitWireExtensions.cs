#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Newtonsoft.Json.Linq;
using System;

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

        public static JObject Convert(this Commit model)
        {
            // Parents
            JArray parents = null;
            if (model.Parents != null)
            {
                switch (model.Parents.Count)
                {
                    case 0:
                        parents = new JArray();
                        break;

                    case 1:
                        var sha1 = model.Parents[0].Sha1;
                        parents = new JArray(new[] { new JValue(sha1.ToString("N")) });
                        break;

                    default:
                        {
                            var array = new JValue[model.Parents.Count];
                            for (var i = 0; i < array.Length; i++)
                                array[i] = new JValue(model.Parents[i].Sha1.ToString("N"));

                            parents = new JArray(array);
                        }
                        break;
                }
            }

            // Author
            var author = model.Author == default ? null : model.Author.Convert();

            // Committer
            var committer = model.Committer == default ? null : model.Committer.Convert();

            // Message
            var msg = model.Message == null ? null : new JValue(model.Message);

            var wire = new JObject
            {
                [_parents] = parents,
                [_author] = author,
                [_committer] = committer,
                [_message] = msg
            };

            // TreeId
            if (model.TreeId != null)
                wire[_treeId] = model.TreeId.Value.ToString();

            return wire;
        }

        public static Commit ConvertCommit(this JObject wire)
        {
            if (wire == null) return default;

            // TreeId
            TreeId? treeId = default;
            if (wire.TryGetValue(_treeId, out var jv) && jv != null)
            {
                var str = (string)jv;
                treeId = TreeId.Parse(str);
            }

            // Parents
            var parents = Array.Empty<CommitId>();
            if (wire.TryGetValue(_parents, out jv)
                && jv != null
                && jv is JArray ja
                && ja.Count > 0)
            {
                parents = new CommitId[ja.Count];
                for (var i = 0; i < parents.Length; i++)
                {
                    var str = (string)ja[i];
                    parents[i] = CommitId.Parse(str);
                }
            }

            // Author
            Audit author = default;
            if (wire.TryGetValue(_author, out var jo)
                && jo != null
                && jo is JObject jobj1)
            {
                author = jobj1.ConvertAudit();
            }

            // Committer
            Audit committer = default;
            if (wire.TryGetValue(_committer, out jo)
                && jo != null
                && jo is JObject jobj2)
            {
                committer = jobj2.ConvertAudit();
            }

            // Message
            string message = null;
            if (wire.TryGetValue(_message, out jv)
                && jv != null)
            {
                message = (string)jv;
            }

            var model = new Commit(parents, treeId, author, committer, message);
            return model;
        }

        public static Commit ParseCommit(this string json)
        {
            var wire = JToken.Parse(json);

            var model = ((JObject)wire).ConvertCommit();
            return model;
        }

        #endregion
    }
}
