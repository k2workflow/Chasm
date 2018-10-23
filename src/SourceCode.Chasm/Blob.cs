using SourceCode.Clay.Buffers;
using System;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{ToString(),nq,ac}")]
    public readonly struct Blob : IEquatable<Blob>
    {
        #region Constants

        private static readonly Blob s_empty;

        /// <summary>
        /// A singleton representing an empty <see cref="Blob"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static ref readonly Blob Empty => ref s_empty;

        #endregion

        #region Properties

        public byte[] Data { get; }

        #endregion

        #region Constructors

        public Blob(byte[] data)
        {
            Data = data;
        }

        #endregion

        #region IEquatable

        public bool Equals(Blob other) => BufferComparer.Array.Equals(Data, other.Data);

        public override bool Equals(object obj)
            => obj is Blob other
            && Equals(other);

        public override int GetHashCode() => BufferComparer.Array.GetHashCode(Data);

        #endregion

        #region Operators

        public static bool operator ==(Blob x, Blob y) => x.Equals(y);

        public static bool operator !=(Blob x, Blob y) => !(x == y);

        public override string ToString() => nameof(Data.Length) + ": " + (Data == null ? "null" : $"{Data.Length}");

        #endregion
    }
}
