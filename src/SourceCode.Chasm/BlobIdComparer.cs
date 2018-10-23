using System.Collections.Generic;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a way to compare different <see cref="BlobId"/> values.
    /// </summary>
    public abstract class BlobIdComparer : IEqualityComparer<BlobId>, IComparer<BlobId>
    {
        #region Constants

        /// <summary>
        /// Gets a <see cref="BlobIdComparer"/> that compares all fields of a <see cref="BlobId"/> value.
        /// </summary>
        public static BlobIdComparer Default { get; } = new DefaultComparer();

        #endregion

        #region Constructors

        private BlobIdComparer()
        { }

        #endregion

        #region IComparer

        /// <inheritdoc/>
        public abstract int Compare(BlobId x, BlobId y);

        #endregion

        #region IEqualityComparer

        /// <inheritdoc/>
        public abstract bool Equals(BlobId x, BlobId y);

        /// <inheritdoc/>
        public abstract int GetHashCode(BlobId obj);

        #endregion

        #region Concrete

        private sealed class DefaultComparer : BlobIdComparer
        {
            #region Methods

            public override int Compare(BlobId x, BlobId y) => x.Sha1.CompareTo(y.Sha1);

            public override bool Equals(BlobId x, BlobId y) => x.Sha1.Equals(y.Sha1);

            public override int GetHashCode(BlobId obj) => obj.Sha1.GetHashCode();

            #endregion
        }

        #endregion
    }
}
