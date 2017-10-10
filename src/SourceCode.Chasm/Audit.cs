#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{" + nameof(Name) + ".ToString(),nq,ac} ({" + nameof(Timestamp) + ".ToString(\"o\"),nq,ac})")]
    public struct Audit : IEquatable<Audit>
    {
        #region Constants

        /// <summary>
        /// A singleton representing an empty <see cref="Audit"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static Audit Empty { get; }

        #endregion

        #region Fields

        private readonly string _name;

        #endregion

        #region Properties

        public string Name => _name ?? string.Empty; // May be null due to default ctor

        public DateTimeOffset Timestamp { get; }

        #endregion

        #region De/Constructors

        public Audit(string name, DateTimeOffset timestamp)
        {
            _name = name ?? string.Empty;
            Timestamp = timestamp;
        }

        public void Deconstruct(out string name, out DateTimeOffset timestamp)
        {
            name = Name;
            timestamp = Timestamp;
        }

        #endregion

        #region IEquatable

        public bool Equals(Audit other)
        {
            if (StringComparer.Ordinal.Equals(Name, other.Name)) return false;
            if (Timestamp != other.Timestamp) return false;

            return true;
        }

        public override bool Equals(object obj)
            => obj is Audit audit
            && Equals(audit);

        public override int GetHashCode()
        {
            var hc = 17L;

            unchecked
            {
                hc = (hc * 23) + StringComparer.Ordinal.GetHashCode(Name);
                hc = (hc * 23) + Timestamp.GetHashCode();
            }

            return ((int)(hc >> 32)) ^ (int)hc;
        }

        #endregion

        #region Operators

        public static bool operator ==(Audit x, Audit y) => x.Equals(y);

        public static bool operator !=(Audit x, Audit y) => !(x == y);

        public override string ToString() => $"{Name} ({Timestamp:o})";

        #endregion
    }
}
