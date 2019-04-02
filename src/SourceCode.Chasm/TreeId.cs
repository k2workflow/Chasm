using System;
using System.Diagnostics;
using SourceCode.Clay;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{" + nameof(Sha1) + ".ToString(\"D\"),nq,ac}")]
    public readonly struct TreeId : IEquatable<TreeId>, IComparable<TreeId>
    {
        public Sha1 Sha1 { get; }

        [DebuggerStepThrough]
        public TreeId(in Sha1 sha1)
        {
            Sha1 = sha1;
        }

        public override string ToString()
            => Sha1.ToString("n"); // Used by callsites as a proxy for .Sha1.ToString()

        public bool Equals(TreeId other)
            => TreeIdComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is TreeId other
            && Equals(other);

        public override int GetHashCode()
            => TreeIdComparer.Default.GetHashCode(this);

        public int CompareTo(TreeId other)
            => TreeIdComparer.Default.Compare(this, other);

        public static bool operator ==(TreeId x, TreeId y) => TreeIdComparer.Default.Equals(x, y);

        public static bool operator !=(TreeId x, TreeId y) => !(x == y);

        public static bool operator >=(TreeId x, TreeId y) => TreeIdComparer.Default.Compare(x, y) >= 0;

        public static bool operator >(TreeId x, TreeId y) => TreeIdComparer.Default.Compare(x, y) > 0;

        public static bool operator <=(TreeId x, TreeId y) => TreeIdComparer.Default.Compare(x, y) <= 0;

        public static bool operator <(TreeId x, TreeId y) => TreeIdComparer.Default.Compare(x, y) < 0;
    }
}
