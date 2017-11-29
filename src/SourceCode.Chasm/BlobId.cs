#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{" + nameof(Sha1) + ".ToString(\"D\"),nq,ac}")]
    public readonly struct BlobId : IEquatable<BlobId>, IComparable<BlobId>
    {
        #region Properties

        public Sha1 Sha1 { get; }

        #endregion

        #region Constructors

        [DebuggerStepThrough]
        public BlobId(in Sha1 sha1)
        {
            Sha1 = sha1;
        }

        #endregion

        #region IEquatable

        public bool Equals(BlobId other) => BlobIdComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is BlobId other
            && Equals(other);

        public override int GetHashCode() => BlobIdComparer.Default.GetHashCode(this);

        #endregion

        #region IComparable

        public int CompareTo(BlobId other) => BlobIdComparer.Default.Compare(this, other);

        #endregion

        #region Operators

        public static bool operator >=(BlobId x, BlobId y) => BlobIdComparer.Default.Compare(x, y) >= 0;

        public static bool operator >(BlobId x, BlobId y) => BlobIdComparer.Default.Compare(x, y) > 0;

        public static bool operator <=(BlobId x, BlobId y) => BlobIdComparer.Default.Compare(x, y) <= 0;

        public static bool operator <(BlobId x, BlobId y) => BlobIdComparer.Default.Compare(x, y) < 0;

        public static bool operator ==(BlobId x, BlobId y) => BlobIdComparer.Default.Equals(x, y);

        public static bool operator !=(BlobId x, BlobId y) => !(x == y);

        public override string ToString() => Sha1.ToString("N"); // Used by callsites as a proxy for .Sha1.ToString()

        #endregion
    }
}
