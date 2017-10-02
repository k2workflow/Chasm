using System.Collections.Generic;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a way to compare different <see cref="CommitRef"/> values.
    /// </summary>
    public abstract class CommitRefComparer : IEqualityComparer<CommitRef>, IComparer<CommitRef>
    {
        #region Constants

        /// <summary>
        /// Gets a <see cref="CommitRefComparer"/> that compares all fields of a <see cref="CommitRef"/> value.
        /// </summary>
        public static CommitRefComparer Default { get; } = new DefaultComparer();

        #endregion

        #region Constructors

        protected CommitRefComparer()
        { }

        #endregion

        #region IComparer

        /// <inheritdoc/>
        public abstract int Compare(CommitRef x, CommitRef y);

        #endregion

        #region IEqualityComparer

        /// <inheritdoc/>
        public abstract bool Equals(CommitRef x, CommitRef y);

        /// <inheritdoc/>
        public abstract int GetHashCode(CommitRef obj);

        #endregion

        #region Concrete

        private sealed class DefaultComparer : CommitRefComparer
        {
            internal DefaultComparer()
            { }

            public override int Compare(CommitRef x, CommitRef y) => CommitIdComparer.Default.Compare(x.CommitId, y.CommitId);

            public override bool Equals(CommitRef x, CommitRef y) => CommitIdComparer.Default.Equals(x.CommitId, y.CommitId);

            public override int GetHashCode(CommitRef obj) => CommitIdComparer.Default.GetHashCode(obj.CommitId);
        }

        #endregion
    }
}
