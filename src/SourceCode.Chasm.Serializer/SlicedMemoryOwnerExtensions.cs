#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Buffers;
using System.Diagnostics;

// See [ArrayPool.cs](https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/System/Buffers/ArrayPool.cs)
// See [MemoryPool.cs](https://github.com/dotnet/corefx/blob/c5cad0cdbee20f2c5f392f3e4b19c71a274b2f2e/src/System.Memory/src/System/Buffers/MemoryPool.cs)
// See [Create usage guidelines for lifetime management of System.Memory<T>](https://github.com/dotnet/docs/issues/4823)
// See [Memory<T> usage guidelines](https://gist.github.com/GrabYourPitchforks/4c3e1935fd4d9fa2831dbfcab35dffc6)
// See [Memory<T> API documentation and samples](https://gist.github.com/GrabYourPitchforks/8efb15abbd90bc5b128f64981766e834)

namespace SourceCode.Chasm.Serializer
{
    public static class SlicedMemoryOwnerExtensions
    {
        public static IMemoryOwner<T> Slice<T>(this IMemoryOwner<T> owner, int start, int length)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (start < 0 || start >= owner.Memory.Length) throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0 || length > owner.Memory.Length - start) throw new ArgumentOutOfRangeException(nameof(length));

            return new SliceOwner<T>(owner, start, length);
        }

        public static IMemoryOwner<T> Slice<T>(this IMemoryOwner<T> owner, int start)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (start < 0 || start >= owner.Memory.Length) throw new ArgumentOutOfRangeException(nameof(start));

            return new SliceOwner<T>(owner, start);
        }

        /// <summary>
        /// A specialized <see cref="IMemoryOwner{T}{T}"/> that wraps an existing instance
        /// but slices the Memory.
        /// </summary>
        internal sealed class SliceOwner<T> : IMemoryOwner<T>
        {
            private IMemoryOwner<T> _owner;
            public Memory<T> Memory { get; private set; }

            public SliceOwner(IMemoryOwner<T> owner, int start, int length)
            {
                Debug.Assert(owner != null);
                Debug.Assert(start >= 0 && start < owner.Memory.Length);
                Debug.Assert(length >= 0 && length <= owner.Memory.Length - start);

                _owner = owner;
                Memory = _owner.Memory.Slice(start, length);
            }

            public SliceOwner(IMemoryOwner<T> owner, int start)
            {
                Debug.Assert(owner != null);
                Debug.Assert(start >= 0 && start < owner.Memory.Length);

                _owner = owner;
                Memory = _owner.Memory.Slice(start);
            }

            #region IDisposable

            private bool _disposed;

            private void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        _owner.Dispose();
                    }

                    _owner = null;
                    Memory = default;

                    _disposed = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }

            #endregion
        }
    }
}
