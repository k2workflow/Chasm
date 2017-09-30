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

        public static Comparer DefaultComparer { get; } = new Comparer();

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
                return;
            }

            // Optimize for common cases 0, 1, 2, N
            switch (parents.Count)
            {
                case 0:
                    Parents = Array.Empty<CommitId>();
                    return;

                case 1:
                    Parents = new CommitId[1] { parents[0] };
                    return;

                case 2:
                    {
                        if (parents[0].Sha1 == parents[1].Sha1)
                        {
                            Parents = new CommitId[1] { parents[0] };
                            return;
                        }
                        else
                        {
                            var cmp = Sha1.DefaultComparer.Compare(parents[0].Sha1, parents[1].Sha1);

                            // Sort & assign
                            if (cmp < 0)
                                Parents = new CommitId[2] { parents[0], parents[1] };
                            else
                                Parents = new CommitId[2] { parents[1], parents[0] };
                        }
                    }
                    return;

                default:
                    {
                        Parents = parents
                            .OrderBy(n => n.Sha1, Sha1.DefaultComparer)
                            .Distinct(CommitId.DefaultComparer)
                            .ToArray();
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

        public bool Equals(Commit other) => DefaultComparer.Equals(this, other);

        public override bool Equals(object obj)
            => obj is Commit commit
            && DefaultComparer.Equals(this, commit);

        public override int GetHashCode() => DefaultComparer.GetHashCode(this);

        #endregion

        #region Comparer

        public sealed class Comparer : IEqualityComparer<Commit>
        {
            internal Comparer()
            { }

            public bool Equals(Commit x, Commit y)
            {
                if (x.CommitUtc != y.CommitUtc) return false;
                if (!x.TreeId.Equals(y.TreeId)) return false;
                if (!StringComparer.Ordinal.Equals(x.CommitMessage, y.CommitMessage)) return false;
                if (!x.Parents.NullableEquals(y.Parents, CommitId.DefaultComparer, true)) return false;

                return true;
            }

            public int GetHashCode(Commit obj)
            {
                var h = 11;

                unchecked
                {
                    h = h * 7 + obj.TreeId.GetHashCode();
                    h = h * 7 + obj.CommitUtc.GetHashCode();
                    h = h * 7 + (obj.Parents?.Count ?? 42);
                    h = h * 7 + (obj.CommitMessage?.Length ?? 0);
                }

                return h;
            }
        }

        #endregion

        #region Operators

        public static bool operator ==(Commit x, Commit y) => DefaultComparer.Equals(x, y);

        public static bool operator !=(Commit x, Commit y) => !DefaultComparer.Equals(x, y); // not

        public override string ToString() => $"{nameof(Commit)}: {CommitUtc:o} ({TreeId})";

        #endregion
    }
}
