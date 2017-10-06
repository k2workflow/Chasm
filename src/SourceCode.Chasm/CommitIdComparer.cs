#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System.Collections.Generic;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a way to compare different <see cref="CommitId"/> values.
    /// </summary>
    public abstract class CommitIdComparer : IEqualityComparer<CommitId>, IComparer<CommitId>
    {
        #region Constants

        /// <summary>
        /// Gets a <see cref="CommitIdComparer"/> that compares all fields of a <see cref="CommitId"/> value.
        /// </summary>
        public static CommitIdComparer Default { get; } = new DefaultComparer();

        #endregion

        #region Constructors

        protected CommitIdComparer()
        { }

        #endregion

        #region IComparer

        /// <inheritdoc/>
        public abstract int Compare(CommitId x, CommitId y);

        #endregion

        #region IEqualityComparer

        /// <inheritdoc/>
        public abstract bool Equals(CommitId x, CommitId y);

        /// <inheritdoc/>
        public abstract int GetHashCode(CommitId obj);

        #endregion

        #region Concrete

        private sealed class DefaultComparer : CommitIdComparer
        {
            #region Constructors

            internal DefaultComparer()
            { }

            #endregion

            #region Methods

            public override int Compare(CommitId x, CommitId y) => Sha1Comparer.Default.Compare(x.Sha1, y.Sha1);

            public override bool Equals(CommitId x, CommitId y) => Sha1Comparer.Default.Equals(x.Sha1, y.Sha1);

            public override int GetHashCode(CommitId obj) => Sha1Comparer.Default.GetHashCode(obj.Sha1);

            #endregion
        }

        #endregion
    }
}
