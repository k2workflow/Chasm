using System;
using System.IO.Compression;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SourceCode.Chasm.Serializer;

namespace SourceCode.Chasm.Repository.AzureBlob
{
    public sealed partial class AzureBlobChasmRepo : ChasmRepository
    {
        #region Fields

        private readonly Lazy<CloudBlobContainer> _refsContainer;
        private readonly Lazy<CloudBlobContainer> _objectsContainer;

        #endregion

        #region Constructors

        public AzureBlobChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
            : base(serializer, compressionLevel, maxDop)
        {
            if (storageAccount == null) throw new ArgumentNullException(nameof(storageAccount));

            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            client.DefaultRequestOptions.ParallelOperationThreadCount = 4; // Default is 1

            // Refs
            _refsContainer = new Lazy<CloudBlobContainer>(() =>
            {
                const string container = "refs";
                CloudBlobContainer tr = client.GetContainerReference(container);

                tr.CreateIfNotExistsAsync().Wait();

                return tr;
            }, LazyThreadSafetyMode.PublicationOnly);

            // Objects
            _objectsContainer = new Lazy<CloudBlobContainer>(() =>
            {
                const string container = "objects";
                CloudBlobContainer tr = client.GetContainerReference(container);

                tr.CreateIfNotExistsAsync().Wait();

                return tr;
            }, LazyThreadSafetyMode.PublicationOnly);
        }

        public AzureBlobChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer, CompressionLevel compressionLevel)
          : this(storageAccount, serializer, compressionLevel, -1)
        { }

        public AzureBlobChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer)
            : this(storageAccount, serializer, CompressionLevel.Optimal)
        { }

        #endregion

        #region Factory

        public static AzureBlobChasmRepo Create(string connectionString, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var repo = new AzureBlobChasmRepo(storageAccount, serializer, compressionLevel, maxDop);

            return repo;
        }

        #endregion
    }
}
