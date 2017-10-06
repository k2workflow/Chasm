#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;

namespace SourceCode.Chasm
{
    public struct TreeNode : IEquatable<TreeNode>, IComparable<TreeNode>
    {
        #region Constants

        /// <summary>
        /// A singleton representing an empty <see cref="TreeNode"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static TreeNode Empty { get; }

        /// <summary>
        /// A singleton representing an empty <see cref="TreeNode"/> value with <see cref="Kind"/> set to <see cref="NodeKind.Blob"/>.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static TreeNode EmptyBlob { get; } = new TreeNode(null, Sha1.Empty, NodeKind.Blob);

        /// <summary>
        /// A singleton representing an empty <see cref="TreeNode"/> value with <see cref="Kind"/> set to <see cref="NodeKind.Tree"/>.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static TreeNode EmptyTree { get; } = new TreeNode(null, Sha1.Empty, NodeKind.Tree);

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

        public TreeNode(string name, BlobId blobId)
            : this(name, NodeKind.Blob, blobId.Sha1)
        { }

        public TreeNode(string name, TreeId treeId)
            : this(name, NodeKind.Tree, treeId.Sha1)
        { }

        public void Deconstruct(out string name, out NodeKind kind, out Sha1 sha1)
        {
            name = Name;
            sha1 = Sha1;
            kind = Kind;
        }

        #endregion

        #region IEquatable

        public bool Equals(TreeNode other) => TreeNodeComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is TreeNode node
            && TreeNodeComparer.Default.Equals(this, node);

        public override int GetHashCode() => TreeNodeComparer.Default.GetHashCode(this);

        #endregion

        #region IComparable

        public int CompareTo(TreeNode other) => TreeNodeComparer.Default.Compare(this, other);

        #endregion

        #region Operators

        public static bool operator ==(TreeNode x, TreeNode y) => TreeNodeComparer.Default.Equals(x, y);

        public static bool operator !=(TreeNode x, TreeNode y) => !(x == y);

        public static bool operator >=(TreeNode x, TreeNode y) => TreeNodeComparer.Default.Compare(x, y) >= 0;

        public static bool operator >(TreeNode x, TreeNode y) => TreeNodeComparer.Default.Compare(x, y) > 0;

        public static bool operator <=(TreeNode x, TreeNode y) => TreeNodeComparer.Default.Compare(x, y) <= 0;

        public static bool operator <(TreeNode x, TreeNode y) => TreeNodeComparer.Default.Compare(x, y) < 0;

        public override string ToString() => $"{Kind}: {Name} ({Sha1})";

        #endregion
    }
}
