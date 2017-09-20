using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class BlobIdTests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(BlobId_has_empty_sha1))]
        public static void BlobId_has_empty_sha1()
        {
            Assert.Equal(Sha1.Empty, BlobId.Empty.Sha1);
            Assert.Equal(default, BlobId.Empty);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(BlobId_equality))]
        public static void BlobId_equality()
        {
            var blobId1 = new BlobId(Sha1.Hash("abc"));
            var blobId2 = new BlobId(Sha1.Hash("abc"));
            var blobId3 = new BlobId(Sha1.Hash("def"));

            Assert.Equal(blobId1, blobId2);
            Assert.NotEqual(BlobId.Empty, blobId1);
            Assert.NotEqual(blobId3, blobId1);
        }
    }
}
