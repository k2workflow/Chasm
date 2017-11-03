#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Newtonsoft.Json.Linq;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class TreeWireExtensions
    {
        #region Methods

        public static JArray Convert(this TreeNodeMap model)
        {
            if (model.Count == 0) return new JArray();

            var items = new JObject[model.Count];
            for (var i = 0; i < items.Length; i++)
                items[i] = model[i].Convert();

            var wire = new JArray(items);
            return wire;
        }

        public static TreeNodeMap ConvertTree(this JArray wire)
        {
            if (wire == null) return default;
            if (wire.Count == 0) return default;

            var nodes = new TreeNode[wire.Count];
            for (var i = 0; i < nodes.Length; i++)
                nodes[i] = ((JObject)wire[i]).ConvertTreeNode();

            var model = new TreeNodeMap(nodes);
            return model;
        }

        public static TreeNodeMap ParseTree(this string json)
        {
            var wire = JToken.Parse(json);

            var model = ((JArray)wire).ConvertTree();
            return model;
        }

        #endregion
    }
}
