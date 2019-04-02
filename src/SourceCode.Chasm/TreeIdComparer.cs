using System.Collections.Generic;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a way to compare different <see cref="TreeId"/> values.
    /// </summary>
    internal abstract class TreeIdComparer : IEqualityComparer<TreeId>, IComparer<TreeId>
    {
        /// <summary>
        /// Gets a <see cref="TreeIdComparer"/> that compares all fields of a <see cref="TreeId"/> value.
        /// </summary>
        public static TreeIdComparer Default { get; } = new DefaultComparer();

        private TreeIdComparer()
        { }

        /// <inheritdoc/>
        public abstract int Compare(TreeId x, TreeId y);

        /// <inheritdoc/>
        public abstract bool Equals(TreeId x, TreeId y);

        /// <inheritdoc/>
        public abstract int GetHashCode(TreeId obj);

        private sealed class DefaultComparer : TreeIdComparer
        {
            public override int Compare(TreeId x, TreeId y)
                => x.Sha1.CompareTo(y.Sha1);

            public override bool Equals(TreeId x, TreeId y)
                => x.Sha1.Equals(y.Sha1);

            public override int GetHashCode(TreeId obj)
                => obj.Sha1.GetHashCode();
        }
    }
}
