#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using TreePair = System.Collections.Generic.KeyValuePair<string, SourceCode.Chasm.TreeNode>;

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class TreeWireExtensions
    {
        #region Methods

        public static TreeWire Convert(this TreeMap model)
        {
            if (model.Count == 0) return new TreeWire();

            var wire = new TreeWire();
            for (var i = 0; i < model.Count; i++)
            {
                var node = model[i].Convert();
                wire.Nodes.Add(node);
            }

            return wire;
        }

        public static TreeMap Convert(this TreeWire wire)
        {
            if (wire == null) return default;
            if (wire.Nodes.Count == 0) return default;

            var nodes = new TreePair[wire.Nodes?.Count ?? 0];
            for (var i = 0; i < nodes.Length; i++)
                nodes[i] = wire.Nodes[i].Convert();

            var model = new TreeMap(nodes);
            return model;
        }

        public static NodeKind Convert(this NodeKindWire wire)
        {
            // Do not use direct integer conversion - it may fail silently
            switch (wire)
            {
                case NodeKindWire.Blob: return NodeKind.Blob;
                case NodeKindWire.Tree: return NodeKind.Map;
                default: throw new ArgumentOutOfRangeException(nameof(wire));
            }
        }

        public static NodeKindWire Convert(this NodeKind model)
        {
            // Do not use direct integer conversion - it may fail silently
            switch (model)
            {
                case NodeKind.Blob: return NodeKindWire.Blob;
                case NodeKind.Map: return NodeKindWire.Tree;
                default: throw new ArgumentOutOfRangeException(nameof(model));
            }
        }

        #endregion
    }
}
