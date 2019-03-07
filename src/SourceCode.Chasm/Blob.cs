using System;
using System.Diagnostics;
using SourceCode.Clay.Buffers;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{ToString(),nq,ac}")]
    public readonly struct Blob : IEquatable<Blob>
    {
        private static readonly Blob s_empty;

        /// <summary>
        /// A singleton representing an empty <see cref="Blob"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static ref readonly Blob Empty => ref s_empty;

        public ReadOnlyMemory<byte> Data { get; }

        public Blob(ReadOnlyMemory<byte> data)
        {
            Data = data;
        }

        public bool Equals(Blob other) => BufferComparer.Memory.Equals(Data, other.Data);

        public override bool Equals(object obj)
            => obj is Blob other
            && Equals(other);

        public override int GetHashCode() => BufferComparer.Memory.GetHashCode(Data);

        public static bool operator ==(Blob x, Blob y) => x.Equals(y);

        public static bool operator !=(Blob x, Blob y) => !(x == y);

        public override string ToString() => $"{nameof(Data.Length)}: {Data.Length}";
    }
}
