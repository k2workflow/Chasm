#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.IO.Compression;
using System.Threading;

namespace SourceCode.Chasm.IO.AzureTable
{
    public sealed partial class AzureTableChasmRepo : ChasmRepository
    {
        #region Fields

        private readonly Lazy<CloudTable> _refsTable;
        private readonly Lazy<CloudTable> _objectsTable;

        #endregion

        #region Constructors

        public AzureTableChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
            : base(serializer, compressionLevel, maxDop)
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

        public AzureTableChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer, CompressionLevel compressionLevel)
          : this(storageAccount, serializer, compressionLevel, -1)
        { }

        public AzureTableChasmRepo(CloudStorageAccount storageAccount, IChasmSerializer serializer)
            : this(storageAccount, serializer, CompressionLevel.Optimal)
        { }

        #endregion

        #region Factory

        public static AzureTableChasmRepo Create(string connectionString, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException(nameof(connectionString));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var repo = new AzureTableChasmRepo(storageAccount, serializer, compressionLevel, maxDop);

            return repo;
        }

        #endregion
    }
}
