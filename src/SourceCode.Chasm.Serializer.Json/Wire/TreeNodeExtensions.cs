#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Newtonsoft.Json;
using SourceCode.Clay.Json;

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

        public static TreeNode ReadNode(this JsonReader jr)
        {
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
                        break;

                    case _kind:
                        kind = jr.ReadEnum<NodeKind>(true) ?? default;
                        break;

                    case _nodeId:
                        sha1 = jr.ReadSha1() ?? default;
                        break;
                }
            },

            // Factory
            () => name == null && kind == default && sha1 == default ? default : new TreeNode(name, kind, sha1));
        }

        #endregion

        #region Write

        public static void Write(this JsonWriter jw, TreeNode model)
        {
            if (model == TreeNode.Empty) return; // null

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

        #endregion
    }
}
