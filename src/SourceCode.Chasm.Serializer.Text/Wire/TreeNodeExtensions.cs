using System;
using System.Runtime.Serialization;
using SourceCode.Clay;

namespace SourceCode.Chasm.Serializer.Text.Wire
{
    internal static class TreeNodeExtensions
    {
        private const string TreePerm = "040000";
        private const string BlobPerm = "100664";

        public static string Convert(this TreeNode model)
        {
            if (model == TreeNode.Empty) return default;

            string kind, perm;
            switch (model.Kind)
            {
                case NodeKind.Tree:
                    kind = "tree";
                    perm = TreePerm;
                    break;

                case NodeKind.Blob:
                    kind = "blob";
                    perm = BlobPerm;
                    break;

                default: throw new SerializationException();
            }

            string wire = $"{perm} {kind} {model.Sha1:n} {model.Name}";
            return wire;
        }

#if NETSTANDARD2_0
        private static readonly char[] s_split = new char[1] { ' ' };
#endif
        public static TreeNode ConvertTreeNode(this string wire)
        {
            if (string.IsNullOrWhiteSpace(wire)) return default;

#if !NETSTANDARD2_0
            string[] tokens = wire.Split(' ', 4, StringSplitOptions.None);
#else
            string[] tokens = wire.Split(s_split, 4, StringSplitOptions.None);
#endif
            if (tokens.Length != 4) throw new SerializationException();

#if !NETSTANDARD2_0
            NodeKind kind = Enum.Parse<NodeKind>(tokens[1], true);
#else
            var kind = (NodeKind)Enum.Parse(typeof(NodeKind), tokens[1], true);
#endif
            var sha1 = Sha1.Parse(tokens[2]);
            string name = tokens[3];

            var model = new TreeNode(name, kind, sha1);
            return model;
        }
    }
}
