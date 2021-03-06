using System;
using System.Diagnostics;
using SourceCode.Clay;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{ToString(),nq,ac}")]
    public readonly struct TreeNode : IEquatable<TreeNode>, IComparable<TreeNode>
    {
        private static readonly TreeNode s_empty;

        /// <summary>
        /// A singleton representing an empty <see cref="TreeNode"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static ref readonly TreeNode Empty => ref s_empty;

        public string Name { get; }

        public Sha1 Sha1 { get; }

        public NodeKind Kind { get; }

        public ReadOnlyMemory<byte>? Data { get; }

        public TreeId TreeId
        {
            get
            {
                if (Kind != NodeKind.Tree)
                    throw new InvalidOperationException($"The node {Name} must be a {nameof(NodeKind.Tree)}.");
                return new TreeId(Sha1);
            }
        }

        public BlobId BlobId
        {
            get
            {
                if (Kind != NodeKind.Blob)
                    throw new InvalidOperationException($"The node {Name} must be a {nameof(NodeKind.Blob)}.");
                return new BlobId(Sha1);
            }
        }

        private TreeNode(string name, in Sha1 sha1, NodeKind kind, ReadOnlyMemory<byte>? data)
        {
            // Used for .Empty (no validation)

            Name = name;
            Kind = kind;
            Sha1 = sha1;
            Data = data;
        }

        public TreeNode(string name, NodeKind kind, in Sha1 sha1, ReadOnlyMemory<byte>? data = null)
            : this(name, sha1, kind, data)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (!Enum.IsDefined(typeof(NodeKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        }

        public TreeNode(string name, in BlobId blobId, ReadOnlyMemory<byte>? data = null)
            : this(name, NodeKind.Blob, blobId.Sha1, data)
        { }

        public TreeNode(string name, in TreeId treeId, ReadOnlyMemory<byte>? data = null)
            : this(name, NodeKind.Tree, treeId.Sha1, data)
        { }

        public void Deconstruct(out string name, out NodeKind kind, out Sha1 sha1, out ReadOnlyMemory<byte>? data)
        {
            name = Name;
            sha1 = Sha1;
            kind = Kind;
            data = Data;
        }

        public override string ToString()
            => $"{Name}: {Kind} ({Sha1:D})";

        public bool Equals(TreeNode other)
            => TreeNodeComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is TreeNode other
            && Equals(other);

        public override int GetHashCode()
            => TreeNodeComparer.Default.GetHashCode(this);

        public int CompareTo(TreeNode other)
            => TreeNodeComparer.Default.Compare(this, other);

        public static bool operator ==(TreeNode x, TreeNode y) => TreeNodeComparer.Default.Equals(x, y);

        public static bool operator !=(TreeNode x, TreeNode y) => !(x == y);

        public static bool operator >=(TreeNode x, TreeNode y) => TreeNodeComparer.Default.Compare(x, y) >= 0;

        public static bool operator >(TreeNode x, TreeNode y) => TreeNodeComparer.Default.Compare(x, y) > 0;

        public static bool operator <=(TreeNode x, TreeNode y) => TreeNodeComparer.Default.Compare(x, y) <= 0;

        public static bool operator <(TreeNode x, TreeNode y) => TreeNodeComparer.Default.Compare(x, y) < 0;
    }
}
