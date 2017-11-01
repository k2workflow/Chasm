#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{" + nameof(Sha1) + ".ToString(\"D\"),nq,ac}")]
    public struct BlobId : IEquatable<BlobId>, IComparable<BlobId>
    {
        #region Properties

        public Sha1 Sha1 { get; }

        #endregion

        #region Constructors

        public BlobId(Sha1 sha1)
        {
            Sha1 = sha1;
        }

        public static BlobId Parse(string hex) => new BlobId(Sha1.Parse(hex));

        public static bool TryParse(string hex, out BlobId value)
        {
            if (Sha1.TryParse(hex, out var sha))
            {
                value = new BlobId(sha);
                return true;
            }
            value = default;
            return false;
        }

        #endregion

        #region IEquatable

        public bool Equals(BlobId other) => BlobIdComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is BlobId blobId
            && BlobIdComparer.Default.Equals(this, blobId);

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
