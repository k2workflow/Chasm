using System.Runtime.CompilerServices;

namespace SourceCode.Chasm.IO.Bond.Wire
{
    internal static class TreeWireExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeKind Convert(this NodeKindWire wire)
            => (NodeKind)(int)wire;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeKindWire Convert(this NodeKind model)
            => (NodeKindWire)(int)model;

        public static int EstimateBytes(this TreeWire wire)
        {
            if (wire == null || wire.Nodes == null || wire.Nodes.Count == 0)
                return 0;

            // Value
            var len = wire.Nodes.Count * TreeWireNode.EstimateBytes;

            // Key
            foreach (var node in wire.Nodes)
                len += node.Name.Length * 2; // utf8

            return len;
        }

        public static TreeWire Convert(this TreeNodeList model)
        {
            if (model == TreeNodeList.Empty) return new TreeWire();

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
            if (wire == null) return new TreeNodeList();

            var nodes = new TreeNode[wire.Nodes?.Count ?? 0];
            for (var i = 0; i < nodes.Length; i++)
                nodes[i] = wire.Nodes[i].Convert();

            var model = new TreeNodeList(nodes);
            return model;
        }
    }
}
