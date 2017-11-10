#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System.Collections.Generic;

namespace SourceCode.Chasm
{
    public static class ChasmExtensions
    {
        #region Methods

        public static TreeNodeMap? Merge(this TreeNodeMap? first, TreeNodeMap? second)
        {
            if (!first.HasValue) return second;
            if (!second.HasValue) return first;
            return first.Value.Merge(second.Value);
        }

        public static TreeNodeMap? Merge(this TreeNodeMap? first, ICollection<TreeNode> second)
        {
            if (!first.HasValue) return new TreeNodeMap(second);
            return first.Value.Merge(second);
        }

        public static bool TryGetValue(this TreeNodeMap? map, string key, out TreeNode value)
        {
            if (!map.HasValue)
            {
                value = default;
                return false;
            }
            return map.Value.TryGetValue(key, out value);
        }

        public static bool TryGetValue(this TreeNodeMap? map, string key, NodeKind kind, out TreeNode value)
        {
            if (!map.HasValue)
            {
                value = default;
                return false;
            }
            return map.Value.TryGetValue(key, kind, out value);
        }

        #endregion
    }
}
