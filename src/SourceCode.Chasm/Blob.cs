using System;
using System.Collections.Generic;
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
        public static ref readonly Blob Empty => ref s_empty;

        private readonly byte[] _data;

        public IReadOnlyList<byte> Data => _data;

        public Blob(byte[] data)
        {
            _data = data;
        }

        public override string ToString()
            => nameof(_data.Length) + ": " + (_data == null ? "null" : $"{_data.Length}");

        public bool Equals(Blob other)
            => BufferComparer.Array.Equals(_data, other._data);

        public override bool Equals(object obj)
            => obj is Blob other
            && Equals(other);

        public override int GetHashCode()
            => BufferComparer.Memory.GetHashCode(_data);

        public static bool operator ==(Blob x, Blob y) => x.Equals(y);

        public static bool operator !=(Blob x, Blob y) => !(x == y);
    }
}
