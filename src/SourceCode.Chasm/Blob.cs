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
        {
            if (Data == null ^ other.Data == null) return false;
            if (Data == null) return true;

            if (Data.Length != other.Data.Length) return false;

            switch (Data.Length)
            {
                case 0: return true;
                case 1: return Data[0] == other.Data[0];
                default:
                    {
                        for (var i = 0; i < Data.Length; i++)
                            if (Data[i] != other.Data[i])
                                return false;
                        return true;
                    }
            }
        }

        public override bool Equals(object obj)
            => obj is Blob blob
            && Equals(blob);

        public override int GetHashCode()
            => Data.Length.GetHashCode();

        public static bool operator ==(Blob x, Blob y) => x.Equals(y);

        public static bool operator !=(Blob x, Blob y) => !x.Equals(y);

        #endregion

        #region Operators

        public override string ToString()
            => $"{nameof(Blob)}: {Data?.Length ?? 0}";

        #endregion
    }
}
