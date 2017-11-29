#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Newtonsoft.Json;
using SourceCode.Clay;
using SourceCode.Clay.Json;
using System;
using System.IO;
using System.Text;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class TreeNodeExtensions
    {
        #region Constants

        // Naming follows convention in ProtoSerializer

        private const string _name = "name";
        private const string _kind = "kind";
        private const string _nodeId = "nodeId";

        #endregion

        #region Read

        public static TreeNode ReadTreeNode(this JsonReader jr)
        {
            if (jr == null) throw new ArgumentNullException(nameof(jr));

            string name = default;
            NodeKind kind = default;
            Sha1 sha1 = default;

            // Switch
            return jr.ReadObject(n =>
            {
                switch (n)
                {
                    case _name:
                        name = (string)jr.Value;
                        return true;

                    case _kind:
                        kind = jr.ReadEnum<NodeKind>(true) ?? default;
                        return true;

                    case _nodeId:
                        sha1 = jr.ReadSha1() ?? default;
                        return true;
                }

                return false;
            },

            // Factory
            () => name == null && kind == default && sha1 == default ? default : new TreeNode(name, kind, sha1));
        }

        public static TreeNode ReadTreeNode(this string json)
        {
            if (json == null || json == JsonConstants.Null) return default;

            using (var tr = new StringReader(json))
            using (var jr = new JsonTextReader(tr))
            {
                jr.DateParseHandling = DateParseHandling.None;

                var model = ReadTreeNode(jr);
                return model;
            }
        }

        #endregion

        #region Write

        public static void Write(this JsonWriter jw, TreeNode model)
        {
            if (jw == null) throw new ArgumentNullException(nameof(jw));

            if (model == default)
            {
                jw.WriteNull();
                return;
            }

            jw.WriteStartObject();
            {
                // Name
                jw.WritePropertyName(_name);
                jw.WriteValue(model.Name);

                // Kind
                jw.WritePropertyName(_kind);
                jw.WriteValue(model.Kind.ToString());

                // NodeId
                jw.WritePropertyName(_nodeId);
                jw.WriteValue(model.Sha1.ToString("N"));
            }
            jw.WriteEndObject();
        }

        public static string Write(this TreeNode model)
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
