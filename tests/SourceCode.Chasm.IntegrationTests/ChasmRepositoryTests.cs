#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Microsoft.WindowsAzure.Storage;
using SourceCode.Chasm;
using SourceCode.Chasm.Repository;
using SourceCode.Chasm.Repository.AzureBlob;
using SourceCode.Chasm.Repository.AzureTable;
using SourceCode.Chasm.Repository.Disk;
using SourceCode.Chasm.Serializer.Json;
using SourceCode.Clay;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SoruceCode.Chasm.IntegrationTests
{
    public class ChasmRepositoryTests
    {
        #region Fields

        private const string DevelopmentStorage = "UseDevelopmentStorage=true";

        #endregion

        #region Methods

        private static async Task TestRepository(IChasmRepository repository)
        {
            var g = Guid.NewGuid();

            byte[] data = g.ToByteArray();
            var sha = Sha1.Hash(data);

            // Unknown SHA
            var usha = Sha1.Hash(Guid.NewGuid().ToByteArray());
            var usha2 = Sha1.Hash(Guid.NewGuid().ToByteArray());

            // Blob
            await repository.WriteObjectAsync(sha, new ArraySegment<byte>(data), false, default);
            ReadOnlyMemory<byte>? rdata = (await repository.ReadObjectAsync(sha, default));
            Assert.True(rdata.HasValue);
            Assert.Equal(16, rdata.Value.Length);
            //Assert.Equal(g, rdata.Value.Span.NonPortableCast<byte, Guid>()[0]); // TODO: Fix test

            ReadOnlyMemory<byte>? urdata = await repository.ReadObjectAsync(usha, default);
            Assert.False(urdata.HasValue);

            // Tree
            var tree = new TreeNodeMap(
                new TreeNode("firstItem", NodeKind.Blob, sha),
                new TreeNode("secondItem", NodeKind.Blob, sha)
            );
            TreeId treeId = await repository.WriteTreeAsync(tree, default);
            TreeNodeMap? rtree = await repository.ReadTreeAsync(treeId, default);
            Assert.True(rtree.HasValue);
            Assert.Equal(tree, rtree.Value);

            TreeNodeMap? urtree = await repository.ReadTreeAsync(new TreeId(usha), default);
            Assert.False(urtree.HasValue);

            // Commit
            var commit = new Commit(
                new CommitId?(),
                treeId,
                new Audit("User1", DateTimeOffset.UtcNow.AddDays(-1)),
                new Audit("User2", DateTimeOffset.UtcNow),
                "Initial commit"
            );
            CommitId commitId = await repository.WriteCommitAsync(commit, default);
            Commit? rcommit = await repository.ReadCommitAsync(commitId, default);
            Assert.True(rcommit.HasValue);
            Assert.Equal(commit, rcommit);

            Commit? urcommit = await repository.ReadCommitAsync(new CommitId(usha), default);
            Assert.False(urcommit.HasValue);

            // CommitRef
            string commitRefName = Guid.NewGuid().ToString("N");
            var commitRef = new CommitRef("production", commitId);
            await repository.WriteCommitRefAsync(null, commitRefName, commitRef, default);
            CommitRef? rcommitRef = await repository.ReadCommitRefAsync(commitRefName, commitRef.Branch, default);
            Assert.True(rcommit.HasValue);
            Assert.Equal(commitRef, rcommitRef);

            CommitRef? urcommitRef = await repository.ReadCommitRefAsync(commitRefName + "_", commitRef.Branch, default);
            Assert.False(urcommit.HasValue);

            await Assert.ThrowsAsync<ChasmConcurrencyException>(() =>
                repository.WriteCommitRefAsync(null, commitRefName, new CommitRef("production", new CommitId(usha)), default));

            await Assert.ThrowsAsync<ChasmConcurrencyException>(() =>
                repository.WriteCommitRefAsync(new CommitId(usha2), commitRefName, new CommitRef("production", new CommitId(usha)), default));

            await repository.WriteCommitRefAsync(null, commitRefName, new CommitRef("dev", commitId), default);
            await repository.WriteCommitRefAsync(null, commitRefName, new CommitRef("staging", new CommitId(usha)), default);
            await repository.WriteCommitRefAsync(null, commitRefName + "_1", new CommitRef("production", new CommitId(usha)), default);

            System.Collections.Generic.IReadOnlyList<string> names = await repository.GetNamesAsync(default);
            Assert.Contains(names, x => x == commitRefName);
            Assert.Contains(names, x => x == commitRefName + "_1");

            System.Collections.Generic.IReadOnlyList<CommitRef> branches = await repository.GetBranchesAsync(commitRefName, default);
            Assert.Contains(branches.Select(x => x.Branch), x => x == "production");
            Assert.Contains(branches.Select(x => x.Branch), x => x == "dev");
            Assert.Contains(branches.Select(x => x.Branch), x => x == "staging");

            Assert.Equal(commitId, branches.First(x => x.Branch == "production").CommitId);
            Assert.Equal(commitId, branches.First(x => x.Branch == "dev").CommitId);
            Assert.Equal(new CommitId(usha), branches.First(x => x.Branch == "staging").CommitId);

            branches = await repository.GetBranchesAsync(commitRefName + "_1", default);
            Assert.Contains(branches.Select(x => x.Branch), x => x == "production");
            Assert.Equal(new CommitId(usha), branches.First(x => x.Branch == "production").CommitId);
        }

        [Fact(DisplayName = nameof(DiskChasmRepo_Test))]
        public static async Task DiskChasmRepo_Test()
        {
            string tmp = Path.GetTempFileName();
            File.Delete(tmp);
            try
            {
                if (!tmp.EndsWith('/')) tmp += '/';
                var repo = new DiskChasmRepo(tmp, new JsonChasmSerializer(), CompressionLevel.Optimal);
                await TestRepository(repo);
            }
            finally
            {
                Directory.Delete(tmp, true);
            }
        }

        [Fact(DisplayName = nameof(AzureBlobChasmRepo_Test)
            , Skip = "Azure Table Storage Emulator *STILL* doesn't support everything."
        )]
        public static async Task AzureBlobChasmRepo_Test()
        {
            // Use your own cstring here.
            var csa = CloudStorageAccount.Parse(DevelopmentStorage);
            var repo = new AzureBlobChasmRepo(csa, new JsonChasmSerializer(), CompressionLevel.Optimal);
            await TestRepository(repo);
        }

        [Fact(DisplayName = nameof(AzureTableChasmRepo_Test)
            , Skip = "Azure Table Storage Emulator *STILL* doesn't support everything."
        )]
        public static async Task AzureTableChasmRepo_Test()
        {
            // Use your own cstring here.
            var csa = CloudStorageAccount.Parse(DevelopmentStorage);
            var repo = new AzureTableChasmRepo(csa, new JsonChasmSerializer(), CompressionLevel.Optimal);
            await TestRepository(repo);
        }

        #endregion
    }
}
