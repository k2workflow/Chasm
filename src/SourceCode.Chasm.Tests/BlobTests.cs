#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class BlobTests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Blob_is_empty))]
        public static void Blob_is_empty()
        {
            var noData = new Blob();
            var nullData = new Blob(null);
            var emptyData = new Blob(Array.Empty<byte>());

            Assert.True(default == Blob.Empty);
            Assert.False(default != Blob.Empty);
            Assert.True(Blob.Empty.Equals((object)Blob.Empty));
            Assert.False(Blob.Empty.Equals(new object()));

            Assert.Null(Blob.Empty.Data);
            Assert.Null(nullData.Data);
            Assert.Null(noData.Data);

            Assert.Equal(noData, nullData);
            Assert.Equal(noData.ToString(), nullData.ToString());
            Assert.Equal("Length: 0", emptyData.ToString());
            Assert.Equal(noData.GetHashCode(), nullData.GetHashCode());

            Assert.Equal(Blob.Empty, noData);
            Assert.Equal(Blob.Empty.GetHashCode(), noData.GetHashCode());

            Assert.Equal(Blob.Empty, nullData);
            Assert.Equal(Blob.Empty.GetHashCode(), nullData.GetHashCode());

            // null and [] both have same hash
            Assert.NotEqual(Blob.Empty, emptyData);
            Assert.NotEqual(noData, emptyData);
        }

#pragma warning disable xUnit1025 // InlineData should be unique within the Theory it belongs to

        [InlineData(null, null, true)]
        [InlineData(new byte[0], null, false)]
        [InlineData(new byte[0], new byte[0], true)]
        [InlineData(new byte[0], new byte[1] { 0 }, false)]
        [InlineData(new byte[1] { 0 }, new byte[1] { 0 }, true)]
        [InlineData(new byte[1] { 0 }, new byte[1] { 1 }, false)]
        [InlineData(new byte[1] { 0 }, new byte[2] { 0, 1 }, false)]
        [InlineData(new byte[2] { 0, 0 }, new byte[2] { 0, 1 }, false)]
        [InlineData(new byte[2] { 0, 1 }, new byte[2] { 0, 1 }, true)]
        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(Blob_equality))]
        public static void Blob_equality(byte[] x, byte[] y, bool isEqual)
        {
            var blob0 = new Blob(x);
            var blob1 = new Blob(y);
            Assert.Equal(isEqual, blob0.Equals((object)blob1));
        }

#pragma warning restore xUnit1025 // InlineData should be unique within the Theory it belongs to

        #endregion
    }
}
