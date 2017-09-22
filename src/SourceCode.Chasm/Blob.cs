using SourceCode.Clay.Buffers;
using System;

namespace SourceCode.Chasm
{
    public struct Blob : IEquatable<Blob>
    {
        #region Constants

        public static Blob Empty { get; }

        #endregion

        #region Properties

        public byte[] Data { get; }

        #endregion

        #region De/Constructors

        public Blob(byte[] data)
        {
            Data = data;
        }

        public void Deconstruct(out byte[] data)
        {
            data = Data;
        }

        #endregion

        #region IEquatable

        public bool Equals(Blob other)
            => BufferComparer.Default.Equals(Data, other.Data); // Callee has necessary null-handling logic

        public override bool Equals(object obj)
            => obj is Blob blob
            && Equals(blob);

        public override int GetHashCode()
            => Data == null ? 0 : Data.Length.GetHashCode();

        public static bool operator ==(Blob x, Blob y) => x.Equals(y);

        public static bool operator !=(Blob x, Blob y) => !x.Equals(y);

        #endregion

        #region Operators

        public override string ToString()
            => $"{nameof(Blob)}: {Data?.Length ?? 0}";

        #endregion
    }
}
