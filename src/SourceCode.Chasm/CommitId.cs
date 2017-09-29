using System;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    public struct CommitId : IEquatable<CommitId>
    {
        #region Constants

        public static CommitId Empty { get; }

        public static Comparer DefaultComparer { get; } = new Comparer();

        #endregion

        #region Properties

        public Sha1 Sha1 { get; }

        #endregion

        #region De/Constructors

        public CommitId(Sha1 sha1)
        {
            Sha1 = sha1;
        }

        public void Deconstruct(out Sha1 sha1)
        {
            sha1 = Sha1;
        }

        #endregion

        #region IEquatable

        public bool Equals(CommitId other) => DefaultComparer.Equals(this, other);

        public override bool Equals(object obj)
            => obj is CommitId blobId
            && DefaultComparer.Equals(this, blobId);

        public override int GetHashCode() => DefaultComparer.GetHashCode(this);

        #endregion

        #region Operators

        public static bool operator ==(CommitId x, CommitId y) => DefaultComparer.Equals(x, y);

        public static bool operator !=(CommitId x, CommitId y) => !DefaultComparer.Equals(x, y); // not

        public override string ToString() => $"{nameof(CommitId)}: {Sha1}";

        #endregion

        #region Nested

        public sealed class Comparer : IEqualityComparer<CommitId>, IComparer<CommitId>
        {
            internal Comparer()
            { }

            public int Compare(CommitId x, CommitId y) => Sha1.DefaultComparer.Compare(x.Sha1, y.Sha1);

            public bool Equals(CommitId x, CommitId y) => Sha1.DefaultComparer.Equals(x.Sha1, y.Sha1);

            public int GetHashCode(CommitId obj) => Sha1.DefaultComparer.GetHashCode(obj.Sha1);
        }

        #endregion
    }
}
