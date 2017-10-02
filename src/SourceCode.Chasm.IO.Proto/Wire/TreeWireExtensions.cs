using System.Runtime.CompilerServices;

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class TreeWireExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeKind Convert(this NodeKindWire wire)
           => (NodeKind)(int)wire;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeKindWire Convert(this NodeKind model)
            => (NodeKindWire)(int)model;

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

            var nodes = new TreeNode[wire.Nodes?.Count ?? 0];
            for (var i = 0; i < nodes.Length; i++)
                nodes[i] = wire.Nodes[i].Convert();

            var model = new TreeNodeList(nodes);
            return model;
        }
    }
}
