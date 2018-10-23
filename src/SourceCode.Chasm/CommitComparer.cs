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
                if (!x.TreeId.Equals(y.TreeId)) return false;
                if (x.Author != y.Author) return false;
                if (x.Committer != y.Committer) return false;
                if (!StringComparer.Ordinal.Equals(x.Message, y.Message)) return false;
                if (!x.Parents.NullableSequenceEqual(y.Parents, CommitIdComparer.Default)) return false;

                return true;
            }

            public override int GetHashCode(Commit obj) => HashCode.Combine(
                TreeIdComparer.Default.GetHashCode(obj.TreeId ?? default),
                obj.Author,
                obj.Committer,
                obj.Parents.Count,
                StringComparer.Ordinal.GetHashCode(obj.Message ?? string.Empty)
            );

            #endregion
        }

        #endregion
    }
}
