#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Newtonsoft.Json;
using SourceCode.Clay.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class CommitExtensions
    {
        #region Constants

        // Naming follows convention in ProtoSerializer

        private const string _parents = "parents";
        private const string _treeMapId = "treeMapId";
        private const string _author = "author";
        private const string _committer = "committer";
        private const string _message = "message";

        #endregion

        #region Read

        public static Commit ReadCommit(this JsonReader jr)
        {
            if (jr == null) throw new ArgumentNullException(nameof(jr));

            Audit author = default;
            Audit committer = default;
            TreeMapId? treeMapId = default;
            IReadOnlyList<CommitId> parents = null;
            string message = null;

            // Switch
            return jr.ReadObject(n =>
            {
                switch (n)
                {
                    case _parents:
                        parents = ReadParents();
                        return true;

                    case _author:
                        author = jr.ReadAudit();
                        return true;

                    case _committer:
                        committer = jr.ReadAudit();
                        return true;

                    case _message:
                        message = (string)jr.Value;
                        return true;

                    case _treeMapId:
                        treeMapId = ReadTreeMapId();
                        return true;
                }

                return false;
            },

            // Factory
            () => new Commit(parents, treeMapId, author, committer, message));

            // Property

            IReadOnlyList<CommitId> ReadParents() => jr.ReadArray(() =>
            {
                var sha1 = jr.ReadSha1();
                return sha1 == null ? default : new CommitId(sha1.Value);
            });

            TreeMapId? ReadTreeMapId()
            {
                var sha1 = jr.ReadSha1();
                return sha1 == null ? null : (TreeMapId?)(new TreeMapId(sha1.Value));
            }
        }

        public static Commit ReadCommit(this string json)
        {
            if (json == null || json == JsonConstants.Null) return default;

            using (var tr = new StringReader(json))
            using (var jr = new JsonTextReader(tr))
            {
                jr.DateParseHandling = DateParseHandling.None;

                var model = ReadCommit(jr);
                return model;
            }
        }

        #endregion

        #region Write

        public static void Write(this JsonTextWriter jw, Commit model)
        {
            if (jw == null) throw new ArgumentNullException(nameof(jw));

            if (model == default)
            {
                jw.WriteNull();
                return;
            }

            jw.WriteStartObject();
            {
                // Parents
                if (model.Parents != null && model.Parents.Count > 0)
                {
                    jw.WritePropertyName(_parents);
                    jw.WriteStartArray();

                    for (var i = 0; i < model.Parents.Count; i++)
                        jw.WriteValue(model.Parents[i].Sha1.ToString("N"));

                    jw.WriteEndArray();
                }

                // Author
                if (model.Author != default)
                {
                    jw.WritePropertyName(_author);
                    jw.Write(model.Author);
                }

                // Committer
                if (model.Committer != default)
                {
                    jw.WritePropertyName(_committer);
                    jw.Write(model.Committer);
                }

                // Message
                if (model.Message != null)
                {
                    jw.WritePropertyName(_message);
                    jw.WriteValue(model.Message);
                }

                // TreeMapId
                if (model.TreeMapId != null)
                {
                    jw.WritePropertyName(_treeMapId);
                    jw.WriteValue(model.TreeMapId.Value.ToString());
                }
            }
            jw.WriteEndObject();
        }

        public static string Write(this Commit model)
        {
            if (model == default) return JsonConstants.Null;

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var jw = new JsonTextWriter(sw))
            {
                Write(jw, model);
            }

            return sb.ToString();
        }

        #endregion
    }
}
