using System;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    partial struct TreeNode
    {
        #region Constants

        /// <summary>
        /// Gets a <see cref="TreeNodeComparer"/> that compares all fields of a <see cref="TreeNode"/> value.
        /// </summary>
        public static TreeNodeComparer DefaultComparer { get; } = new TreeNodeComparer();

        /// <summary>
        /// Gets a <see cref="Comparison{T}"/> that compares all fields of a <see cref="TreeNode"/> value.
        /// </summary>
        public static int DefaultComparison(TreeNode x, TreeNode y)
        {
            // Delegate dispatch faster than interface dispatch (https://github.com/dotnet/coreclr/pull/8504)

            // Nodes are always sorted by Name first (see TreeNodeList)
            var cmp = string.CompareOrdinal(x.Name, y.Name);
            if (cmp != 0) return cmp;

            // Then by Sha1 (in order to detect duplicate)
            cmp = Sha1.DefaultComparer.Compare(x.Sha1, y.Sha1);
            if (cmp != 0) return cmp;

            cmp = ((int)x.Kind).CompareTo((int)y.Kind);
            return cmp;
        }

        #endregion

        #region Nested

        /// <summary>
        /// Represents a way to compare different <see cref="TreeNode"/> values.
        /// </summary>
        public sealed class TreeNodeComparer : IEqualityComparer<TreeNode>, IComparer<TreeNode>
        {
            internal TreeNodeComparer()
            { }

            /// <inheritdoc/>
            public int Compare(TreeNode x, TreeNode y)
                => DefaultComparison(x, y);

            /// <inheritdoc/>
            public bool Equals(TreeNode x, TreeNode y)
            {
                if (x.Kind != y.Kind) return false;
                if (x.Sha1 != y.Sha1) return false;
                if (!StringComparer.Ordinal.Equals(x.Name, y.Name)) return false;

                return true;
            }

            /// <inheritdoc/>
            public int GetHashCode(TreeNode obj)
            {
                var h = 11;

                unchecked
                {
                    h = h * 7 + (obj.Name == null ? 0 : StringComparer.Ordinal.GetHashCode(obj.Name));
                    h = h * 7 + (int)obj.Kind;
                    h = h * 7 + obj.Sha1.GetHashCode();
                }

                return h;
            }
        }

        #endregion
    }
}
