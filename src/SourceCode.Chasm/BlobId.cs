using System;
using System.Diagnostics;
using SourceCode.Clay;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{" + nameof(Sha1) + ".ToString(\"D\"),nq,ac}")]
    public readonly struct BlobId : IEquatable<BlobId>, IComparable<BlobId>
    {
        public Sha1 Sha1 { get; }

        [DebuggerStepThrough]
        public BlobId(in Sha1 sha1)
        {
            Sha1 = sha1;
        }

        public override string ToString()
            => Sha1.ToString("n"); // Used by callsites as a proxy for .Sha1.ToString()

        public bool Equals(BlobId other)
            => Sha1.Equals(other.Sha1);

        public override bool Equals(object obj)
            => obj is BlobId other
            && Equals(other);

        public override int GetHashCode()
            => Sha1.GetHashCode();

        public int CompareTo(BlobId other)
            => Sha1.CompareTo(other.Sha1);

        public static bool operator >=(BlobId x, BlobId y) => x.Sha1.CompareTo(y.Sha1) >= 0;

        public static bool operator >(BlobId x, BlobId y) => x.Sha1.CompareTo(y.Sha1) > 0;

        public static bool operator <=(BlobId x, BlobId y) => x.Sha1.CompareTo(y.Sha1) <= 0;

        public static bool operator <(BlobId x, BlobId y) => x.Sha1.CompareTo(y.Sha1) < 0;

        public static bool operator ==(BlobId x, BlobId y) => x.Sha1.Equals(y.Sha1);

        public static bool operator !=(BlobId x, BlobId y) => !(x == y);
    }
}
