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
                if (x._nodes.Length != y._nodes.Length) return false;
                if (x._nodes.IsEmpty) return true;

                var xspan = x._nodes.Span;
                var yspan = y._nodes.Span;

                for (var i = 0; i < xspan.Length; i++)
                    if (xspan[i] != yspan[i])
                        return false;

                return true;
            }

            public override int GetHashCode(TreeNodeList obj)
            {
                var h = 11;

                unchecked
                {
                    h = h * 7 + obj._nodes.Length;

                    if (obj._nodes.Length > 0)
                        h = h * 7 + obj._nodes.Span[0].GetHashCode();
                }

                return h;
            }
        }

        #endregion
    }
}
