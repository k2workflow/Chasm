#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Newtonsoft.Json;
using SourceCode.Clay.Json;
using System;
using System.IO;
using System.Text;
using TreePair = System.Collections.Generic.KeyValuePair<string, SourceCode.Chasm.TreeNode>;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class TreePairExtensions
    {
        #region Constants

        // Naming follows convention in ProtoSerializer

        private const string _name = "name";
        private const string _kind = "kind";
        private const string _nodeId = "nodeId";

        #endregion

        #region Read

        public static TreePair ReadTreeNode(this JsonReader jr)
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
            () => name == null ? default : new TreeNode(kind, sha1).CreateMap(name));
        }

        public static TreePair ReadTreeNode(this string json)
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

        public static void Write(this JsonWriter jw, TreePair model)
        {
            if (jw == null) throw new ArgumentNullException(nameof(jw));

            if (model.Key == null)
            {
                jw.WriteNull();
                return;
            }

            jw.WriteStartObject();
            {
                // Name
                jw.WritePropertyName(_name);
                jw.WriteValue(model.Key);

                // Kind
                jw.WritePropertyName(_kind);
                jw.WriteValue(model.Value.Kind.ToString());

                // NodeId
                jw.WritePropertyName(_nodeId);
                jw.WriteValue(model.Value.Sha1.ToString("N"));
            }
            jw.WriteEndObject();
        }

        public static string Write(this TreePair model)
        {
            if (model.Key == null) return JsonConstants.Null;

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
