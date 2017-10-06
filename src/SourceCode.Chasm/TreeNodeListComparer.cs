#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Collections.Generic;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a way to compare different <see cref="TreeNodeList"/> values.
    /// </summary>
    public abstract class TreeNodeListComparer : IEqualityComparer<TreeNodeList>
    {
        #region Constants

        /// <summary>
        /// Gets a <see cref="TreeNodeListComparer"/> that compares all fields of a <see cref="TreeNodeList"/> value.
        /// </summary>
        public static TreeNodeListComparer Default { get; } = new DefaultComparer();

        #endregion

        #region Constructors

        protected TreeNodeListComparer()
        { }

        #endregion

        #region IEqualityComparer

        /// <inheritdoc/>
        public abstract bool Equals(TreeNodeList x, TreeNodeList y);

        /// <inheritdoc/>
        public abstract int GetHashCode(TreeNodeList obj);

        #endregion

        #region Concrete

        private sealed class DefaultComparer : TreeNodeListComparer
        {
            #region Constructors

            internal DefaultComparer()
            { }

            #endregion

            #region Methods

            public override bool Equals(TreeNodeList x, TreeNodeList y)
                => x._nodes.MemoryEquals(y._nodes, true);

            public override int GetHashCode(TreeNodeList obj)
            {
                var hc = 17L;

                unchecked
                {
                    hc = hc * 23 + obj._nodes.Length;

                    if (obj._nodes.Length > 0)
                        hc = hc * 23 + obj._nodes.Span[0].GetHashCode();
                }

                return ((int)(hc >> 32)) ^ (int)hc;
            }

            #endregion
        }

        #endregion
    }
}
