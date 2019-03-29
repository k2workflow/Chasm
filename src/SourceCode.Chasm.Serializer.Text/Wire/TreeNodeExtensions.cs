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

            string kind;
            switch (model.Kind)
            {
                case NodeKind.Tree:
                    kind = "tree";
                    break;

                case NodeKind.Blob:
                    kind = "blob";
                    break;

                default: throw new SerializationException();
            }

            var str = $"{kind} {model.Sha1:n} {model.Name}";

            if (model.Data != null)
            {
                string base64 = System.Convert.ToBase64String(model.Data.Value.Span, Base64FormattingOptions.None);
                str += " " + base64;
            }

            return str;
        }

        private static readonly char[] s_split = new char[1] { ' ' };

        public static TreeNode ConvertTreeNode(this string wire)
        {
            if (string.IsNullOrWhiteSpace(wire)) return default;

            string[] tokens = wire.Split(s_split, 4, StringSplitOptions.None);
            if (tokens.Length < 3)
                throw new SerializationException();

            var kind = (NodeKind)Enum.Parse(typeof(NodeKind), tokens[0], true);
            var sha1 = Sha1.Parse(tokens[1]);
            string name = tokens[2];

            ReadOnlyMemory<byte>? data = null;
            if (tokens.Length == 4)
            {
                string base64 = tokens[3];
                data = System.Convert.FromBase64String(base64);
            }

            return new TreeNode(name, kind, sha1, data);
        }
    }
}
