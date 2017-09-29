using System;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    public struct TreeId : IEquatable<TreeId>
    {
        #region Constants

        public static TreeId Empty { get; }

        public static TreeIdComparer Comparer { get; } = new TreeIdComparer();

        #endregion

        #region Properties

        public Sha1 Sha1 { get; }

        #endregion

        #region De/Constructors

        public TreeId(Sha1 sha1)
        {
            Sha1 = sha1;
        }

        public void Deconstruct(out Sha1 sha1)
        {
            sha1 = Sha1;
        }

        #endregion

        #region IEquatable

        public bool Equals(TreeId other) => Comparer.Equals(this, other);

        public override bool Equals(object obj)
            => obj is TreeId blobId
            && Comparer.Equals(this, blobId);

        public override int GetHashCode() => Comparer.GetHashCode(this);

        #endregion

        #region Operators

        public static bool operator ==(TreeId x, TreeId y) => Comparer.Equals(x, y);

        public static bool operator !=(TreeId x, TreeId y) => !Comparer.Equals(x, y); // not

        public override string ToString() => $"{nameof(TreeId)}: {Sha1}";

        #endregion

        #region Nested

        public sealed class TreeIdComparer : IEqualityComparer<TreeId>, IComparer<TreeId>
        {
            public int Compare(TreeId x, TreeId y) => Sha1.Comparer.Compare(x.Sha1, y.Sha1);

            public bool Equals(TreeId x, TreeId y) => Sha1.Comparer.Equals(x.Sha1, y.Sha1);

            public int GetHashCode(TreeId obj) => Sha1.Comparer.GetHashCode(obj.Sha1);
        }

        #endregion
    }
}
