using SourceCode.Clay.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SourceCode.Chasm
{
    public struct Commit : IEquatable<Commit>
    {
        #region Constants

        public static Commit Empty { get; }

        public static CommitId[] Orphaned { get; } = new[] { CommitId.Empty };

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
            if (commitUtc != default && commitUtc.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(commitUtc));

            TreeId = treeId;
            CommitUtc = commitUtc;
            CommitMessage = commitMessage;

            if (parents == null)
            {
                Parents = null;
            }
            // Optimize for common 0, 1, 2 and N cases
            else if (parents.Count == 0)
            {
                Parents = Array.Empty<CommitId>();
            }
            else
            {
                // TODO: Sha1.Comparer
                Parents = parents.Distinct(CommitId.Comparer).OrderBy(n => n.Sha1, Sha1.Comparer).ToArray();
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

        public bool Equals(Commit other)
        {
            if (CommitUtc != other.CommitUtc) return false;
            if (!TreeId.Equals(other.TreeId)) return false;
            if (!StringComparer.Ordinal.Equals(CommitMessage, other.CommitMessage)) return false;
            if (!Parents.NullableEquals(other.Parents, CommitId.Comparer, true)) return false;

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
                h = h * 7 + (CommitMessage?.Length ?? 0);
            }

            return h;
        }

        public static bool operator ==(Commit x, Commit y) => x.Equals(y);

        public static bool operator !=(Commit x, Commit y) => !(x == y);

        #endregion

        #region Operators

        public override string ToString()
            => $"{nameof(Commit)}: {CommitUtc:o} ({TreeId})";

        #endregion
    }
}
