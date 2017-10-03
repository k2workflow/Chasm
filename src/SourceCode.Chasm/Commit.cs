using System;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    public struct Commit : IEquatable<Commit>
    {
        #region Constants

        /// <summary>
        /// A singleton representing an empty <see cref="Commit"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static Commit Empty { get; }

        public static CommitId[] Orphaned { get; } = new[] { CommitId.Empty };

        #endregion

        #region Fields

        private readonly IReadOnlyList<CommitId> _parents; // May be null due to default ctor
        private readonly string _message;

        #endregion

        #region Properties

        public IReadOnlyList<CommitId> Parents => _parents ?? Array.Empty<CommitId>(); // May be null due to default ctor

        public TreeId TreeId { get; }

        public DateTime CommitUtc { get; }

        public string CommitMessage => _message ?? string.Empty; // May be null due to default ctor

        #endregion

        #region De/Constructors

        public Commit(IReadOnlyList<CommitId> parents, TreeId treeId, DateTime commitUtc, string commitMessage)
        {
            if (commitUtc != default && commitUtc.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(commitUtc));

            TreeId = treeId;
            CommitUtc = commitUtc;
            _message = commitMessage ?? string.Empty;

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
                        // Silently de-duplicate
                        if (parents[0].Sha1 == parents[1].Sha1)
                        {
                            _parents = new CommitId[1] { parents[0] };
                            return;
                        }

                        // Else sort
                        var cmp = Sha1Comparer.Default.Compare(parents[0].Sha1, parents[1].Sha1);
                        if (cmp < 0)
                            _parents = new CommitId[2] { parents[0], parents[1] };
                        else
                            _parents = new CommitId[2] { parents[1], parents[0] };
                    }
                    return;

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

        public Commit(CommitId parent, TreeId treeId, DateTime commitUtc, string commitMessage)
            : this(new[] { parent }, treeId, commitUtc, commitMessage)
        { }

        public void Deconstruct(out IReadOnlyList<CommitId> parents, out TreeId treeId, out DateTime commitUtc, out string commitMessage)
        {
            parents = Parents;
            treeId = TreeId;
            commitUtc = CommitUtc;
            commitMessage = CommitMessage;
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

        public static bool operator !=(Commit x, Commit y) => !CommitComparer.Default.Equals(x, y); // not

        public override string ToString() => $"{nameof(Commit)}: {CommitUtc:o} ({TreeId})";

        #endregion
    }
}
