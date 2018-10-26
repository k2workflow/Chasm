using System;
using System.IO.Compression;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Chasm.Serializer;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.Repository.AzureTable
{
    public sealed partial class AzureTableChasmRepo : ChasmRepository
    {
        private readonly Lazy<CloudTable> _refsTable;
        private readonly Lazy<CloudTable> _objectsTable;

        public AzureTableChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop, crypt.SHA1 hasher)
            : base(serializer, compressionLevel, maxDop, hasher)
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

        public AzureTableChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer, CompressionLevel compressionLevel, crypt.SHA1 hasher)
          : this(storageAccount, serializer, compressionLevel, -1, hasher)
        { }

        public AzureTableChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer, crypt.SHA1 hasher)
            : this(storageAccount, serializer, CompressionLevel.Optimal, hasher)
        { }

        public static AzureTableChasmRepo Create(string connectionString, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop, crypt.SHA1 hasher)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var repo = new AzureTableChasmRepo(storageAccount, serializer, compressionLevel, maxDop, hasher);

            return repo;
        }
    }
}
