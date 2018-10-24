using System;
using SourceCode.Clay;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class BlobIdTests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(BlobId_equality))]
        public static void BlobId_equality()
        {
            var blobId1 = new BlobId(Sha1.Hash("abc"));
            var blobId2 = new BlobId(Sha1.Hash("abc"));
            var blobId3 = new BlobId(Sha1.Hash("def"));

            Assert.True(blobId1 == blobId2);
            Assert.False(blobId1 != blobId2);
            Assert.True(blobId1.Equals((object)blobId2));
            Assert.False(blobId1.Equals(new object()));

            Assert.Equal(blobId1.Sha1.ToString(), blobId1.ToString());
            Assert.Equal(blobId2.Sha1.ToString(), blobId2.ToString());
            Assert.Equal(blobId3.Sha1.ToString(), blobId3.ToString());

            Assert.Equal(blobId1, blobId2);
            Assert.Equal(blobId1.GetHashCode(), blobId2.GetHashCode());
            Assert.Equal(blobId1.ToString(), blobId2.ToString());

            Assert.NotEqual(blobId3, blobId1);
            Assert.NotEqual(blobId3.GetHashCode(), blobId1.GetHashCode());
            Assert.NotEqual(blobId3.ToString(), blobId1.ToString());
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(BlobId_Compare))]
        public static void BlobId_Compare()
        {
            BlobIdComparer comparer = BlobIdComparer.Default;

            var blobId1 = new BlobId(Sha1.Hash("abc"));
            var blobId2 = new BlobId(Sha1.Hash("abc"));
            var blobId3 = new BlobId(Sha1.Hash("def"));
            BlobId[] list = new[] { blobId1, blobId2, blobId3 };

            Assert.True(blobId1.CompareTo(blobId2) == 0);
            Assert.True(blobId1.CompareTo(blobId3) != 0);

            Array.Sort(list, comparer.Compare);

            Assert.True(list[0] <= list[1]);
            Assert.True(list[2] >= list[1]);
        }
    }
}
