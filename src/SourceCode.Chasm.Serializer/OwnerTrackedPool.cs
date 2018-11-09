#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Buffers;
using System.Collections.Generic;

// See [ArrayPool.cs](https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/System/Buffers/ArrayPool.cs)
// See [MemoryPool.cs](https://github.com/dotnet/corefx/blob/c5cad0cdbee20f2c5f392f3e4b19c71a274b2f2e/src/System.Memory/src/System/Buffers/MemoryPool.cs)
// See [Create usage guidelines for lifetime management of System.Memory<T>](https://github.com/dotnet/docs/issues/4823)
// See [Memory<T> usage guidelines](https://gist.github.com/GrabYourPitchforks/4c3e1935fd4d9fa2831dbfcab35dffc6)
// See [Memory<T> API documentation and samples](https://gist.github.com/GrabYourPitchforks/8efb15abbd90bc5b128f64981766e834)

namespace SourceCode.Chasm.Serializer
{
    /// <summary>
    /// A specialized <see cref="MemoryPool{T}"/>.
    /// </summary>
    public sealed class OwnerTrackedPool<T> : IDisposable
    {
        private readonly MemoryPool<T> _pool;
        private readonly IList<IMemoryOwner<T>> _owners;

        public int Count => _owners?.Count ?? 0;

        public OwnerTrackedPool(int capacity, MemoryPool<T> pool = null)
        {
            _pool = pool ?? MemoryPool<T>.Shared;
            _owners = new List<IMemoryOwner<T>>(capacity);
        }

        public OwnerTrackedPool(MemoryPool<T> pool = null)
        {
            _pool = pool ?? MemoryPool<T>.Shared;
            _owners = new List<IMemoryOwner<T>>();
        }

        public Memory<T> Rent(int minBufferSize)
        {
            IMemoryOwner<T> owner = _pool.Rent(minBufferSize);
            _owners.Add(owner);

            return owner.Memory;
        }

        #region IDisposable

        private bool _disposed; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    IList<IMemoryOwner<T>> owners = _owners;

                    if (owners != null)
                    {
                        for (int i = 0; i < owners.Count; i++)
                        {
                            IMemoryOwner<T> owner = owners[i];
                            if (owner != null)
                            {
                                owner.Dispose();
                                owners[i] = null;
                            }
                        }
                    }
                }

                _owners.Clear();

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
