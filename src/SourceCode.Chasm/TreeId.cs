using System;

namespace SourceCode.Chasm
{
    public struct TreeId : IEquatable<TreeId>
    {
        #region Constants

        /// <summary>
        /// A singleton representing an empty <see cref="TreeId"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
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

        public bool Equals(TreeId other) => TreeIdComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is TreeId blobId
            && TreeIdComparer.Default.Equals(this, blobId);

        public override int GetHashCode() => TreeIdComparer.Default.GetHashCode(this);

        #endregion

        #region Operators

        public static bool operator ==(TreeId x, TreeId y) => TreeIdComparer.Default.Equals(x, y);

        public static bool operator !=(TreeId x, TreeId y) => !TreeIdComparer.Default.Equals(x, y); // not

        public override string ToString() => $"{nameof(TreeId)}: {Sha1}";

        #endregion
    }
}
