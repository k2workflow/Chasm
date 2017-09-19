using SourceCode.Clay;
using System;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    public struct CommitId : IEquatable<CommitId>
    {
        #region Constants

        public static CommitId Empty { get; }

        public static IEqualityComparer<CommitId> DefaultComparer { get; } = new EqualityComparerImpl();

        #endregion

        #region Properties

        public Sha1 Sha1 { get; }

        #endregion

        #region Constructors

        public CommitId(Sha1 sha1)
        {
            Sha1 = sha1;
        }

        #endregion

        #region IEquatable

        public bool Equals(CommitId other)
            => DefaultComparer.Equals(this, other);

        public override bool Equals(object obj)
            => obj is CommitId commitId
            && DefaultComparer.Equals(this, commitId);

        public override int GetHashCode()
            => DefaultComparer.GetHashCode(this);

        public static bool operator ==(CommitId x, CommitId y) => x.Equals(y);

        public static bool operator !=(CommitId x, CommitId y) => !x.Equals(y);

        #endregion

        #region Operators

        public override string ToString() => $"{Sha1}";

        #endregion

        #region Nested

        private sealed class EqualityComparerImpl : IEqualityComparer<CommitId>
        {
            public bool Equals(CommitId x, CommitId y) => x.Sha1 == y.Sha1;

            public int GetHashCode(CommitId obj) => obj.Sha1.GetHashCode();
        }

        #endregion
    }
}
