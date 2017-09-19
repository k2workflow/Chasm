using SourceCode.Clay.Collections.Generic;
using System;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    public struct Commit : IEquatable<Commit>
    {
        #region Constants

        public static Commit Empty { get; }

        private static readonly CommitId[] _orphan = new[] { CommitId.Empty };

        #endregion

        #region Properties

        public IReadOnlyList<CommitId> Parents { get; }

        public TreeId TreeId { get; }

        public DateTime CommitUtc { get; }

        public string CommitMessage { get; }

        #endregion

        #region De/Constructors

        public Commit(IReadOnlyList<CommitId> parents, TreeId treeId, DateTime commitUtc, string commitMessage)
        {
            if (commitUtc.Kind != DateTimeKind.Utc) throw new ArgumentException(nameof(commitUtc));

            Parents = (parents == null || parents.Count == 0) ? _orphan : parents;
            TreeId = treeId;
            CommitUtc = commitUtc;
            CommitMessage = commitMessage;
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

        public bool Equals(Commit other)
        {
            if (CommitUtc != other.CommitUtc) return false;
            if (!StringComparer.Ordinal.Equals(CommitMessage, other.CommitMessage)) return false;
            if (!TreeId.Equals(other.TreeId)) return false;
            if (!Parents.NullableEquals(other.Parents, CommitId.DefaultComparer, true)) return false;

            return true;
        }

        public override bool Equals(object obj)
            => obj is Commit commit
            && Equals(commit);

        public override int GetHashCode()
        {
            var h = 11;

            unchecked
            {
                h = h * 7 + TreeId.GetHashCode();
                h = h * 7 + CommitUtc.GetHashCode();
                h = h * 7 + (Parents?.Count ?? 0);
            }

            return h;
        }

        public static bool operator ==(Commit x, Commit y) => x.Equals(y);

        public static bool operator !=(Commit x, Commit y) => !x.Equals(y);

        #endregion

        #region Operators

        public override string ToString()
            => $"{nameof(Commit)}: {CommitUtc:o} ({TreeId})";

        #endregion
    }
}
