using System.Collections.Generic;

namespace SourceCode.Chasm
{
    public static class ChasmExtensions
    {
        public static TreeNodeMap? Merge(this TreeNodeMap? first, in TreeNodeMap? second)
        {
            if (!first.HasValue) return second;
            if (!second.HasValue) return first;
            return first.Value.Merge(second.Value);
        }

        public static TreeNodeMap? Merge(this TreeNodeMap? first, in ICollection<TreeNode> second)
        {
            if (!first.HasValue) return new TreeNodeMap(second);
            return first.Value.Merge(second);
        }

        public static TreeNodeMap? Merge(this TreeNodeMap? first, in TreeNode second)
        {
            if (!first.HasValue) return new TreeNodeMap(second);
            return first.Value.Add(second);
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
    }
}
