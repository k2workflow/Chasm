#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{" + nameof(Sha1) + ".ToString(\"D\"),nq,ac}")]
    public readonly struct TreeMapId : IEquatable<TreeMapId>, IComparable<TreeMapId>
    {
        #region Properties

        public Sha1 Sha1 { get; }

        #endregion

        #region Constructors

        public TreeMapId(in Sha1 sha1)
        {
            Sha1 = sha1;
        }

        #endregion

        #region Helpers

        public KeyValuePair<string, TreeNode> CreateMap(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return new KeyValuePair<string, TreeNode>(key, this);
        }

        #endregion

        #region IEquatable

        public bool Equals(TreeMapId other) => TreeMapIdComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is TreeMapId blobId
            && TreeMapIdComparer.Default.Equals(this, blobId);

        public override int GetHashCode() => TreeMapIdComparer.Default.GetHashCode(this);

        #endregion

        #region IComparable

        public int CompareTo(TreeMapId other) => TreeMapIdComparer.Default.Compare(this, other);

        #endregion

        #region Operators

        public static bool operator ==(TreeMapId x, TreeMapId y) => TreeMapIdComparer.Default.Equals(x, y);

        public static bool operator !=(TreeMapId x, TreeMapId y) => !(x == y);

        public static bool operator >=(TreeMapId x, TreeMapId y) => TreeMapIdComparer.Default.Compare(x, y) >= 0;

        public static bool operator >(TreeMapId x, TreeMapId y) => TreeMapIdComparer.Default.Compare(x, y) > 0;

        public static bool operator <=(TreeMapId x, TreeMapId y) => TreeMapIdComparer.Default.Compare(x, y) <= 0;

        public static bool operator <(TreeMapId x, TreeMapId y) => TreeMapIdComparer.Default.Compare(x, y) < 0;

        public override string ToString() => Sha1.ToString("N"); // Used by callsites as a proxy for .Sha1.ToString()

        #endregion
    }
}
