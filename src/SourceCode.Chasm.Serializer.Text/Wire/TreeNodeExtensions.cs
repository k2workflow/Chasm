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

        private static readonly char[] s_split = new char[1] { ' ' };

        public static TreeNode ConvertTreeNode(this string wire)
        {
            if (string.IsNullOrWhiteSpace(wire)) return default;

            string[] tokens = wire.Split(s_split, 4, StringSplitOptions.None);
            if (tokens.Length != 4)
                throw new SerializationException();

            var kind = (NodeKind)Enum.Parse(typeof(NodeKind), tokens[1], true);
            var sha1 = Sha1.Parse(tokens[2]);
            string name = tokens[3];

            var model = new TreeNode(name, kind, sha1);
            return model;
        }
    }
}
