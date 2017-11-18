#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay;
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

        private CommitComparer()
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
            #region Methods

            public override bool Equals(Commit x, Commit y)
            {
                if (!x.TreeMapId.Equals(y.TreeMapId)) return false;
                if (x.Author != y.Author) return false;
                if (x.Committer != y.Committer) return false;
                if (!StringComparer.Ordinal.Equals(x.Message, y.Message)) return false;
                if (!x.Parents.NullableListEquals(y.Parents, CommitIdComparer.Default)) return false;

                return true;
            }

            public override int GetHashCode(Commit obj)
            {
                var hc = HashCode.Combine(obj.TreeMapId ?? default, TreeMapIdComparer.Default);
                hc = HashCode.Combine(hc, obj.Author, obj.Committer, obj.Parents == null ? 0 : obj.Parents.Count);
                hc = HashCode.Combine(hc, obj.Message ?? string.Empty, StringComparer.Ordinal);

                return hc;
            }

            #endregion
        }

        #endregion
    }
}
