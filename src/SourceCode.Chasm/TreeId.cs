using System;

namespace SourceCode.Chasm
{
    public struct TreeId : IEquatable<TreeId>
    {
        #region Constants

        public static TreeId Empty { get; }

        #endregion

        #region Properties

        public Sha1 Sha1 { get; }

        #endregion

        #region De/Constructors

        public TreeId(Sha1 sha1)
        {
            Sha1 = sha1;
        }

        public void Deconstruct(out Sha1 sha1)
        {
            sha1 = Sha1;
        }

        #endregion

        #region IEquatable

        public bool Equals(TreeId other)
            => Sha1 == other.Sha1;

        public override bool Equals(object obj)
            => obj is TreeId treeId
            && Equals(treeId);

        public override int GetHashCode()
            => Sha1.GetHashCode();

        public static bool operator ==(TreeId x, TreeId y) => x.Equals(y);

        public static bool operator !=(TreeId x, TreeId y) => !x.Equals(y);

        #endregion

        #region Operators

        public override string ToString()
            => $"{nameof(TreeId)}: {Sha1}";

        #endregion
    }
}
