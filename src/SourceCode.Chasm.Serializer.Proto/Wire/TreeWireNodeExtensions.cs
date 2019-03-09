namespace SourceCode.Chasm.Serializer.Proto.Wire
{
    internal static class TreeWireNodeExtensions
    {
        public static TreeNode Convert(this TreeWireNode wire)
        {
            if (wire == null) return default;

            Clay.Sha1? sha1 = wire.NodeId.Convert();

            return new TreeNode(wire.Name, wire.Kind.Convert(), sha1.Value);
        }

        public static TreeWireNode Convert(this TreeNode model)
        {
            if (model == TreeNode.Empty) return default;

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
