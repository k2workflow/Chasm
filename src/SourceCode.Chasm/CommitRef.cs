using System;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    public struct CommitRef : IEquatable<CommitRef>
    {
        #region Constants

        public static CommitRef Empty { get; }

        #endregion

        #region Properties

        public CommitId CommitId { get; }

        public static Comparer DefaultComparer { get; } = new Comparer();

        #endregion

        #region De/Constructors

        public CommitRef(CommitId commitId)
        {
            if (commitId == CommitId.Empty) throw new ArgumentNullException(nameof(commitId));

            CommitId = commitId;
        }

        public void Deconstruct(out CommitId commitId)
        {
            commitId = CommitId;
        }

        #endregion

        #region IEquatable

        public bool Equals(CommitRef other) => DefaultComparer.Equals(this, other);

        public override bool Equals(object obj)
            => obj is CommitRef commitRef
            && DefaultComparer.Equals(this, commitRef);

        public override int GetHashCode() => DefaultComparer.GetHashCode(this);

        #endregion

        #region Comparer

        public sealed class Comparer : IEqualityComparer<CommitRef>
        {
            internal Comparer()
            { }

            public bool Equals(CommitRef x, CommitRef y)
            {
                if (!x.CommitId.Equals(y.CommitId)) return false;

                return true;
            }

            public int GetHashCode(CommitRef obj)
            {
                var h = 11;

                unchecked
                {
                    h = h * 7 + obj.CommitId.GetHashCode();
                }

                return h;
            }
        }

        #endregion

        #region Operators

        public static bool operator ==(CommitRef x, CommitRef y) => DefaultComparer.Equals(x, y);

        public static bool operator !=(CommitRef x, CommitRef y) => !DefaultComparer.Equals(x, y); // not

        public override string ToString() => $"{nameof(CommitRef)}: {CommitId}";

        #endregion
    }
}
