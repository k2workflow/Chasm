using System;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SourceCode.Chasm.Repository.Disk;

namespace SourceCode.Chasm.Repository.AzureBlob
{
    public sealed partial class AzureBlobChasmRepo : ChasmRepository
    {
        private readonly Lazy<CloudBlobContainer> _refsContainer;
        private readonly Lazy<CloudBlobContainer> _objectsContainer;
        private readonly DiskChasmRepo _diskRepo;

        public AzureBlobChasmRepo(CloudStorageAccount storageAccount, DiskChasmRepo diskRepo)
            : base(diskRepo.Serializer)
        {
            if (storageAccount == null) throw new ArgumentNullException(nameof(storageAccount));

            // File staging repo
            _diskRepo = diskRepo ?? throw new ArgumentNullException(nameof(diskRepo));

            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            client.DefaultRequestOptions.ParallelOperationThreadCount = 4; // Default is 1

            // Objects
            _objectsContainer = new Lazy<CloudBlobContainer>(() =>
            {
                const string container = "objects";
                CloudBlobContainer tr = client.GetContainerReference(container);

                tr.CreateIfNotExistsAsync().Wait();

                return tr;
            }, LazyThreadSafetyMode.PublicationOnly);

            // Refs
            _refsContainer = new Lazy<CloudBlobContainer>(() =>
            {
                const string container = "refs";
                CloudBlobContainer tr = client.GetContainerReference(container);

                tr.CreateIfNotExistsAsync().Wait();

                return tr;
            }, LazyThreadSafetyMode.PublicationOnly);
        }

        public static AzureBlobChasmRepo Create(string connectionString, DiskChasmRepo diskRepo)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (diskRepo == null) throw new ArgumentNullException(nameof(diskRepo));

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var repo = new AzureBlobChasmRepo(storageAccount, diskRepo);

            return repo;
        }
    }
}
