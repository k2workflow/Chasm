using System;
using System.Collections.Generic;
using System.Text;

namespace SourceCode.Chasm.Serializer.Text.Wire
{
    internal static class TreeNodeMapExtensions
    {
        public static string Convert(this TreeNodeMap model)
        {
            switch (model.Count)
            {
                case 0:
                    return string.Empty;

                case 1:
                    return model[0].Convert();

                default:
                    {
                        var sb = new StringBuilder();
                        for (int i = 0; i < model.Count; i++)
                        {
                            string text = model[i].Convert();
                            sb.AppendLine(text);
                        }

                        string wire = sb.ToString();
                        return wire;
                    }
            }
        }

        private static readonly char[] s_split = new char[1] { '\n' };

        public static TreeNodeMap ConvertTree(this string wire)
        {
            if (string.IsNullOrWhiteSpace(wire)) return default;

            string[] tokens = wire.Split(s_split, StringSplitOptions.None);
            if (tokens.Length == 0)
                return TreeNodeMap.Empty;

            var nodes = new List<TreeNode>(tokens.Length);
            for (int i = 0; i < tokens.Length; i++)
            {
                string text = tokens[i].Trim();
                if (string.IsNullOrWhiteSpace(text)) break;

                TreeNode node = text.ConvertTreeNode();
                nodes.Add(node);
            }

            var model = new TreeNodeMap(nodes);
            return model;
        }
    }
}
