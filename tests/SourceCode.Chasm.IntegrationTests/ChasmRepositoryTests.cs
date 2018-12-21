#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using SourceCode.Chasm;
using SourceCode.Chasm.Repository;
using SourceCode.Chasm.Repository.AzureBlob;
using SourceCode.Chasm.Repository.AzureTable;
using SourceCode.Chasm.Repository.Disk;
using SourceCode.Chasm.Serializer.Json;
using SourceCode.Clay;
using Xunit;

namespace SoruceCode.Chasm.IntegrationTests
{
    public static class ChasmRepositoryTests
    {
        private const string DevelopmentStorage = "UseDevelopmentStorage=true";
        private static readonly System.Security.Cryptography.SHA1 s_hasher = System.Security.Cryptography.SHA1.Create();

        private static async Task TestRepository(IChasmRepository repository)
        {
            var g = Guid.NewGuid();

            byte[] buffer = g.ToByteArray();
            Sha1 sha = s_hasher.HashData(buffer);

            // Unknown SHA
            Sha1 usha1 = s_hasher.HashData(Guid.NewGuid().ToByteArray());
            Sha1 usha2 = s_hasher.HashData(Guid.NewGuid().ToByteArray());

            // Blob
            Sha1 sha2 = await repository.WriteObjectAsync(new Memory<byte>(buffer), false, default);
            Assert.Equal(sha, sha2);
            ReadOnlyMemory<byte>? rdata = await repository.ReadObjectAsync(sha, default);
            Assert.True(rdata.HasValue);
            Assert.Equal(16, rdata.Value.Length);
            Assert.Equal(g, new Guid(rdata.Value.ToArray()));

            ReadOnlyMemory<byte>? urdata = await repository.ReadObjectAsync(usha1, default);
            Assert.False(urdata.HasValue);

            //
            sha2 = await repository.WriteObjectAsync(buffer, true, default);
            Assert.Equal(sha, sha2);

            ReadOnlyMemory<byte>? cnt2 = await repository.ReadObjectAsync(sha2, default);
            Assert.Equal(buffer, cnt2.Value.ToArray());

            using (Stream stream2 = await repository.ReadStreamAsync(sha2, default))
            using (var ms = new MemoryStream())
            {
                stream2.CopyTo(ms);
                Assert.Equal(buffer, ms.ToArray());
            }

            using (Stream stream2 = new MemoryStream(buffer))
            {
                sha2 = await repository.WriteObjectAsync(stream2, true, default);
            }
            Assert.Equal(sha, sha2);

            using (Stream stream2 = await repository.ReadStreamAsync(sha2, default))
            using (var ms = new MemoryStream())
            {
                stream2.CopyTo(ms);
                Assert.Equal(buffer, ms.ToArray());
            }

            // Tree
            var tree = new TreeNodeMap(
                new TreeNode("firstItem", NodeKind.Blob, sha),
                new TreeNode("secondItem", NodeKind.Blob, sha)
            );
            TreeId treeId = await repository.WriteTreeAsync(tree, default);
            TreeNodeMap? rtree = await repository.ReadTreeAsync(treeId, default);
            Assert.True(rtree.HasValue);
            Assert.Equal(tree, rtree.Value);

            TreeNodeMap? urtree = await repository.ReadTreeAsync(new TreeId(usha1), default);
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

            Commit? urcommit = await repository.ReadCommitAsync(new CommitId(usha1), default);
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
                repository.WriteCommitRefAsync(null, commitRefName, new CommitRef("production", new CommitId(usha1)), default));

            await Assert.ThrowsAsync<ChasmConcurrencyException>(() =>
                repository.WriteCommitRefAsync(new CommitId(usha2), commitRefName, new CommitRef("production", new CommitId(usha1)), default));

            await repository.WriteCommitRefAsync(null, commitRefName, new CommitRef("dev", commitId), default);
            await repository.WriteCommitRefAsync(null, commitRefName, new CommitRef("staging", new CommitId(usha1)), default);
            await repository.WriteCommitRefAsync(null, commitRefName + "_1", new CommitRef("production", new CommitId(usha1)), default);

            IReadOnlyList<string> names = await repository.GetNamesAsync(default);
            Assert.Contains(names, x => x == commitRefName);
            Assert.Contains(names, x => x == commitRefName + "_1");

            IReadOnlyList<CommitRef> branches = await repository.GetBranchesAsync(commitRefName, default);
            Assert.Contains(branches.Select(x => x.Branch), x => x == "production");
            Assert.Contains(branches.Select(x => x.Branch), x => x == "dev");
            Assert.Contains(branches.Select(x => x.Branch), x => x == "staging");

            Assert.Equal(commitId, branches.First(x => x.Branch == "production").CommitId);
            Assert.Equal(commitId, branches.First(x => x.Branch == "dev").CommitId);
            Assert.Equal(new CommitId(usha1), branches.First(x => x.Branch == "staging").CommitId);

            branches = await repository.GetBranchesAsync(commitRefName + "_1", default);
            Assert.Contains(branches.Select(x => x.Branch), x => x == "production");
            Assert.Equal(new CommitId(usha1), branches.First(x => x.Branch == "production").CommitId);
        }

        [Fact(DisplayName = nameof(DiskChasmRepo_Test))]
        public static async Task DiskChasmRepo_Test()
        {
            string tmp = Path.GetTempFileName();
            File.Delete(tmp);
            try
            {
                if (!tmp.EndsWith('/')) tmp += '/';
                using (var serializer = new JsonChasmSerializer())
                {
                    var repo = new DiskChasmRepo(tmp, serializer);
                    await TestRepository(repo);
                }
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
            using (var serializer = new JsonChasmSerializer())
            {
                var repo = new AzureBlobChasmRepo(csa, serializer);
                await TestRepository(repo);
            }
        }

        [Fact(DisplayName = nameof(AzureTableChasmRepo_Test)
            , Skip = "Azure Table Storage Emulator *STILL* doesn't support everything."
        )]
        public static async Task AzureTableChasmRepo_Test()
        {
            // Use your own cstring here.
            var csa = CloudStorageAccount.Parse(DevelopmentStorage);
            using (var serializer = new JsonChasmSerializer())
            {
                var repo = new AzureTableChasmRepo(csa, serializer);
                await TestRepository(repo);
            }
        }
    }
}
