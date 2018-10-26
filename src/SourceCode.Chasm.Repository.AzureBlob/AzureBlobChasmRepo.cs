using System;
using System.IO.Compression;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SourceCode.Chasm.Serializer;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.Repository.AzureBlob
{
    public sealed partial class AzureBlobChasmRepo : ChasmRepository
    {
        #region Fields

        private readonly Lazy<CloudBlobContainer> _refsContainer;
        private readonly Lazy<CloudBlobContainer> _objectsContainer;

        #endregion

        #region Constructors

        public AzureBlobChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop, crypt.SHA1 hasher)
            : base(serializer, compressionLevel, maxDop, hasher)
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

        public AzureBlobChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer, CompressionLevel compressionLevel, crypt.SHA1 hasher)
          : this(storageAccount, serializer, compressionLevel, -1, hasher)
        { }

        public AzureBlobChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer, crypt.SHA1 hasher)
            : this(storageAccount, serializer, CompressionLevel.Optimal, hasher)
        { }

        #endregion

        #region Factory

        public static AzureBlobChasmRepo Create(string connectionString, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop, crypt.SHA1 hasher)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var repo = new AzureBlobChasmRepo(storageAccount, serializer, compressionLevel, maxDop, hasher);

            return repo;
        }

        #endregion
    }
}
