using SourceCode.Clay.Collections.Generic;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a way to compare different <see cref="TreeNodeList"/> values.
    /// </summary>
    public abstract class TreeNodeListComparer : IEqualityComparer<TreeNodeList>
    {
        #region Constants

        /// <summary>
        /// Gets a <see cref="TreeNodeListComparer"/> that compares all fields of a <see cref="TreeNodeList"/> value.
        /// </summary>
        public static TreeNodeListComparer Default { get; } = new DefaultComparer();

        #endregion

        #region Constructors

        protected TreeNodeListComparer()
        { }

        #endregion

        #region IEqualityComparer

        /// <inheritdoc/>
        public abstract bool Equals(TreeNodeList x, TreeNodeList y);

        /// <inheritdoc/>
        public abstract int GetHashCode(TreeNodeList obj);

        #endregion

        #region Concrete

        private sealed class DefaultComparer : TreeNodeListComparer
        {
            internal DefaultComparer()
            { }

            public override bool Equals(TreeNodeList x, TreeNodeList y)
            {
                if (ReferenceEquals(x, y)) return true; // (x, x) or (null, null)
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false; // (x, null) or (null, y)
                if (!x._nodes.NullableEquals(y._nodes, true)) return false;

                return true;
            }

            public override int GetHashCode(TreeNodeList obj)
            {
                var h = 11;

                unchecked
                {
                    h = h * 7 + (obj._nodes?.Length ?? 42);
                }

                return h;
            }
        }

        #endregion
    }
}
