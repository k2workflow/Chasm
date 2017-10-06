#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System.Collections.Generic;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a way to compare different <see cref="CommitRef"/> values.
    /// </summary>
    public abstract class CommitRefComparer : IEqualityComparer<CommitRef>
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

        #region IEqualityComparer

        /// <inheritdoc/>
        public abstract bool Equals(CommitRef x, CommitRef y);

        /// <inheritdoc/>
        public abstract int GetHashCode(CommitRef obj);

        #endregion

        #region Concrete

        private sealed class DefaultComparer : CommitRefComparer
        {
            #region Constructors

            internal DefaultComparer()
            { }

            #endregion

            #region Methods

            public override bool Equals(CommitRef x, CommitRef y) => CommitIdComparer.Default.Equals(x.CommitId, y.CommitId);

            public override int GetHashCode(CommitRef obj) => CommitIdComparer.Default.GetHashCode(obj.CommitId);

            #endregion
        }

        #endregion
    }
}
