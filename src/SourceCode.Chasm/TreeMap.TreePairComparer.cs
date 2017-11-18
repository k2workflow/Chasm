using SourceCode.Clay;
using System;
using System.Collections.Generic;
using System.Text;
using TreePair = System.Collections.Generic.KeyValuePair<string, SourceCode.Chasm.TreeNode>;

namespace SourceCode.Chasm
{
    partial struct TreeMap
    {
        private sealed class TreePairComparer : IComparer<TreePair>, IEqualityComparer<TreePair>
        {
            public static readonly TreePairComparer Default = new TreePairComparer();

            public int Compare(TreePair x, TreePair y)
            {
                var cmp = string.CompareOrdinal(x.Key, y.Key);
                if (cmp != 0) return cmp;

                cmp = x.Value.Kind.CompareTo(y.Value.Kind);
                if (cmp != 0) return cmp;

                cmp = x.Value.Sha1.CompareTo(y.Value.Sha1);
                return cmp;
            }

            public bool Equals(TreePair x, TreePair y)
            {
                if (x.Value.Kind != y.Value.Kind) return false;
                if (x.Value.Sha1 != y.Value.Sha1) return false;
                if (!string.Equals(x.Key, y.Key, StringComparison.Ordinal)) return false;
                return true;
            }

            public int GetHashCode(TreePair obj) => HashCode.Combine(
                StringComparer.Ordinal.GetHashCode(obj.Key ?? string.Empty),
                obj.Value.Kind,
                obj.Value.Sha1
            );
        }
    }
}
