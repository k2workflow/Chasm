using System;
using SourceCode.Clay.Buffers;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a way to compare different <see cref="TreeNodeMap"/> values.
    /// </summary>
    public abstract class TreeNodeMapComparer : IEqualityComparer<TreeNodeMap>
    {
        #region Constants

        /// <summary>
        /// Gets a <see cref="TreeNodeMapComparer"/> that compares all fields of a <see cref="TreeNodeMap"/> value.
        /// </summary>
        public static TreeNodeMapComparer Default { get; } = new DefaultComparer();

        #endregion

        #region Constructors

        private TreeNodeMapComparer()
        { }

        #endregion

        #region IEqualityComparer

        /// <inheritdoc/>
        public abstract bool Equals(TreeNodeMap x, TreeNodeMap y);

        /// <inheritdoc/>
        public abstract int GetHashCode(TreeNodeMap obj);

        #endregion

        #region Concrete

        private sealed class DefaultComparer : TreeNodeMapComparer
        {
            public override bool Equals(TreeNodeMap x, TreeNodeMap y) => x._nodes.MemoryEquals(y._nodes);

            public override int GetHashCode(TreeNodeMap obj) => HashCode.Combine(obj._nodes.Length);
        }

        #endregion
    }
}
