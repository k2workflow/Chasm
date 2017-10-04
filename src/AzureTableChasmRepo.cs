using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.IO.Compression;
using System.Threading;

namespace SourceCode.Chasm.IO.AzureTableStorage
{
    public sealed partial class AzureTableChasmRepo : ChasmRepository
    {
        #region Fields

        private readonly Lazy<CloudTable> _refsTable;
        private readonly Lazy<CloudTable> _objectsTable;

        #endregion

        #region Constructors

        public AzureTableChasmRepo(ChasmSerializer serializer, CloudStorageAccount storageAccount, CompressionLevel compressionLevel)
            : base(serializer, compressionLevel)
        {
            if (storageAccount == null) throw new ArgumentNullException(nameof(storageAccount));

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

        #endregion

        #region Factory

        public static AzureTableChasmRepo Create(ChasmSerializer serializer, string connectionString, CompressionLevel compressionLevel)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException(nameof(connectionString));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var repo = new AzureTableChasmRepo(serializer, storageAccount, compressionLevel);

            return repo;
        }

        #endregion

        #region Helpers

        private static ChasmConcurrencyException BuildConcurrencyException(string branch, string name, Exception innerException)
            => new ChasmConcurrencyException($"Concurrent write detected on {nameof(CommitRef)} {branch}/{name}", innerException);

        #endregion
    }
}
