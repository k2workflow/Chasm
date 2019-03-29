using System;
using System.Collections.Generic;
using SourceCode.Clay;
using SourceCode.Clay.Buffers;

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

        /// <summary>
        /// Gets a <see cref="TreeNodeComparer"/> that only compares the <see cref="TreeNode.Name"/> field of a <see cref="TreeNode"/> value.
        /// </summary>
        public static TreeNodeComparer NameOnly { get; } = new NameOnlyComparer();

        #endregion

        #region Constructors

        private TreeNodeComparer()
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
            public override int Compare(TreeNode x, TreeNode y)
            {
                // Nodes are always sorted by Name first (see TreeNodeMap)
                int cmp = string.CompareOrdinal(x.Name, y.Name);
                if (cmp == 0)
                {
                    // Then by Sha1 (in order to detect duplicate)
                    cmp = Sha1Comparer.Default.Compare(x.Sha1, y.Sha1);
                    if (cmp == 0)
                    {
                        // Then by Kind
                        cmp = ((int)x.Kind).CompareTo((int)y.Kind);
                        if (cmp == 0)
                        {
                            // And lastly by Data
                            if (x.Data == null)
                                return y.Data == null ? 0 : -1;

                            if (y.Data == null)
                                return 1;

                            cmp = BufferComparer.Memory.Compare(x.Data.Value, y.Data.Value);
                        }
                    }
                }

                return cmp < 0 ? -1 : (cmp > 0 ? 1 : 0);
            }

            public override bool Equals(TreeNode x, TreeNode y)
            {
                if (x.Kind != y.Kind) return false;
                if (x.Sha1 != y.Sha1) return false;
                if (!StringComparer.Ordinal.Equals(x.Name, y.Name)) return false;

                if (x.Data == null ^ y.Data == null) return false;
                return x.Data == null || x.Data.Value.MemoryEquals(y.Data.Value);
            }

            public override int GetHashCode(TreeNode obj)
            {
#if !NETSTANDARD2_0
                var hc = new HashCode();

                hc.Add(obj.Kind);
                hc.Add(obj.Sha1, Sha1Comparer.Default);
                hc.Add(obj.Name ?? string.Empty, StringComparer.Ordinal);
                hc.Add(obj.Data == null ? 0 : Clay.Buffers.BufferComparer.Memory.GetHashCode(obj.Data.Value));

                return hc.ToHashCode();
#else
                int hc = 11;
                unchecked
                {
                    hc = hc * 7 + (int)obj.Kind;
                    hc = hc * 7 + obj.Sha1.GetHashCode();
                    hc = hc * 7 + obj.Name?.GetHashCode() ?? 0;
                    hc = hc * 7 + (obj.Data == null ? 0 : Clay.Buffers.BufferComparer.Memory.GetHashCode(obj.Data.Value));
                }
                return hc;
#endif
            }
        }

        private sealed class NameOnlyComparer : TreeNodeComparer
        {
            public override int Compare(TreeNode x, TreeNode y) => string.CompareOrdinal(x.Name, y.Name);

            public override bool Equals(TreeNode x, TreeNode y) => StringComparer.Ordinal.Equals(x.Name, y.Name);

            public override int GetHashCode(TreeNode obj)
            {
#if !NETSTANDARD2_0
                var hc = new HashCode();

                hc.Add(obj.Name, StringComparer.Ordinal);

                return hc.ToHashCode();
#else
                int hc = 11;
                unchecked
                {
                    hc = hc * 7 + obj.Name?.GetHashCode() ?? 0;
                }
                return hc;
#endif
            }
        }

        #endregion
    }
}
