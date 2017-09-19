using System;

namespace SourceCode.Chasm
{
    public struct BlobId : IEquatable<BlobId>
    {
        #region Constants

        public static BlobId Empty { get; }

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

        public bool Equals(BlobId other)
            => Sha1 == other.Sha1;

        public override bool Equals(object obj)
            => obj is BlobId blobId
            && Equals(blobId);

        public override int GetHashCode()
            => Sha1.GetHashCode();

        public static bool operator ==(BlobId x, BlobId y) => x.Equals(y);

        public static bool operator !=(BlobId x, BlobId y) => !x.Equals(y);

        #endregion

        #region Operators

        public override string ToString()
            => $"{nameof(BlobId)}: {Sha1}";

        #endregion
    }
}
