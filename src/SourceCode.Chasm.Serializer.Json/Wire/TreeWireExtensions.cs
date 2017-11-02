#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Json;
using System.Json;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class TreeWireExtensions
    {
        #region Methods

        public static JsonValue Convert(this TreeNodeMap model)
        {
            if (model.Count == 0) return new JsonArray();

            var items = new JsonValue[model.Count];
            for (var i = 0; i < items.Length; i++)
                items[i] = model[i].Convert();

            var wire = new JsonArray(items);
            return wire;
        }

        public static TreeNodeMap ConvertTree(this JsonArray wire)
        {
            if (wire == null) return default;
            if (wire.Count == 0) return default;

            var nodes = new TreeNode[wire.Count];
            for (var i = 0; i < nodes.Length; i++)
                nodes[i] = ((JsonObject)wire[i]).ConvertTreeNode();

            var model = new TreeNodeMap(nodes);
            return model;
        }

        public static TreeNodeMap ParseTree(this string json)
        {
            var wire = json.ParseJsonArray();

            var model = wire.ConvertTree();
            return model;
        }

        #endregion
    }
}
