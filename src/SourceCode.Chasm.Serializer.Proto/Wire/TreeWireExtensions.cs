using System;

namespace SourceCode.Chasm.Serializer.Proto.Wire
{
    internal static class TreeWireExtensions
    {
        public static TreeWire Convert(this TreeNodeMap model)
        {
            if (model.Count == 0) return new TreeWire();

            var wire = new TreeWire();
            for (int i = 0; i < model.Count; i++)
            {
                TreeWireNode node = model[i].Convert();
                wire.Nodes.Add(node);
            }

            return wire;
        }

        public static TreeNodeMap Convert(this TreeWire wire)
        {
            if (wire == null) return default;
            if (wire.Nodes.Count == 0) return default;

            var nodes = new TreeNode[wire.Nodes?.Count ?? 0];
            for (int i = 0; i < nodes.Length; i++)
                nodes[i] = wire.Nodes[i].Convert();

            var model = new TreeNodeMap(nodes);
            return model;
        }

        public static NodeKind Convert(this NodeKindWire wire)
        {
            // Do not use direct integer conversion - it may fail silently
            switch (wire)
            {
                case NodeKindWire.Blob: return NodeKind.Blob;
                case NodeKindWire.Tree: return NodeKind.Tree;
                default: throw new ArgumentOutOfRangeException(nameof(wire));
            }
        }

        public static NodeKindWire Convert(this NodeKind model)
        {
            // Do not use direct integer conversion - it may fail silently
            switch (model)
            {
                case NodeKind.Blob: return NodeKindWire.Blob;
                case NodeKind.Tree: return NodeKindWire.Tree;
                default: throw new ArgumentOutOfRangeException(nameof(model));
            }
        }
    }
}
