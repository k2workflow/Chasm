#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using TreePair = System.Collections.Generic.KeyValuePair<string, SourceCode.Chasm.TreeNode>;

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class TreeWireNodeExtensions
    {
        #region Methods

        public static TreePair Convert(this TreeWireNode wire)
        {
            if (wire == null) return default;

            var sha1 = wire.NodeId.Convert();

            var model = new TreeNode(wire.Kind.Convert(), sha1.Value).CreateMap(wire.Name);

            return model;
        }

        public static TreeWireNode Convert(this TreePair model)
        {
            if (model.Key == null) return default;

            var wire = new TreeWireNode
            {
                Name = model.Key,
                Kind = model.Value.Kind.Convert(),
                NodeId = model.Value.Sha1.Convert()
            };

            return wire;
        }

        #endregion
    }
}
