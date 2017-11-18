#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{Kind,nq,ac}({Sha1,nq,ac})")]
    public readonly struct TreeNode : IEquatable<TreeNode>, IComparable<TreeNode>
    {
        #region Properties
        
        public Sha1 Sha1 { get; }

        public NodeKind Kind { get; }

        #endregion

        #region De/Constructors
        
        public TreeNode(NodeKind kind, in Sha1 sha1)
        {
            if (!Enum.IsDefined(typeof(NodeKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            Kind = kind;
            Sha1 = sha1;
        }

        public TreeNode(in BlobId blobId)
            : this(NodeKind.Blob, blobId.Sha1)
        { }

        public TreeNode(in TreeMapId treeMapId)
            : this(NodeKind.Map, treeMapId.Sha1)
        { }

        public void Deconstruct(out NodeKind kind, out Sha1 sha1)
        {
            sha1 = Sha1;
            kind = Kind;
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

#pragma warning disable CA2225 // Operator overloads have named alternates
        // Provided by constructor

        public static implicit operator TreeNode(TreeMapId treeMapId) => new TreeNode(treeMapId);

        public static implicit operator TreeNode(BlobId blobId) => new TreeNode(blobId);

        public static implicit operator TreeNode?(TreeMapId? treeMapId) => treeMapId.HasValue ? new TreeNode(treeMapId.Value) : default;

        public static implicit operator TreeNode?(BlobId? blobId) =>  blobId.HasValue ? new TreeNode(blobId.Value) : default;

#pragma warning restore CA2225 // Operator overloads have named alternates

        public override string ToString() => $"{Kind}({Sha1:D})";

        #endregion
    }
}
