#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{" + nameof(Author) + ".ToString(),nq,ac} ({" + nameof(TreeId) + "?." + nameof(Chasm.TreeId.Sha1) + ".ToString(\"D\"),nq,ac})")]
    public struct Commit : IEquatable<Commit>
    {
        #region Constants

        public static Commit Empty { get; }

        public static CommitId[] Orphaned { get; } = Array.Empty<CommitId>();

        #endregion

        #region Fields

        private readonly IReadOnlyList<CommitId> _parents; // May be null due to default ctor
        private readonly string _message;

        #endregion

        #region Properties

        public IReadOnlyList<CommitId> Parents => _parents ?? Array.Empty<CommitId>(); // May be null due to default ctor

        public TreeId? TreeId { get; }

        public Audit Author { get; }

        public Audit Committer { get; }

        public string Message => _message ?? string.Empty; // May be null due to default ctor

        #endregion

        #region De/Constructors

        public Commit(IList<CommitId> parents, TreeId? treeId, Audit author, Audit committer, string message)
        {
            TreeId = treeId;
            Author = author;
            Committer = committer;
            _message = message;

            // We choose to coerce empty & null, so de/serialization round-trips with fidelity
            if (parents == null)
            {
                _parents = Array.Empty<CommitId>();
                return;
            }

            // Optimize for common cases 0, 1, 2, N
            switch (parents.Count)
            {
                case 0:
                    _parents = Array.Empty<CommitId>();
                    return;

                case 1:
                    _parents = new CommitId[1] { parents[0] };
                    return;

                case 2:
                    {
                        var cmp = CommitIdComparer.Default.Compare(parents[0], parents[1]);
                        switch (cmp)
                        {
                            // Silently de-duplicate
                            case 0: _parents = new CommitId[1] { parents[0] }; return;

                            // Sort forward
                            case -1: _parents = new CommitId[2] { parents[0], parents[1] }; return;

                            // Sort reverse
                            default: _parents = new CommitId[2] { parents[1], parents[0] }; return;
                        }
                    }

                default:
                    {
                        // Copy
                        var array = new CommitId[parents.Count];
                        for (var i = 0; i < parents.Count; i++)
                            array[i] = parents[i];

                        // Sort: Delegate dispatch faster than interface (https://github.com/dotnet/coreclr/pull/8504)
                        Array.Sort(array, CommitIdComparer.Default.Compare);

                        // Distinct
                        var j = 1;
                        for (var i = 1; i < array.Length; i++)
                        {
                            // If it's a duplicate, silently skip
                            if (CommitIdComparer.Default.Equals(array[i - 1], array[i]))
                                continue;

                            array[j++] = array[i]; // Increment target index iff distinct
                        }

                        if (j < array.Length)
                            Array.Resize(ref array, j);

                        // Assign
                        _parents = array;
                    }
                    return;
            }
        }

        public Commit(CommitId? parent, TreeId? treeId, Audit author, Audit committer, string message)
            : this(parent.HasValue ? new[] { parent.Value } : Array.Empty<CommitId>(), treeId, author, committer, message)
        { }

        public void Deconstruct(out IReadOnlyList<CommitId> parents, out TreeId? treeId, out Audit author, out Audit committer, out string message)
        {
            parents = Parents;
            treeId = TreeId;
            author = Author;
            committer = Committer;
            message = Message;
        }

        #endregion

        #region IEquatable

        public bool Equals(Commit other) => CommitComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is Commit commit
            && CommitComparer.Default.Equals(this, commit);

        public override int GetHashCode() => CommitComparer.Default.GetHashCode(this);

        #endregion

        #region Operators

        public static bool operator ==(Commit x, Commit y) => CommitComparer.Default.Equals(x, y);

        public static bool operator !=(Commit x, Commit y) => !(x == y);

        public override string ToString() => $"{TreeId:D} ({Author})";

        #endregion
    }
}
