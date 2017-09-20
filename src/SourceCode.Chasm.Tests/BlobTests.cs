﻿using System;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class BlobTests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Blob_is_empty))]
        public static void Blob_is_empty()
        {
            var noData = new Blob();
            var nullData = new Blob(null);
            var emptyData = new Blob(Array.Empty<byte>());

            Assert.Null(Blob.Empty.Data);
            Assert.Null(nullData.Data);
            Assert.Null(noData.Data);

            Assert.Equal(noData, nullData);
            Assert.Equal(Blob.Empty, noData);
            Assert.Equal(Blob.Empty, nullData);
            Assert.NotEqual(Blob.Empty, emptyData);
            Assert.NotEqual(noData, emptyData);
        }

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
            Assert.Equal(isEqual, new Blob(x) == new Blob(y));
        }
    }
}