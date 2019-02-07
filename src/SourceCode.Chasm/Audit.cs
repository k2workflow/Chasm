using System;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{" + nameof(Name) + ".ToString(),nq,ac} ({" + nameof(Timestamp) + ".ToString(\"o\"),nq,ac})")]
    public readonly struct Audit : IEquatable<Audit>
    {
        #region Constants

        private static readonly Audit s_empty;

        /// <summary>
        /// A singleton representing an empty <see cref="Audit"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static ref readonly Audit Empty => ref s_empty;

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
            if (!StringComparer.Ordinal.Equals(Name, other.Name)) return false;
            if (Timestamp != other.Timestamp) return false;

            return true;
        }

        public override bool Equals(object obj)
            => obj is Audit other
            && Equals(other);

        public override int GetHashCode()
        {
#if !NETSTANDARD2_0
            var hc = new HashCode();

            hc.Add(Name ?? string.Empty, StringComparer.Ordinal);
            hc.Add(Timestamp);

            return hc.ToHashCode();
#else
            int hc = 11;
            unchecked
            {
                hc = hc * 7 + Name?.GetHashCode() ?? 0;
                hc = hc * 7 + Timestamp.GetHashCode();
            }
            return hc;
#endif
        }

        #endregion

        #region Operators

        public static bool operator ==(Audit x, Audit y) => x.Equals(y);

        public static bool operator !=(Audit x, Audit y) => !(x == y);

        public override string ToString() => $"{Name} ({Timestamp:o})";

        #endregion
    }
}
