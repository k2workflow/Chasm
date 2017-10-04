using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Chasm.IO;
using System;
using System.IO.Compression;
using System.Threading;

namespace SourceCode.Chasm.Repository.AzureTable
{
    public sealed partial class AzureTableChasmRepo : IChasmRepository
    {
        #region Fields

        private readonly Lazy<CloudTable> _refsTable;
        private readonly Lazy<CloudTable> _objectsTable;

        #endregion

        #region Properties

        public IChasmSerializer Serializer { get; }

        public CompressionLevel CompressionLevel { get; }

        public int MaxDop { get; }

        #endregion

        #region Constructors

        public AzureTableChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
        {
            if (storageAccount == null) throw new ArgumentNullException(nameof(storageAccount));
            if (!Enum.IsDefined(typeof(CompressionLevel), compressionLevel)) throw new ArgumentOutOfRangeException(nameof(compressionLevel));
            if (maxDop < -1 || maxDop == 0) throw new ArgumentOutOfRangeException(nameof(maxDop));

            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            CompressionLevel = compressionLevel;
            MaxDop = maxDop;

            var client = storageAccount.CreateCloudTableClient();

            // Refs
            _refsTable = new Lazy<CloudTable>(() =>
            {
                const string table = "refs";
                var tr = client.GetTableReference(table);

                tr.CreateIfNotExistsAsync().Wait();

                return tr;
            }, LazyThreadSafetyMode.PublicationOnly);

            // Objects
            _objectsTable = new Lazy<CloudTable>(() =>
            {
                const string container = "objects";
                var tr = client.GetTableReference(container);

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

        #endregion

        #region Factory

        public static AzureTableChasmRepo Create(string connectionString, IChasmSerializer serializer, CompressionLevel compressionLevel)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException(nameof(connectionString));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var repo = new AzureTableChasmRepo(storageAccount, serializer, compressionLevel);

            return repo;
        }

        #endregion
    }
}
