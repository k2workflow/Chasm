using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Chasm.Serializer;
using System;
using System.IO.Compression;
using System.Threading;

namespace SourceCode.Chasm.Repository.AzureTable
{
    public sealed partial class AzureTableChasmRepo : ChasmRepository
    {
        private readonly Lazy<CloudTable> _refsTable;
        private readonly Lazy<CloudTable> _objectsTable;

        public AzureTableChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
            : base(serializer, compressionLevel, maxDop)
        {
            if (storageAccount == null) throw new ArgumentNullException(nameof(storageAccount));

            CloudTableClient client = storageAccount.CreateCloudTableClient();

            // Refs
            _refsTable = new Lazy<CloudTable>(() =>
            {
                const string table = "refs";
                CloudTable tr = client.GetTableReference(table);

                tr.CreateIfNotExistsAsync().Wait();

                return tr;
            }, LazyThreadSafetyMode.PublicationOnly);

            // Objects
            _objectsTable = new Lazy<CloudTable>(() =>
            {
                const string container = "objects";
                CloudTable tr = client.GetTableReference(container);

                tr.CreateIfNotExistsAsync().Wait();

                return tr;
            }, LazyThreadSafetyMode.PublicationOnly);
        }

        public AzureTableChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer, CompressionLevel compressionLevel)
          : this(storageAccount, serializer, compressionLevel, -1)
        { }

        public AzureTableChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer)
            : this(storageAccount, serializer, CompressionLevel.Optimal)
        { }

        public static AzureTableChasmRepo Create(string connectionString, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var repo = new AzureTableChasmRepo(storageAccount, serializer, compressionLevel, maxDop);

            return repo;
        }
    }
}
