#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Json;
using System;
using System.Json;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class TreeWireNodeExtensions
    {
        #region Constants

        // Naming follows convention in ProtoSerializer

        private const string _name = "name";
        private const string _kind = "kind";
        private const string _nodeId = "nodeId";

        #endregion

        #region Methods

        public static JsonObject Convert(this TreeNode model)
        {
            if (model == TreeNode.Empty) return default;

            var wire = new JsonObject
            {
                [_name] = model.Name,
                [_kind] = model.Kind.ToString(),
                [_nodeId] = model.Sha1.ToString("N")
            };

            return wire;
        }

        public static TreeNode ConvertTreeNode(this JsonObject wire)
        {
            if (wire == null) return default;

            var name = (string)wire.GetValue(_name, JsonType.String, false);

            var jv = wire.GetValue(_kind, JsonType.String, false);
            var kind = (NodeKind)Enum.Parse(typeof(NodeKind), jv, true);

            jv = wire.GetValue(_nodeId, JsonType.String, false);
            var sha1 = Sha1.Parse(jv);

            var model = new TreeNode(name, kind, sha1);
            return model;
        }

        #endregion
    }
}
