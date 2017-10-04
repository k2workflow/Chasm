using System;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a way to compare different <see cref="TreeNode"/> values.
    /// </summary>
    public abstract class TreeNodeComparer : IEqualityComparer<TreeNode>, IComparer<TreeNode>
    {
        #region Constants

        /// <summary>
        /// Gets a <see cref="TreeNodeComparer"/> that compares all fields of a <see cref="TreeNode"/> value.
        /// </summary>
        public static TreeNodeComparer Default { get; } = new DefaultComparer();

        #endregion

        #region Constructors

        protected TreeNodeComparer()
        { }

        #endregion

        #region IComparer

        /// <inheritdoc/>
        public abstract int Compare(TreeNode x, TreeNode y);

        #endregion

        #region IEqualityComparer

        /// <inheritdoc/>
        public abstract bool Equals(TreeNode x, TreeNode y);

        /// <inheritdoc/>
        public abstract int GetHashCode(TreeNode obj);

        #endregion

        #region Concrete

        private sealed class DefaultComparer : TreeNodeComparer
        {
            internal DefaultComparer()
            { }

            public override int Compare(TreeNode x, TreeNode y)
            {
                // Nodes are always sorted by Name first (see TreeNodeList)
                var cmp = string.CompareOrdinal(x.Name, y.Name);
                if (cmp != 0) return cmp;

                // Then by Sha1 (in order to detect duplicate)
                cmp = Sha1Comparer.Default.Compare(x.Sha1, y.Sha1);
                if (cmp != 0) return cmp;

                // And lastly by Kind
                cmp = ((int)x.Kind).CompareTo((int)y.Kind);
                return cmp;
            }

            public override bool Equals(TreeNode x, TreeNode y)
            {
                if (x.Kind != y.Kind) return false;
                if (x.Sha1 != y.Sha1) return false;
                if (!StringComparer.Ordinal.Equals(x.Name, y.Name)) return false;

                return true;
            }

            public override int GetHashCode(TreeNode obj)
            {
                var hc = 11L;

                unchecked
                {
                    hc = hc * 7 + (obj.Name == null ? 0 : StringComparer.Ordinal.GetHashCode(obj.Name));
                    hc = hc * 7 + (int)obj.Kind;
                    hc = hc * 7 + obj.Sha1.GetHashCode();
                }

                return ((int)(hc >> 32)) ^ (int)hc;
            }
        }

        #endregion
    }
}
