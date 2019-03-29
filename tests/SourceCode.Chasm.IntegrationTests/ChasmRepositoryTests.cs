#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.IO;
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
            var cx = new ChasmRequestContext { CorrelationId = Guid.NewGuid().ToString("D") };

            byte[] buffer = g.ToByteArray();
            Sha1 sha = s_hasher.HashData(buffer);

            // Unknown SHA
            Sha1 usha1 = s_hasher.HashData(Guid.NewGuid().ToByteArray());
            Sha1 usha2 = s_hasher.HashData(Guid.NewGuid().ToByteArray());

            // Blob
            var metadata = new ChasmMetadata("application/json", "file123.txt");
            Sha1 sha2 = await repository.WriteObjectAsync(new ReadOnlyMemory<byte>(buffer), metadata, false, cx, default);
            Assert.Equal(sha, sha2);
            IChasmBlob rdata = await repository.ReadObjectAsync(sha, default);
            Assert.True(rdata != null);
            Assert.Equal(16, rdata.Content.Length);
            Assert.Equal(g, new Guid(rdata.Content.ToArray()));
            Assert.Equal("application/json", rdata.Metadata.ContentType);
            Assert.Equal("file123.txt", rdata.Metadata.Filename);

            IChasmBlob urdata = await repository.ReadObjectAsync(usha1, default);
            Assert.False(urdata != null);

            //
            metadata = new ChasmMetadata("application/text", null);
            sha2 = await repository.WriteObjectAsync(buffer, metadata, true, cx, default);
            Assert.Equal(sha, sha2);

            IChasmBlob cnt2 = await repository.ReadObjectAsync(sha2, default);
            Assert.Equal(buffer, cnt2.Content.ToArray());
            Assert.Equal("application/text", cnt2.Metadata.ContentType);
            Assert.Null(cnt2.Metadata.Filename);

            using (IChasmStream stream2 = await repository.ReadStreamAsync(sha2, cx, default))
            using (var ms = new MemoryStream())
            {
                stream2.Content.CopyTo(ms);
                Assert.Equal(buffer, ms.ToArray());
            }

            using (Stream stream2 = new MemoryStream(buffer))
            {
                sha2 = await repository.WriteObjectAsync(stream2, null, true, cx, default);
            }
            Assert.Equal(sha, sha2);

            using (IChasmStream stream2 = await repository.ReadStreamAsync(sha2, cx, default))
            using (var ms = new MemoryStream())
            {
                stream2.Content.CopyTo(ms);
                Assert.Equal(buffer, ms.ToArray());
            }

            // Tree
            var tree = new TreeNodeMap(
                new TreeNode("firstItem", NodeKind.Blob, sha),
                new TreeNode("secondItem", NodeKind.Blob, sha)
            );
            TreeId treeId = await repository.WriteTreeAsync(tree, cx, default);
            TreeNodeMap? rtree = await repository.ReadTreeAsync(treeId, cx, default);
            Assert.True(rtree.HasValue);
            Assert.Equal(tree, rtree.Value);

            TreeNodeMap? urtree = await repository.ReadTreeAsync(new TreeId(usha1), cx, default);
            Assert.False(urtree.HasValue);
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
                var repo = new AzureBlobChasmRepo(csa, new DiskChasmRepo(@"c:\temp\", serializer));
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
                var repo = new AzureTableChasmRepo(csa, new DiskChasmRepo(@"c:\temp\", serializer));
                await TestRepository(repo);
            }
        }
    }
}
