#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SourceCode.Chasm.IO;
using SourceCode.Chasm.Tests.Helpers;
using SourceCode.Chasm.Tests.TestObjects;
using SourceCode.Clay.Collections.Generic;
using Xunit;

namespace SourceCode.Chasm.Tests.IO
{
    public static class ChasmRepositoryTreeTests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_Branch_CommitRefName))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_Branch_CommitRefName()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            mockChasmSerializer.Setup(i => i.DeserializeCommit(It.IsAny<ReadOnlySpan<byte>>())).Returns(CommitTestObject.Random);
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
            var actual = await mockChasmRepository.Object.ReadTreeAsync(RandomHelper.String, CommitRefTestObject.Random.Name, TestValues.CancellationToken);

            // Assert
            Assert.Equal(TreeNodeList.Empty, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_Branch_CommitRefName_Empty))]
        public static void ChasmRepositoryTree_ReadTreeAsync_Branch_CommitRefName_Empty()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            var actual = Assert.ThrowsAsync<ArgumentNullException>(async () => await mockChasmRepository.Object.ReadTreeAsync(RandomHelper.String, default, TestValues.CancellationToken));

            // Assert
            Assert.Contains("commitRefName", actual.Result.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_Branch_CommitRefName_EmptyBuffer))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_Branch_CommitRefName_EmptyBuffer()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeAsync(RandomHelper.String, CommitRefTestObject.Random.Name, TestValues.CancellationToken);

            // Assert
            Assert.Equal(TreeNodeList.Empty, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_Branch_Empty))]
        public static void ChasmRepositoryTree_ReadTreeAsync_Branch_Empty()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            var actual = Assert.ThrowsAsync<ArgumentNullException>(async () => await mockChasmRepository.Object.ReadTreeAsync(default, CommitRefTestObject.Random.Name, TestValues.CancellationToken));

            // Assert
            Assert.Contains("branch", actual.Result.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_CommitId))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_CommitId()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            mockChasmSerializer.Setup(i => i.DeserializeCommit(It.IsAny<ReadOnlySpan<byte>>())).Returns(CommitTestObject.Random);
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
            var actual = await mockChasmRepository.Object.ReadTreeAsync(CommitIdTestObject.Random, TestValues.CancellationToken);

            // Assert
            Assert.Equal(TreeNodeList.Empty, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_CommitId_Empty))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_CommitId_Empty()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeAsync(CommitId.Empty, TestValues.CancellationToken);

            // Assert
            Assert.Equal(TreeNodeList.Empty, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_CommitId_EmptyBuffer))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_CommitId_EmptyBuffer()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeAsync(CommitIdTestObject.Random, TestValues.CancellationToken);

            // Assert
            Assert.Equal(TreeNodeList.Empty, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_TreeId))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_TreeId()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            mockChasmSerializer.Setup(i => i.DeserializeCommit(It.IsAny<ReadOnlySpan<byte>>())).Returns(CommitTestObject.Random);
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
            var actual = await mockChasmRepository.Object.ReadTreeAsync(TreeIdTestObject.Random, TestValues.CancellationToken);

            // Assert
            Assert.Equal(TreeNodeList.Empty, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_TreeId_Empty))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_TreeId_Empty()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeAsync(TreeId.Empty, TestValues.CancellationToken);

            // Assert
            Assert.Equal(TreeNodeList.Empty, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_TreeId_EmptyBuffer))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_TreeId_EmptyBuffer()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeAsync(TreeIdTestObject.Random, TestValues.CancellationToken);

            // Assert
            Assert.Equal(TreeNodeList.Empty, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds))]
        public static async Task ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            mockChasmRepository.Setup(i => i.ReadObjectBatchAsync(It.IsAny<IEnumerable<Sha1>>(), It.IsAny<ParallelOptions>()))
                .Returns<IEnumerable<Sha1>, ParallelOptions>(async (objectIds, parallelOptions) =>
                {
                    await Task.Yield();

                    var dictionary = new Dictionary<Sha1, ReadOnlyMemory<byte>>();
                    foreach (var objectId in objectIds)
                    {
                        dictionary.Add(objectId, TestValues.ReadOnlyMemory);
                    }

                    return new ReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>(dictionary);
                });

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeBatchAsync(new TreeId[] { TreeIdTestObject.Random }, TestValues.ParallelOptions);

            // Assert
            Assert.Equal(1, actual.Count);
            Assert.Equal(TreeNodeList.Empty, actual.Values.FirstOrDefault());
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds_Empty))]
        public static async Task ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds_Empty()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeBatchAsync(null, TestValues.ParallelOptions);

            // Assert
            Assert.Equal(ReadOnlyDictionary.Empty<TreeId, TreeNodeList>(), actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds_EmptyBuffer))]
        public static async Task ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds_EmptyBuffer()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            mockChasmRepository.Setup(i => i.ReadObjectBatchAsync(It.IsAny<IEnumerable<Sha1>>(), It.IsAny<ParallelOptions>()))
                .Returns(async () =>
                {
                    await Task.Yield();
                    return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();
                });

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeBatchAsync(new TreeId[] { TreeIdTestObject.Random }, TestValues.ParallelOptions);

            // Assert
            Assert.Equal(ReadOnlyDictionary.Empty<TreeId, TreeNodeList>(), actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_WriteTreeAsync_Tree))]
        public static async Task ChasmRepositoryTree_WriteTreeAsync_CommitIds()
        {
            // Arrange
            var parents = new List<CommitId> { CommitIdTestObject.Random };
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            var actual = await mockChasmRepository.Object.WriteTreeAsync(parents, TreeNodeListTestObject.Random, AuditTestObject.Random, AuditTestObject.Random, RandomHelper.String, TestValues.CancellationToken);

            // Assert
            Assert.Equal(CommitId.Empty, actual);
            mockChasmRepository.Verify(i => i.WriteTreeAsync(It.IsAny<TreeNodeList>(), It.IsAny<CancellationToken>()));
            mockChasmRepository.Verify(i => i.WriteCommitAsync(It.IsAny<Commit>(), It.IsAny<CancellationToken>()));
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_WriteTreeAsync_Tree))]
        public static async Task ChasmRepositoryTree_WriteTreeAsync_Tree()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            // Action
            var actual = await mockChasmRepository.Object.WriteTreeAsync(TreeNodeList.Empty, TestValues.CancellationToken);

            // Assert
            Assert.Equal(TreeId.Empty, actual);
            mockChasmRepository.Verify(i => i.WriteObjectAsync(It.IsAny<Sha1>(), It.IsAny<ArraySegment<byte>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
        }

        #endregion
    }
}
