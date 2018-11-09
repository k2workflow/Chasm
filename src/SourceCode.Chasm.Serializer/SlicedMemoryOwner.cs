#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Buffers;

// See [ArrayPool.cs](https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/System/Buffers/ArrayPool.cs)
// See [MemoryPool.cs](https://github.com/dotnet/corefx/blob/c5cad0cdbee20f2c5f392f3e4b19c71a274b2f2e/src/System.Memory/src/System/Buffers/MemoryPool.cs)
// See [Create usage guidelines for lifetime management of System.Memory<T>](https://github.com/dotnet/docs/issues/4823)
// See [Memory<T> usage guidelines](https://gist.github.com/GrabYourPitchforks/4c3e1935fd4d9fa2831dbfcab35dffc6)
// See [Memory<T> API documentation and samples](https://gist.github.com/GrabYourPitchforks/8efb15abbd90bc5b128f64981766e834)

namespace SourceCode.Chasm.Serializer
{
    /// <summary>
    /// A specialized <see cref="IMemoryOwner{T}{T}"/> that wraps an existing instance
    /// but slices the Memory.
    /// </summary>
    public sealed class SlicedMemoryOwner<T> : IMemoryOwner<T>
    {
        private IMemoryOwner<T> _owner;
        public Memory<T> Memory { get; private set; }

        public SlicedMemoryOwner(IMemoryOwner<T> owner, int start, int length)
        {
            _owner = owner;
            Memory = _owner.Memory.Slice(start, length);
        }

        public SlicedMemoryOwner(IMemoryOwner<T> owner, int start)
        {
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
