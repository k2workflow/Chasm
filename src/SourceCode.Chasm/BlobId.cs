using System;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    public struct BlobId : IEquatable<BlobId>
    {
        #region Constants

        public static BlobId Empty { get; }

        public static Comparer DefaultComparer { get; } = new Comparer();

        #endregion

        #region Properties

        public Sha1 Sha1 { get; }

        #endregion

        #region De/Constructors

        public BlobId(Sha1 sha1)
        {
            Sha1 = sha1;
        }

        public void Deconstruct(out Sha1 sha1)
        {
            sha1 = Sha1;
        }

        #endregion

        #region IEquatable

        public bool Equals(BlobId other) => DefaultComparer.Equals(this, other);

        public override bool Equals(object obj)
            => obj is BlobId blobId
            && DefaultComparer.Equals(this, blobId);

        public override int GetHashCode() => DefaultComparer.GetHashCode(this);

        #endregion

        #region Comparer

        public sealed class Comparer : IEqualityComparer<BlobId>, IComparer<BlobId>
        {
            internal Comparer()
            { }

            public int Compare(BlobId x, BlobId y) => Sha1.DefaultComparer.Compare(x.Sha1, y.Sha1);

            public bool Equals(BlobId x, BlobId y) => Sha1.DefaultComparer.Equals(x.Sha1, y.Sha1);

            public int GetHashCode(BlobId obj) => Sha1.DefaultComparer.GetHashCode(obj.Sha1);
        }

        #endregion

        #region Operators

        public static bool operator ==(BlobId x, BlobId y) => DefaultComparer.Equals(x, y);

        public static bool operator !=(BlobId x, BlobId y) => !DefaultComparer.Equals(x, y); // not

        public override string ToString() => $"{nameof(BlobId)}: {Sha1}";

        #endregion
    }
}
