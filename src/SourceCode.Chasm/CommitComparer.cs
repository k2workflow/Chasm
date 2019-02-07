using System;
using System.Collections.Generic;
using SourceCode.Clay.Collections.Generic;

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
            public override bool Equals(Commit x, Commit y)
            {
                if (!x.TreeId.Equals(y.TreeId)) return false;
                if (x.Author != y.Author) return false;
                if (x.Committer != y.Committer) return false;
                if (!StringComparer.Ordinal.Equals(x.Message, y.Message)) return false;
                if (!x.Parents.NullableSequenceEqual(y.Parents, CommitIdComparer.Default)) return false;

                return true;
            }

            public override int GetHashCode(Commit obj)
            {
#if !NETSTANDARD2_0
                var hc = new HashCode();

                hc.Add(obj.TreeId ?? default, TreeIdComparer.Default);
                hc.Add(obj.Author);
                hc.Add(obj.Committer);
                hc.Add(obj.Parents?.Count ?? 0);
                hc.Add(obj.Message ?? string.Empty, StringComparer.Ordinal);

                return hc.ToHashCode();
#else
                int hc = 11;
                unchecked
                {
                    hc = hc * 7 + obj.TreeId?.GetHashCode() ?? 0;
                    hc = hc * 7 + obj.Author.GetHashCode();
                    hc = hc * 7 + obj.Committer.GetHashCode();
                    hc = hc * 7 + obj.Parents?.Count ?? 0;
                    hc = hc * 7 + obj.Message?.GetHashCode() ?? 0;
                }
                return hc;
#endif
            }
        }

        #endregion
    }
}
