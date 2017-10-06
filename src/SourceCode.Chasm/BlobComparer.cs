#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Buffers;
using System.Collections.Generic;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a way to compare different <see cref="Blob"/> values.
    /// </summary>
    public abstract class BlobComparer : IEqualityComparer<Blob>
    {
        #region Constants

        /// <summary>
        /// Gets a <see cref="BlobComparer"/> that compares all fields of a <see cref="Blob"/> value.
        /// </summary>
        public static BlobComparer Default { get; } = new DefaultComparer();

        #endregion

        #region Constructors

        protected BlobComparer()
        { }

        #endregion

        #region IEqualityComparer

        /// <inheritdoc/>
        public abstract bool Equals(Blob x, Blob y);

        /// <inheritdoc/>
        public abstract int GetHashCode(Blob obj);

        #endregion

        #region Concrete

        private sealed class DefaultComparer : BlobComparer
        {
            #region Constructors

            internal DefaultComparer()
            { }

            #endregion

            #region Methods

            public override bool Equals(Blob x, Blob y) => BufferComparer.Array.Equals(x.Data, y.Data);

            public override int GetHashCode(Blob obj) => BufferComparer.Array.GetHashCode(obj.Data);

            #endregion
        }

        #endregion
    }
}
