using System;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    public struct TreeNode : IEquatable<TreeNode>, IComparable<TreeNode>
    {
        #region Constants

        public static TreeNode Empty { get; }

        public static TreeNode EmptyBlob { get; } = new TreeNode(null, Sha1.Empty, NodeKind.Blob);

        public static TreeNode EmptyTree { get; } = new TreeNode(null, Sha1.Empty, NodeKind.Tree);

        public static Comparer DefaultComparer { get; } = new Comparer();

        #endregion

        #region Properties

        public string Name { get; }

        public Sha1 Sha1 { get; }

        public NodeKind Kind { get; }

        #endregion

        #region De/Constructors

        private TreeNode(string name, Sha1 sha1, NodeKind kind)
        {
            // Used for .Empty (no validation)

            Name = name;
            Kind = kind;
            Sha1 = sha1;
        }

        public TreeNode(string name, NodeKind kind, Sha1 sha1)
            : this(name, sha1, kind)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (!Enum.IsDefined(typeof(NodeKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        }

        public void Deconstruct(out string name, out NodeKind kind, out Sha1 sha1)
        {
            name = Name;
            sha1 = Sha1;
            kind = Kind;
        }

        public TreeNode(string name, BlobId blobId)
            : this(name, blobId.Sha1, NodeKind.Blob)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
        }

        public TreeNode(string name, TreeId treeId)
            : this(name, treeId.Sha1, NodeKind.Tree)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
        }

        #endregion

        #region IEquatable

        public bool Equals(TreeNode other) => DefaultComparer.Equals(this, other);

        public override bool Equals(object obj)
            => obj is TreeNode node
            && DefaultComparer.Equals(this, node);

        public override int GetHashCode() => DefaultComparer.GetHashCode(this);

        #endregion

        #region IComparable

        public int CompareTo(TreeNode other) => DefaultComparer.Compare(this, other);

        #endregion

        #region Comparer

        public sealed class Comparer : IEqualityComparer<TreeNode>, IComparer<TreeNode>
        {
            internal Comparer()
            { }

            public int Compare(TreeNode x, TreeNode y)
            {
                // Nodes are always sorted by Name first (see TreeNodeList)
                var cmp = StringComparer.Ordinal.Compare(x.Name, y.Name);
                if (cmp != 0) return cmp;

                // Then by Sha1 (in order to detect duplicate)
                cmp = Sha1.DefaultComparer.Compare(x.Sha1, y.Sha1);
                if (cmp != 0) return cmp;

                cmp = ((int)x.Kind).CompareTo((int)y.Kind);
                return cmp;
            }

            public bool Equals(TreeNode x, TreeNode y)
            {
                if (x.Kind != y.Kind) return false;
                if (x.Sha1 != y.Sha1) return false;
                if (!StringComparer.Ordinal.Equals(x.Name, y.Name)) return false;

                return true;
            }

            public int GetHashCode(TreeNode obj)
            {
                var h = 11;

                unchecked
                {
                    h = h * 7 + (obj.Name == null ? 0 : StringComparer.Ordinal.GetHashCode(obj.Name));
                    h = h * 7 + (int)obj.Kind;
                    h = h * 7 + obj.Sha1.GetHashCode();
                }

                return h;
            }

            // TODO: Is this the best place for this method
            // Delegate dispatch faster than interface dispatch (https://github.com/dotnet/coreclr/pull/8504)
            public static int NameComparison(TreeNode x, TreeNode y)
                => string.CompareOrdinal(x.Name, y.Name);

            // TODO: Is this the best place for this method
            // Delegate dispatch faster than interface dispatch (https://github.com/dotnet/coreclr/pull/8504)
            public static int Sha1Comparison(TreeNode x, TreeNode y)
                => Sha1.DefaultComparer.Compare(x.Sha1, y.Sha1);
        }

        #endregion

        #region Operators

        public static bool operator ==(TreeNode x, TreeNode y) => DefaultComparer.Equals(x, y);

        public static bool operator !=(TreeNode x, TreeNode y) => !DefaultComparer.Equals(x, y); // not

        public static bool operator >=(TreeNode x, TreeNode y) => DefaultComparer.Compare(x, y) >= 0;

        public static bool operator >(TreeNode x, TreeNode y) => DefaultComparer.Compare(x, y) > 0;

        public static bool operator <=(TreeNode x, TreeNode y) => DefaultComparer.Compare(x, y) <= 0;

        public static bool operator <(TreeNode x, TreeNode y) => DefaultComparer.Compare(x, y) < 0;

        public override string ToString() => $"{Kind}: {Name} ({Sha1})";

        #endregion
    }
}
