using System;
using Google.Protobuf;

namespace SourceCode.Chasm.Serializer.Proto.Wire
{
    internal static class TreeWireNodeExtensions
    {
        public static TreeNode Convert(this TreeWireNode wire)
        {
            if (wire == null) return default;

            Clay.Sha1? sha1 = wire.NodeId.Convert();

            ReadOnlyMemory<byte>? data = null;
            if (wire.HasData)
                data = wire.Data.ToByteArray();

            return new TreeNode(wire.Name, wire.Kind.Convert(), sha1.Value, data);
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

            if (model.Data != null)
            {
                byte[] bytes = model.Data.Value.ToArray(); // TODO: Perf
                wire.Data = ByteString.CopyFrom(bytes, 0, bytes.Length);
                wire.HasData = true;
            }

            return wire;
        }
    }
}
