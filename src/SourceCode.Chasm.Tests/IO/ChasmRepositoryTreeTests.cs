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
            var sha1 = Sha1.Hash("abc");
            var commitId = new CommitId(sha1);
            var treeId = new TreeId(sha1);
            var audit = new Audit(Guid.NewGuid().ToString(), DateTimeOffset.MaxValue);
            var commit = new Commit(commitId, treeId, audit, audit, Guid.NewGuid().ToString());
            var commetRef = new CommitRef(Guid.NewGuid().ToString(), commitId);
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            mockChasmSerializer.Setup(i => i.DeserializeCommit(It.IsAny<ReadOnlySpan<byte>>())).Returns(commit);
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            mockChasmRepository.Setup(i => i.ReadCommitRefAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Yield();
                    return commetRef;
                });

            var branch = Guid.NewGuid().ToString();
            var commitRefName = Guid.NewGuid().ToString();

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeAsync(branch, commitRefName, new CancellationToken());

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
            var branch = Guid.NewGuid().ToString();
            var commitRefName = default(string);

            // Action
            var actual = Assert.ThrowsAsync<ArgumentNullException>(async () => await mockChasmRepository.Object.ReadTreeAsync(branch, commitRefName, new CancellationToken()));

            // Assert
            Assert.Contains(nameof(commitRefName), actual.Result.Message);
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

            var branch = Guid.NewGuid().ToString();
            var commitRefName = Guid.NewGuid().ToString();

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeAsync(branch, commitRefName, new CancellationToken());

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
            var branch = default(string);
            var commitRefName = Guid.NewGuid().ToString();

            // Action
            var actual = Assert.ThrowsAsync<ArgumentNullException>(async () => await mockChasmRepository.Object.ReadTreeAsync(branch, commitRefName, new CancellationToken()));

            // Assert
            Assert.Contains(nameof(branch), actual.Result.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_CommitId))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_CommitId()
        {
            // Arrange
            var sha1 = Sha1.Hash("abc");
            var commitId = new CommitId(sha1);
            var treeId = new TreeId(sha1);
            var audit = new Audit();
            var commit = new Commit(commitId, treeId, audit, audit, Guid.NewGuid().ToString());
            var readOnlyMemory = new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3, 4 });
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            mockChasmSerializer.Setup(i => i.DeserializeCommit(It.IsAny<ReadOnlySpan<byte>>())).Returns(commit);
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            mockChasmRepository.Setup(i => i.ReadObjectAsync(It.IsAny<Sha1>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Yield();
                    return readOnlyMemory;
                });

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeAsync(commitId, new CancellationToken());

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
            var actual = await mockChasmRepository.Object.ReadTreeAsync(CommitId.Empty, new CancellationToken());

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

            var commitId = new CommitId(Sha1.Hash("abc"));

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeAsync(commitId, new CancellationToken());

            // Assert
            Assert.Equal(TreeNodeList.Empty, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeAsync_TreeId))]
        public static async Task ChasmRepositoryTree_ReadTreeAsync_TreeId()
        {
            // Arrange
            var sha1 = Sha1.Hash("abc");
            var commitId = new CommitId(sha1);
            var treeId = new TreeId(sha1);
            var audit = new Audit();
            var commit = new Commit(commitId, treeId, audit, audit, Guid.NewGuid().ToString());
            var readOnlyMemory = new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3, 4 });
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            mockChasmSerializer.Setup(i => i.DeserializeCommit(It.IsAny<ReadOnlySpan<byte>>())).Returns(commit);
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, CompressionLevel.NoCompression, 5)
            {
                CallBase = true
            };

            mockChasmRepository.Setup(i => i.ReadObjectAsync(It.IsAny<Sha1>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Yield();
                    return readOnlyMemory;
                });

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeAsync(treeId, new CancellationToken());

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
            var actual = await mockChasmRepository.Object.ReadTreeAsync(TreeId.Empty, new CancellationToken());

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

            var treeId = new TreeId(Sha1.Hash("abc"));

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeAsync(treeId, new CancellationToken());

            // Assert
            Assert.Equal(TreeNodeList.Empty, actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds))]
        public static async Task ChasmRepositoryTree_ReadTreeBatchAsync_TreeIds()
        {
            // Arrange
            var sha1 = Sha1.Hash("abc");
            var readOnlyMemory = new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3, 4 });
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
                        dictionary.Add(objectId, readOnlyMemory);
                    }

                    return new ReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>(dictionary);
                });

            var treeId = new TreeId(Sha1.Hash("abc"));

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeBatchAsync(new TreeId[] { treeId }, new ParallelOptions());

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
            var actual = await mockChasmRepository.Object.ReadTreeBatchAsync(null, new ParallelOptions());

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

            var treeId = new TreeId(Sha1.Hash("abc"));

            // Action
            var actual = await mockChasmRepository.Object.ReadTreeBatchAsync(new TreeId[] { treeId }, new ParallelOptions());

            // Assert
            Assert.Equal(ReadOnlyDictionary.Empty<TreeId, TreeNodeList>(), actual);
        }

        #endregion
    }
}
