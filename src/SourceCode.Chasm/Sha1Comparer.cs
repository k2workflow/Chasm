#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a way to compare different <see cref="Sha1"/> values.
    /// </summary>
    public abstract class Sha1Comparer : IEqualityComparer<Sha1>, IComparer<Sha1>
    {
        #region Constants

        /// <summary>
        /// Gets a <see cref="Sha1Comparer"/> that compares all fields of a <see cref="Sha1"/> value.
        /// </summary>
        public static Sha1Comparer Default { get; } = new DefaultComparer();

        #endregion

        #region Constructors

        private Sha1Comparer()
        { }

        #endregion

        #region IComparer

        /// <inheritdoc/>
        public abstract int Compare(Sha1 x, Sha1 y);

        #endregion

        #region IEqualityComparer

        /// <inheritdoc/>
        public abstract bool Equals(Sha1 x, Sha1 y);

        /// <inheritdoc/>
        public abstract int GetHashCode(Sha1 obj);

        #endregion

        #region Concrete

        private sealed class DefaultComparer : Sha1Comparer
        {
            #region Methods

            public override int Compare(Sha1 x, Sha1 y)
            {
                // CLR returns [-1, 0, +1] for ulong/uint comparisons (vs arbitrary neg/pos values)

                var cmp = x.Blit0.CompareTo(y.Blit0);
                if (cmp != 0) return cmp;

                cmp = x.Blit1.CompareTo(y.Blit1);
                if (cmp != 0) return cmp;

                cmp = x.Blit2.CompareTo(y.Blit2);
                return cmp;
            }

            public override bool Equals(Sha1 x, Sha1 y)
                => x.Blit0 == y.Blit0
                && x.Blit1 == y.Blit1
                && x.Blit2 == y.Blit2;

            public override int GetHashCode(Sha1 obj) => HashCode.Combine(obj.Blit0, obj.Blit1, obj.Blit2);

            #endregion
        }

        #endregion
    }
}
