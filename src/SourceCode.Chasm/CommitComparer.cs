#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Collections.Generic;
using System;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a way to compare different <see cref="Commit"/> values.
    /// </summary>
    public abstract class CommitComparer : IEqualityComparer<Commit>
    {
        #region Constants

        /// <summary>
        /// Gets a <see cref="CommitComparer"/> that compares all fields of a <see cref="Commit"/> value.
        /// </summary>
        public static CommitComparer Default { get; } = new DefaultComparer();

        #endregion

        #region Constructors

        protected CommitComparer()
        { }

        #endregion

        #region IEqualityComparer

        /// <inheritdoc/>
        public abstract bool Equals(Commit x, Commit y);

        /// <inheritdoc/>
        public abstract int GetHashCode(Commit obj);

        #endregion

        #region Concrete

        private sealed class DefaultComparer : CommitComparer
        {
            #region Constructors

            internal DefaultComparer()
            { }

            #endregion

            #region Methods

            public override bool Equals(Commit x, Commit y)
            {
                if (x.CommitUtc != y.CommitUtc) return false;
                if (!x.TreeId.Equals(y.TreeId)) return false;
                if (!StringComparer.Ordinal.Equals(x.CommitMessage, y.CommitMessage)) return false;
                if (!x.Parents.ListEquals(y.Parents, CommitIdComparer.Default, true)) return false;

                return true;
            }

            public override int GetHashCode(Commit obj)
            {
                var hc = 17L;

                unchecked
                {
                    hc = hc * 23 + obj.TreeId.GetHashCode();
                    hc = hc * 23 + obj.CommitUtc.GetHashCode();
                    hc = hc * 23 + (obj.Parents?.Count ?? 42);
                    hc = hc * 23 + (obj.CommitMessage?.Length ?? 0);
                }

                return ((int)(hc >> 32)) ^ (int)hc;
            }

            #endregion
        }

        #endregion
    }
}
