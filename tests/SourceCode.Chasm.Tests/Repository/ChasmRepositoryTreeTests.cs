using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SourceCode.Chasm.Tests;
using SourceCode.Chasm.Tests.TestObjects;
using SourceCode.Clay;
using Xunit;

namespace SourceCode.Chasm.Repository.Tests
{
    public static class ChasmRepositoryTreeTests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_Branch_CommitRefName))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_Branch_CommitRefName()
        {
            // Arrange
            var mockChasmSerializer = new Mock<RandomChasmSerializer>();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            mockChasmRepository.Setup(i => i.ReadCommitRefAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Yield();
                    return CommitRefTestObject.Random;
                });

            // Action
            TreeNodeMap? actual = await mockChasmRepository.Object.ReadTreeAsync(RandomHelper.String, CommitRefTestObject.Random.Branch, TestValues.CancellationToken);

            // Assert
            Assert.Equal(default, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_Branch_CommitRefName_Empty))]
        public static void ChasmRepositoryTree_ReadTreeAsync_Branch_CommitRefName_Empty()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            Task<ArgumentNullException> actual = Assert.ThrowsAsync<ArgumentNullException>(async () => await mockChasmRepository.Object.ReadTreeAsync(RandomHelper.String, default, TestValues.CancellationToken));

            // Assert
            Assert.Contains("commitRefName", actual.Result.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_Branch_CommitRefName_EmptyBuffer))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_Branch_CommitRefName_EmptyBuffer()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            TreeNodeMap? actual = await mockChasmRepository.Object.ReadTreeAsync(RandomHelper.String, CommitRefTestObject.Random.Branch, TestValues.CancellationToken);

            // Assert
            Assert.Equal(default, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_Branch_Empty))]
        public static void ChasmRepositoryTree_ReadTreeAsync_Branch_Empty()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            Task<ArgumentNullException> actual = Assert.ThrowsAsync<ArgumentNullException>(async () => await mockChasmRepository.Object.ReadTreeAsync(default, CommitRefTestObject.Random.Branch, TestValues.CancellationToken));

            // Assert
            Assert.Contains("branch", actual.Result.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_CommitId))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_CommitId()
        {
            // Arrange
            var mockChasmSerializer = new Mock<RandomChasmSerializer>();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            mockChasmRepository.Setup(i => i.ReadObjectAsync(It.IsAny<Sha1>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Yield();
                    return TestValues.ReadOnlyMemory;
                });

            // Action
            TreeNodeMap? actual = await mockChasmRepository.Object.ReadTreeAsync(CommitIdTestObject.Random, TestValues.CancellationToken);

            // Assert
            Assert.Single(actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_CommitId_EmptyBuffer))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_CommitId_EmptyBuffer()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            TreeNodeMap? actual = await mockChasmRepository.Object.ReadTreeAsync(CommitIdTestObject.Random, TestValues.CancellationToken);

            // Assert
            Assert.Equal(default, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_TreeId))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_TreeId()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            mockChasmRepository.Setup(i => i.ReadObjectAsync(It.IsAny<Sha1>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Yield();
                    return TestValues.ReadOnlyMemory;
                });

            // Action
            TreeNodeMap? actual = await mockChasmRepository.Object.ReadTreeAsync(TreeIdTestObject.Random, TestValues.CancellationToken);

            // Assert
            Assert.Single(actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_TreeId_EmptyBuffer))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_TreeId_EmptyBuffer()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            TreeNodeMap? actual = await mockChasmRepository.Object.ReadTreeAsync(TreeIdTestObject.Random, TestValues.CancellationToken);

            // Assert
            Assert.Equal(default, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds))]
        public static async Task ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            mockChasmRepository.Setup(i => i.ReadObjectBatchAsync(It.IsAny<IEnumerable<Sha1>>(), It.IsAny<CancellationToken>()))
                .Returns<IEnumerable<Sha1>, CancellationToken>(async (objectIds, parallelOptions) =>
                {
                    await Task.Yield();

                    var dictionary = new Dictionary<Sha1, ReadOnlyMemory<byte>>();
                    foreach (Sha1 objectId in objectIds)
                    {
                        dictionary.Add(objectId, TestValues.ReadOnlyMemory);
                    }

                    return new ReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>(dictionary);
                });

            // Action
            IReadOnlyDictionary<TreeId, TreeNodeMap> actual = await mockChasmRepository.Object.ReadTreeBatchAsync(new TreeId[] { TreeIdTestObject.Random }, TestValues.ParallelOptions.CancellationToken);

            // Assert
            Assert.Single(actual);
            Assert.Single(actual.Values);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds_Empty))]
        public static async Task ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds_Empty()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            IReadOnlyDictionary<TreeId, TreeNodeMap> actual = await mockChasmRepository.Object.ReadTreeBatchAsync(null, TestValues.ParallelOptions.CancellationToken);

            // Assert
            Assert.Equal(ImmutableDictionary<TreeId, TreeNodeMap>.Empty, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds_EmptyBuffer))]
        public static async Task ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds_EmptyBuffer()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            mockChasmRepository.Setup(i => i.ReadObjectBatchAsync(It.IsAny<IEnumerable<Sha1>>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Yield();
                    return ImmutableDictionary<Sha1, ReadOnlyMemory<byte>>.Empty;
                });

            // Action
            IReadOnlyDictionary<TreeId, TreeNodeMap> actual = await mockChasmRepository.Object.ReadTreeBatchAsync(new TreeId[] { TreeIdTestObject.Random }, TestValues.ParallelOptions.CancellationToken);

            // Assert
            Assert.Equal(ImmutableDictionary<TreeId, TreeNodeMap>.Empty, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_WriteTreeAsync_Tree))]
        public static async Task ChasmRepositoryTree_WriteTreeAsync_CommitIds()
        {
            // Arrange
            var parents = new List<CommitId> { CommitIdTestObject.Random };
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            CommitId actual = await mockChasmRepository.Object.WriteTreeAsync(parents, TreeNodeMapTestObject.Random, AuditTestObject.Random, AuditTestObject.Random, RandomHelper.String, TestValues.CancellationToken);

            // Assert
            Assert.NotEqual(new CommitId(), actual);
            mockChasmRepository.Verify(i => i.WriteTreeAsync(It.IsAny<TreeNodeMap>(), It.IsAny<CancellationToken>()));
            mockChasmRepository.Verify(i => i.WriteCommitAsync(It.IsAny<Commit>(), It.IsAny<CancellationToken>()));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_WriteTreeAsync_Tree))]
        public static async Task ChasmRepositoryTree_WriteTreeAsync_Tree()
        {
            // Arrange
            var mockChasmSerializer = new RandomChasmSerializer();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            TreeId actual = await mockChasmRepository.Object.WriteTreeAsync(TreeNodeMap.Empty, TestValues.CancellationToken)
                .ConfigureAwait(false);

            // Assert
            Assert.NotEqual(new TreeId(), actual);
            mockChasmRepository.Verify(i => i.WriteObjectAsync(It.IsAny<Sha1>(), It.IsAny<Memory<byte>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
        }
    }
}
