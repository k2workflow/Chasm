using System;
using System.Runtime.CompilerServices;

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class TreeWireExtensions
    {
        public static TreeWire Convert(this TreeNodeList model)
        {
            if (model == null) return null;
            if (model.Count == 0) return new TreeWire();

            var wire = new TreeWire();
            for (var i = 0; i < model.Count; i++)
            {
                var node = model[i].Convert();
                wire.Nodes.Add(node);
            }

            return wire;
        }

        public static TreeNodeList Convert(this TreeWire wire)
        {
            if (wire == null) return default;
            if (wire.Nodes.Count == 0) return new TreeNodeList();

            var nodes = new TreeNode[wire.Nodes?.Count ?? 0];
            for (var i = 0; i < nodes.Length; i++)
                nodes[i] = wire.Nodes[i].Convert();

            var model = new TreeNodeList(nodes);
            return model;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeKind Convert(this NodeKindWire wire)
        {
            // Do not use direct integer conversion - it may fail silently
            switch (wire)
            {
                case NodeKindWire.None: return NodeKind.None;
                case NodeKindWire.Blob: return NodeKind.Blob;
                case NodeKindWire.Tree: return NodeKind.Tree;
                default: throw new ArgumentOutOfRangeException(nameof(wire));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeKindWire Convert(this NodeKind model)
        {
            // Do not use direct integer conversion - it may fail silently
            switch (model)
            {
                case NodeKind.None: return NodeKindWire.None;
                case NodeKind.Blob: return NodeKindWire.Blob;
                case NodeKind.Tree: return NodeKindWire.Tree;
                default: throw new ArgumentOutOfRangeException(nameof(model));
            }
        }
    }
}
