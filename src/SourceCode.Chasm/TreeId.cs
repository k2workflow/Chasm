#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

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

        public override string ToString() => Sha1.ToString("N"); // Used by callsites as a proxy for .Sha1.ToString()

        #endregion
    }
}
