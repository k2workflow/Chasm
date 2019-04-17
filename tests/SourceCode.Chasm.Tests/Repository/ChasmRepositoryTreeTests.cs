using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SourceCode.Chasm.Tests;
using SourceCode.Chasm.Tests.TestObjects;
using SourceCode.Clay;
using SourceCode.Clay.Collections.Generic;
using Xunit;

namespace SourceCode.Chasm.Repository.Tests
{
    public static class ChasmRepositoryTreeTests
    {
        [Trait("Type", "Unit")]
        [Fact]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_TreeId()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer)
            {
                CallBase = true
            };

            mockChasmRepository.Setup(i => i.ReadObjectAsync(It.IsAny<Sha1>(), It.IsAny<ChasmRequestContext>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Yield();
                    return new ChasmBlob(TestValues.ReadOnlyMemory, null);
                });

            // Action
            TreeNodeMap? actual = await mockChasmRepository.Object.ReadTreeAsync(TreeIdTestObject.Random, TestValues.RequestContext, TestValues.CancellationToken);

            // Assert
            Assert.Single(actual);
        }

        [Trait("Type", "Unit")]
        [Fact]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_TreeId_EmptyBuffer()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer)
            {
                CallBase = true
            };

            // Action
            TreeNodeMap? actual = await mockChasmRepository.Object.ReadTreeAsync(TreeIdTestObject.Random, TestValues.RequestContext, TestValues.CancellationToken);

            // Assert
            Assert.Equal(default, actual);
        }

        // TODO: Fix
        //[Trait("Type", "Unit")]
        //[Fact]
        //public static async Task ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds()
        //{
        //    // Arrange
        //    var mockChasmSerializer = new RandomChasmSerializer();

        //    var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer)
        //    {
        //        CallBase = true
        //    };

        //    mockChasmRepository.Setup(i => i.ReadObjectBatchAsync(It.IsAny<IEnumerable<Sha1>>(), It.IsAny<ChasmRequestContext>(), It.IsAny<CancellationToken>()))
        //        .Returns<IEnumerable<Sha1>, CancellationToken>(async (objectIds, parallelOptions) =>
        //        {
        //            await Task.Yield();

        //            var dictionary = new Dictionary<Sha1, IChasmBlob>();
        //            foreach (Sha1 objectId in objectIds)
        //            {
        //                dictionary.Add(objectId, new ChasmBlob(TestValues.ReadOnlyMemory, null));
        //            }

        //            return new ReadOnlyDictionary<Sha1, IChasmBlob>(dictionary);
        //        });

        //    // Action
        //    IReadOnlyDictionary<TreeId, TreeNodeMap> actual = await mockChasmRepository.Object.ReadTreeBatchAsync(new TreeId[] { TreeIdTestObject.Random }, TestValues.RequestContext, TestValues.ParallelOptions.CancellationToken);

        //    // Assert
        //    Assert.Single(actual);
        //    Assert.Single(actual.Values);
        //}

        [Trait("Type", "Unit")]
        [Fact]
        public static async Task ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds_Empty()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer)
            {
                CallBase = true
            };

            // Action
            IReadOnlyDictionary<TreeId, TreeNodeMap> actual = await mockChasmRepository.Object.ReadTreeBatchAsync(null, null, TestValues.ParallelOptions.CancellationToken);

            // Assert
            Assert.Equal(ImmutableDictionary<TreeId, TreeNodeMap>.Empty, actual);
        }

        [Trait("Type", "Unit")]
        [Fact]
        public static async Task ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds_EmptyBuffer()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer)
            {
                CallBase = true
            };

            mockChasmRepository.Setup(i => i.ReadObjectBatchAsync(It.IsAny<IEnumerable<Sha1>>(), It.IsAny<ChasmRequestContext>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Yield();
                    return EmptyDictionary.Empty<Sha1, IChasmBlob>();
                });

            // Action
            IReadOnlyDictionary<TreeId, TreeNodeMap> actual = await mockChasmRepository.Object.ReadTreeBatchAsync(new TreeId[] { TreeIdTestObject.Random }, TestValues.RequestContext, TestValues.ParallelOptions.CancellationToken);

            // Assert
            Assert.Equal(EmptyDictionary.Empty<TreeId, TreeNodeMap>(), actual);
        }

        [Trait("Type", "Unit")]
        [Fact]
        public static async Task ChasmRepositoryTree_WriteTreeAsync_Tree()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer)
            {
                CallBase = true
            };

            // Action
            TreeId actual = await mockChasmRepository.Object.WriteTreeAsync(TreeNodeMap.Empty, TestValues.RequestContext, TestValues.CancellationToken)
                .ConfigureAwait(false);

            // Assert
            Assert.Equal(new TreeId(), actual);
            mockChasmRepository.Verify(i => i.WriteObjectAsync(It.IsAny<ReadOnlyMemory<byte>>(), null, It.IsAny<bool>(), It.IsAny<ChasmRequestContext>(), It.IsAny<CancellationToken>()));
        }
    }
}
