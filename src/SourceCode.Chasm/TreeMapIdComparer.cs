#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System.Collections.Generic;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a way to compare different <see cref="TreeMapId"/> values.
    /// </summary>
    public abstract class TreeMapIdComparer : IEqualityComparer<TreeMapId>, IComparer<TreeMapId>
    {
        #region Constants

        /// <summary>
        /// Gets a <see cref="TreeMapIdComparer"/> that compares all fields of a <see cref="TreeMapId"/> value.
        /// </summary>
        public static TreeMapIdComparer Default { get; } = new DefaultComparer();

        #endregion

        #region Constructors

        private TreeMapIdComparer()
        { }

        #endregion

        #region IComparer

        /// <inheritdoc/>
        public abstract int Compare(TreeMapId x, TreeMapId y);

        #endregion

        #region IEqualityComparer

        /// <inheritdoc/>
        public abstract bool Equals(TreeMapId x, TreeMapId y);

        /// <inheritdoc/>
        public abstract int GetHashCode(TreeMapId obj);

        #endregion

        #region Concrete

        private sealed class DefaultComparer : TreeMapIdComparer
        {
            #region Methods

            public override int Compare(TreeMapId x, TreeMapId y) => Sha1Comparer.Default.Compare(x.Sha1, y.Sha1);

            public override bool Equals(TreeMapId x, TreeMapId y) => Sha1Comparer.Default.Equals(x.Sha1, y.Sha1);

            public override int GetHashCode(TreeMapId obj) => Sha1Comparer.Default.GetHashCode(obj.Sha1);

            #endregion
        }

        #endregion
    }
}
