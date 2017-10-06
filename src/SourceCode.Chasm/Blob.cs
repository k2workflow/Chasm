#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

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

        #region Constructors

        public Blob(byte[] data)
        {
            Data = data;
        }

        #endregion

        #region IEquatable

        public bool Equals(Blob other) => BlobComparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is Blob blob
            && BlobComparer.Default.Equals(this, blob);

        public override int GetHashCode() => BlobComparer.Default.GetHashCode(this);

        #endregion

        #region Operators

        public static bool operator ==(Blob x, Blob y) => BlobComparer.Default.Equals(x, y);

        public static bool operator !=(Blob x, Blob y) => !(x == y);

        public override string ToString() => $"{nameof(Blob)}: {Data?.Length ?? 0}";

        #endregion
    }
}
