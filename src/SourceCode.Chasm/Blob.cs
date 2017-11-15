#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Buffers;
using System;
using System.Diagnostics;

namespace SourceCode.Chasm
{
    [DebuggerDisplay("{ToString(),nq,ac}")]
    public readonly struct Blob : IEquatable<Blob>
    {
        #region Constants

        public static Blob Empty { get; }

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
            => obj is Blob blob
            && Equals(blob);

        public override int GetHashCode() => BufferComparer.Array.GetHashCode(Data);

        #endregion

        #region Operators

        public static bool operator ==(Blob x, Blob y) => x.Equals(y);

        public static bool operator !=(Blob x, Blob y) => !(x == y);

        public override string ToString() => nameof(Data.Length) + ": " + (Data == null ? "null" : $"{Data.Length}");

        #endregion
    }
}
