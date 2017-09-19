namespace SourceCode.Chasm.IO.Bond.Wire
{
    internal static class TreeWireNodeExtensions
    {
        public static int EstimateBytes(this TreeWireNode wire)
        {
            if (wire == null) return 0;

            // Name
            var len = 2 * (wire.Name?.Length ?? 0); // utf8

            // Kind
            len += 4;

            // Sha1
            len += wire.NodeId.EstimateBytes();

            return len;
        }

        public static TreeNode Convert(this TreeWireNode wire)
        {
            if (wire == null) return TreeNode.Empty;

            var sha1 = wire.NodeId.Convert();

            var model = new TreeNode(wire.Name, wire.Kind.Convert(), sha1);
            return model;
        }

        public static TreeWireNode Convert(this TreeNode model)
        {
            if (model == TreeNode.Empty) return new TreeWireNode();

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
