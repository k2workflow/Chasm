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
    internal sealed class OwnerTrackingBytePool : MemoryPool<byte>
    {
        private readonly IList<IMemoryOwner<byte>> _owners;

        public override int MaxBufferSize => Int32.MaxValue;

        public int Count => _owners?.Count ?? 0;

        public OwnerTrackingBytePool(int capacity)
        {
            _owners = new List<IMemoryOwner<byte>>(capacity);
        }

        public OwnerTrackingBytePool()
        {
            _owners = new List<IMemoryOwner<byte>>();
        }

        public override IMemoryOwner<byte> Rent(int minBufferSize)
        {
            IMemoryOwner<byte> owner = Shared.Rent(minBufferSize);
            _owners.Add(owner);

            return owner;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IList<IMemoryOwner<byte>> owners = _owners;

                if (owners != null)
                {
                    for (int i = 0; i < owners.Count; i++)
                    {
                        IMemoryOwner<byte> owner = owners[i];
                        if (owner != null)
                        {
                            owner.Dispose();
                            owners[i] = null;
                        }
                    }
                }
            }

            _owners.Clear();
        }
    }
}
