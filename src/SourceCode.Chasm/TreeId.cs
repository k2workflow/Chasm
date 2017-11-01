#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{" + nameof(Sha1) + ".ToString(\"D\"),nq,ac}")]
    public struct TreeId : IEquatable<TreeId>, IComparable<TreeId>
    {
        #region Properties

        public Sha1 Sha1 { get; }

        #endregion

        #region Constructors

        public TreeId(Sha1 sha1)
        {
            Sha1 = sha1;
        }

        public static TreeId Parse(string hex) => new TreeId(Sha1.Parse(hex));

        public static bool TryParse(string hex, out TreeId value)
        {
            if (Sha1.TryParse(hex, out var sha))
            {
                value = new TreeId(sha);
                return true;
            }
            value = default;
            return false;
        }

        #endregion

        #region IEquatable

        public bool Equals(TreeId other) => TreeIdComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is TreeId blobId
            && TreeIdComparer.Default.Equals(this, blobId);

        public override int GetHashCode() => TreeIdComparer.Default.GetHashCode(this);

        #endregion

        #region IComparable

        public int CompareTo(TreeId other) => TreeIdComparer.Default.Compare(this, other);

        #endregion

        #region Operators

        public static bool operator ==(TreeId x, TreeId y) => TreeIdComparer.Default.Equals(x, y);

        public static bool operator !=(TreeId x, TreeId y) => !(x == y);

        public static bool operator >=(TreeId x, TreeId y) => TreeIdComparer.Default.Compare(x, y) >= 0;

        public static bool operator >(TreeId x, TreeId y) => TreeIdComparer.Default.Compare(x, y) > 0;

        public static bool operator <=(TreeId x, TreeId y) => TreeIdComparer.Default.Compare(x, y) <= 0;

        public static bool operator <(TreeId x, TreeId y) => TreeIdComparer.Default.Compare(x, y) < 0;

        public override string ToString() => Sha1.ToString("N"); // Used by callsites as a proxy for .Sha1.ToString()

        #endregion
    }
}
