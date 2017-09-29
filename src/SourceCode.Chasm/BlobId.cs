using System;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    public struct BlobId : IEquatable<BlobId>
    {
        #region Constants

        public static BlobId Empty { get; }

        public static BlobIdComparer Comparer { get; } = new BlobIdComparer();

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

        public bool Equals(BlobId other) => Comparer.Equals(this, other);

        public override bool Equals(object obj)
            => obj is BlobId blobId
            && Comparer.Equals(this, blobId);

        public override int GetHashCode() => Comparer.GetHashCode(this);

        #endregion

        #region Operators

        public static bool operator ==(BlobId x, BlobId y) => Comparer.Equals(x, y);

        public static bool operator !=(BlobId x, BlobId y) => !Comparer.Equals(x, y); // not

        public override string ToString() => $"{nameof(BlobId)}: {Sha1}";

        #endregion

        #region Nested

        public sealed class BlobIdComparer : IEqualityComparer<BlobId>, IComparer<BlobId>
        {
            public int Compare(BlobId x, BlobId y) => Sha1.Comparer.Compare(x.Sha1, y.Sha1);

            public bool Equals(BlobId x, BlobId y) => Sha1.Comparer.Equals(x.Sha1, y.Sha1);

            public int GetHashCode(BlobId obj) => Sha1.Comparer.GetHashCode(obj.Sha1);
        }

        #endregion
    }
}
