#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

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

        protected BlobIdComparer()
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
            #region Constructors

            internal DefaultComparer()
            { }

            #endregion

            #region Methods

            public override int Compare(BlobId x, BlobId y) => Sha1Comparer.Default.Compare(x.Sha1, y.Sha1);

            public override bool Equals(BlobId x, BlobId y) => Sha1Comparer.Default.Equals(x.Sha1, y.Sha1);

            public override int GetHashCode(BlobId obj) => Sha1Comparer.Default.GetHashCode(obj.Sha1);

            #endregion
        }

        #endregion
    }
}
