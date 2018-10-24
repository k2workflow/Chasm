using System;
using System.Runtime.Serialization;
using SourceCode.Clay;

namespace SourceCode.Chasm.Serializer.Text.Wire
{
    internal static class TreeNodeExtensions
    {
        public static string Convert(this TreeNode model)
        {
            if (model == TreeNode.Empty) return default;

            string kind, perm;
            switch (model.Kind)
            {
                case NodeKind.Tree: kind = "tree"; perm = "040000"; break;
                case NodeKind.Blob: kind = "blob"; perm = "100664"; break;
                default: throw new SerializationException();
            }

            string wire = $"{perm} {kind} {model.Sha1:N} {model.Name}";
            return wire;
        }

        public static TreeNode ConvertTreeNode(this string wire)
        {
            if (string.IsNullOrWhiteSpace(wire)) return default;

            string[] tokens = wire.Split(' ', 4, StringSplitOptions.None);
            if (tokens.Length != 4) throw new SerializationException();

            NodeKind kind = Enum.Parse<NodeKind>(tokens[1], true);
            var sha1 = Sha1.Parse(tokens[2]);
            string name = tokens[3];

            var model = new TreeNode(name, kind, sha1);
            return model;
        }
    }
}
