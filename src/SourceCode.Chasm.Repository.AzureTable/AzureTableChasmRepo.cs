using System;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Chasm.Repository.Disk;

namespace SourceCode.Chasm.Repository.AzureTable
{
    public sealed partial class AzureTableChasmRepo : ChasmRepository
    {
        private readonly Lazy<CloudTable> _refsTable;
        private readonly Lazy<CloudTable> _objectsTable;
        private readonly DiskChasmRepo _diskRepo;

        public AzureTableChasmRepo(CloudStorageAccount storageAccount, DiskChasmRepo diskRepo)
            : base(diskRepo.Serializer)
        {
            if (storageAccount == null) throw new ArgumentNullException(nameof(storageAccount));

            // File staging repo
            _diskRepo = diskRepo ?? throw new ArgumentNullException(nameof(diskRepo));

            CloudTableClient client = storageAccount.CreateCloudTableClient();

            // Objects
            _objectsTable = new Lazy<CloudTable>(() =>
            {
                const string container = "objects";
                CloudTable tr = client.GetTableReference(container);

                tr.CreateIfNotExistsAsync().Wait();

                return tr;
            }, LazyThreadSafetyMode.PublicationOnly);

            // Refs
            _refsTable = new Lazy<CloudTable>(() =>
            {
                const string table = "refs";
                CloudTable tr = client.GetTableReference(table);

                tr.CreateIfNotExistsAsync().Wait();

                return tr;
            }, LazyThreadSafetyMode.PublicationOnly);
        }

        public static AzureTableChasmRepo Create(string connectionString, DiskChasmRepo diskRepo)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (diskRepo == null) throw new ArgumentNullException(nameof(diskRepo));

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var repo = new AzureTableChasmRepo(storageAccount, diskRepo);

            return repo;
        }
    }
}
