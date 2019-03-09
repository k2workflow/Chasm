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

            return new TreeNodeMap(nodes);
        }

        public static NodeKind Convert(this NodeKindWire wire) => wire switch
        {
            // Do not use direct integer conversion - it may fail silently
            NodeKindWire.Blob => NodeKind.Blob,
            NodeKindWire.Tree => NodeKind.Tree,
            _ => throw new ArgumentOutOfRangeException(nameof(wire))
        };

        public static NodeKindWire Convert(this NodeKind model) => model switch
        {
            // Do not use direct integer conversion - it may fail silently
            NodeKind.Blob => NodeKindWire.Blob,
            NodeKind.Tree => NodeKindWire.Tree,
            _ => throw new ArgumentOutOfRangeException(nameof(model))
        };
    }
}
