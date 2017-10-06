#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class BlobIdTests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(BlobId_has_empty_sha1))]
        public static void BlobId_has_empty_sha1()
        {
            Assert.Equal(Sha1.Empty, BlobId.Empty.Sha1);
            Assert.Equal(default, BlobId.Empty);
            Assert.Equal(Sha1.Empty.ToString(), BlobId.Empty.ToString());
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(BlobId_equality))]
        public static void BlobId_equality()
        {
            var blobId1 = new BlobId(Sha1.Hash("abc"));
            var blobId2 = new BlobId(Sha1.Hash("abc"));
            var blobId3 = new BlobId(Sha1.Hash("def"));

            Assert.Equal(blobId1.Sha1.ToString(), blobId1.ToString());
            Assert.Equal(blobId2.Sha1.ToString(), blobId2.ToString());
            Assert.Equal(blobId3.Sha1.ToString(), blobId3.ToString());

            Assert.Equal(blobId1, blobId2);
            Assert.Equal(blobId1.GetHashCode(), blobId2.GetHashCode());
            Assert.Equal(blobId1.ToString(), blobId2.ToString());

            Assert.NotEqual(BlobId.Empty, blobId1);
            Assert.NotEqual(BlobId.Empty.GetHashCode(), blobId1.GetHashCode());
            Assert.NotEqual(BlobId.Empty.ToString(), blobId1.ToString());

            Assert.NotEqual(blobId3, blobId1);
            Assert.NotEqual(blobId3.GetHashCode(), blobId1.GetHashCode());
            Assert.NotEqual(blobId3.ToString(), blobId1.ToString());
        }

        #endregion
    }
}
