#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay;
using System;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{" + nameof(Name) + ",nq} ({" + nameof(CommitId) + "." + nameof(Chasm.CommitId.Sha1) + ".ToString(\"D\"),nq,ac})")]
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

        public string Name { get; }

        public CommitId CommitId { get; }

        #endregion

        #region Constructors

        public CommitRef(string name, CommitId commitId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            Name = name;
            CommitId = commitId;
        }

        #endregion

        #region IEquatable

        public bool Equals(CommitRef other)
        {
            if (!CommitIdComparer.Default.Equals(CommitId, other.CommitId)) return false;
            if (!StringComparer.Ordinal.Equals(Name, other.Name)) return false;

            return true;
        }

        public override bool Equals(object obj)
            => obj is CommitRef commitRef
            && Equals(commitRef);

        public override int GetHashCode()
        {
            var hc = new HashCode();

            hc.Add(CommitId, CommitIdComparer.Default);
            hc.Add(Name ?? string.Empty, StringComparer.Ordinal);

            return hc.ToHashCode();
        }

        #endregion

        #region Operators

        public static bool operator ==(CommitRef x, CommitRef y) => x.Equals(y);

        public static bool operator !=(CommitRef x, CommitRef y) => !(x == y);

        public override string ToString() => $"{CommitId.Sha1:D} ({Name})";

        #endregion
    }
}
