#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{" + nameof(Sha1) + ".ToString(\"D\"),nq,ac}")]
    public readonly struct CommitId : IEquatable<CommitId>, IComparable<CommitId>
    {
        #region Properties

        public Sha1 Sha1 { get; }

        #endregion

        #region Constructors

        public CommitId(in Sha1 sha1)
        {
            Sha1 = sha1;
        }

        #endregion

        #region IEquatable

        public bool Equals(CommitId other) => CommitIdComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is CommitId blobId
            && CommitIdComparer.Default.Equals(this, blobId);

        public override int GetHashCode() => CommitIdComparer.Default.GetHashCode(this);

        #endregion

        #region IComparable

        public int CompareTo(CommitId other) => CommitIdComparer.Default.Compare(this, other);

        #endregion

        #region Operators

        public static bool operator ==(CommitId x, CommitId y) => CommitIdComparer.Default.Equals(x, y);

        public static bool operator !=(CommitId x, CommitId y) => !(x == y);

        public static bool operator >=(CommitId x, CommitId y) => CommitIdComparer.Default.Compare(x, y) >= 0;

        public static bool operator >(CommitId x, CommitId y) => CommitIdComparer.Default.Compare(x, y) > 0;

        public static bool operator <=(CommitId x, CommitId y) => CommitIdComparer.Default.Compare(x, y) <= 0;

        public static bool operator <(CommitId x, CommitId y) => CommitIdComparer.Default.Compare(x, y) < 0;

        public override string ToString() => Sha1.ToString("N"); // Used by callsites as a proxy for .Sha1.ToString()

        #endregion
    }
}
