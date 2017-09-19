﻿namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class TreeWireNodeExtensions
    {
        public static TreeNode Convert(this TreeWireNode wire)
        {
            if (wire == null) return TreeNode.Empty;

            var sha1 = wire.NodeId.Convert();

            var model = new TreeNode(wire.Name, wire.Kind.Convert(), sha1);

            return model;
        }

        public static TreeWireNode Convert(this TreeNode model)
        {
            if (model == TreeNode.Empty) return null;

            var wire = new TreeWireNode
            {
                Name = model.Name,
                Kind = model.Kind.Convert(),
                NodeId = model.Sha1.Convert()
            };

            return wire;
        }
    }
}
