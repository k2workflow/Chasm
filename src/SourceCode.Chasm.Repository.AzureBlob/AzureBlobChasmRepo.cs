using System;
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
        private readonly string _scratchPath;

        #endregion

        #region Constructors

        public AzureBlobChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer, int maxDop)
            : base(serializer, maxDop)
        {
            if (storageAccount == null) throw new ArgumentNullException(nameof(storageAccount));

            // Scratch area
            _scratchPath = System.IO.Path.GetTempPath();

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

        public AzureBlobChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer)
          : this(storageAccount, serializer, -1)
        { }

        #endregion

        #region Factory

        public static AzureBlobChasmRepo Create(string connectionString, IChasmSerializer serializer, int maxDop)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var repo = new AzureBlobChasmRepo(storageAccount, serializer, maxDop);

            return repo;
        }

        #endregion
    }
}
