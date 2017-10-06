#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;

namespace SourceCode.Chasm
{
    public struct CommitRef : IEquatable<CommitRef>
    {
        #region Constants

        /// <summary>
        /// A singleton representing an empty <see cref="CommitRef"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
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

        public bool Equals(CommitRef other) => CommitRefComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is CommitRef commitRef
            && CommitRefComparer.Default.Equals(this, commitRef);

        public override int GetHashCode() => CommitRefComparer.Default.GetHashCode(this);

        #endregion

        #region Operators

        public static bool operator ==(CommitRef x, CommitRef y) => CommitRefComparer.Default.Equals(x, y);

        public static bool operator !=(CommitRef x, CommitRef y) => !CommitRefComparer.Default.Equals(x, y); // not

        public override string ToString() => CommitId.Sha1.ToString("N"); // Used by callsites as a proxy for .Sha1.ToString()

        #endregion
    }
}
