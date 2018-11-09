#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using SourceCode.Chasm.Serializer;
using Xunit;

namespace SourceCode.Clay.Buffers.Tests
{
    public static class OwnerTrackingBytePoolTests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(OwnerTrackingBytePool_Rent))]
        public static void OwnerTrackingBytePool_Rent()
        {
            OwnerTrackedPool<byte> pool;

            using (pool = new OwnerTrackedPool<byte>())
            {
                Assert.Equal(0, pool.Count);

                Memory<byte> mem1 = pool.Rent(100);
                Assert.True(mem1.Length >= 100);
                Assert.Equal(1, pool.Count);

                Memory<byte> mem2 = pool.Rent(200);
                Assert.True(mem2.Length >= 200);
                Assert.Equal(2, pool.Count);
            }

            Assert.Equal(0, pool.Count);
        }
    }
}
