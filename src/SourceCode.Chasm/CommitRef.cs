using System;

namespace SourceCode.Chasm
{
    public struct CommitRef : IEquatable<CommitRef>
    {
        #region Constants

        public static CommitRef Empty { get; }

        #endregion

        #region Properties

        public CommitId CommitId { get; }

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

        public bool Equals(CommitRef other)
            => CommitId == other.CommitId;

        public override bool Equals(object obj)
            => obj is CommitRef commitRef
            && Equals(commitRef);

        public override int GetHashCode()
            => CommitId.GetHashCode();

        public static bool operator ==(CommitRef x, CommitRef y) => x.Equals(y);

        public static bool operator !=(CommitRef x, CommitRef y) => !x.Equals(y);

        #endregion

        #region Operators

        public override string ToString()
            => $"{nameof(CommitRef)}: {CommitId}";

        #endregion
    }
}
