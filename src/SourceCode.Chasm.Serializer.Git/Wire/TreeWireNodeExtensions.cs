#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Runtime.Serialization;
using System.Text;

namespace SourceCode.Chasm.IO.Text.Wire
{
    internal static class TreeWireNodeExtensions
    {
        #region Methods

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

            var wire = $"{perm} {kind} {model.Sha1:N} {model.Name}";
            return wire;
        }

        public static TreeNode ConvertTreeNode(this string wire)
        {
            if (string.IsNullOrWhiteSpace(wire)) return default;

            var tokens = wire.Split(' ', 4, StringSplitOptions.None);
            if (tokens.Length != 4) throw new SerializationException();

            var kind = Enum.Parse<NodeKind>(tokens[1], true);
            var sha1 = Sha1.Parse(tokens[2]);
            var name = tokens[3];

            var model = new TreeNode(name, kind, sha1);
            return model;
        }

        #endregion
    }
}
