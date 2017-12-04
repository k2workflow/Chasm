#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay;
using System;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{" + nameof(Branch) + ",nq} ({" + nameof(CommitId) + "." + nameof(Chasm.CommitId.Sha1) + ".ToString(\"D\"),nq,ac})")]
    public readonly struct CommitRef : IEquatable<CommitRef>
    {
        #region Constants

        private static readonly CommitRef _empty;

        /// <summary>
        /// A singleton representing an empty <see cref="CommitRef"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static ref readonly CommitRef Empty => ref _empty;

        #endregion

        #region Properties

        public string Branch { get; }

        public CommitId CommitId { get; }

        #endregion

        #region Constructors

        public CommitRef(in string branch, in CommitId commitId)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));

            Branch = branch;
            CommitId = commitId;
        }

        #endregion

        #region IEquatable

        public bool Equals(CommitRef other)
        {
            if (!CommitIdComparer.Default.Equals(CommitId, other.CommitId)) return false;
            if (!StringComparer.Ordinal.Equals(Branch, other.Branch)) return false;

            return true;
        }

        public override bool Equals(object obj)
            => obj is CommitRef other
            && Equals(other);

        public override int GetHashCode() => HashCode.Combine(
            CommitId,
            StringComparer.Ordinal.GetHashCode(Branch ?? string.Empty)
        );

        #endregion

        #region Operators

        public static bool operator ==(CommitRef x, CommitRef y) => x.Equals(y);

        public static bool operator !=(CommitRef x, CommitRef y) => !(x == y);

        public override string ToString() => $"{CommitId.Sha1:D} ({Branch})";

        #endregion
    }
}
