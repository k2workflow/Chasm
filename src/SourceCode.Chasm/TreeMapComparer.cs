#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay;
using SourceCode.Clay.Collections.Generic;
using System;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a way to compare different <see cref="TreeMap"/> values.
    /// </summary>
    public abstract class TreeMapComparer : IEqualityComparer<TreeMap>
    {
        #region Constants

        /// <summary>
        /// Gets a <see cref="TreeMapComparer"/> that compares all fields of a <see cref="TreeMap"/> value.
        /// </summary>
        public static TreeMapComparer Default { get; } = new DefaultComparer();

        #endregion

        #region Constructors

        private TreeMapComparer()
        { }

        #endregion

        #region IEqualityComparer

        /// <inheritdoc/>
        public abstract bool Equals(TreeMap x, TreeMap y);

        /// <inheritdoc/>
        public abstract int GetHashCode(TreeMap obj);

        #endregion

        #region Concrete

        private sealed class DefaultComparer : TreeMapComparer
        {
            #region Methods

            public override bool Equals(TreeMap x, TreeMap y)
            {
                if (x._nodes.Length != y._nodes.Length) return false;
                if (x._nodes.Length == 0) return true;

                var xs = x._nodes.Span;
                var ys = y._nodes.Span;
                for (var i = 0; i < xs.Length; i++)
                {
                    if (!string.Equals(xs[i].Key, xs[i].Key, StringComparison.Ordinal)) return false;
                    if (!xs[i].Value.Equals(ys[i].Value)) return false;
                }

                return true;
            }

            public override int GetHashCode(TreeMap obj)
            {
                var hc = new HashCode();

                hc.Add(obj._nodes.Length);

                return hc.ToHashCode();
            }

            #endregion
        }

        #endregion
    }
}
