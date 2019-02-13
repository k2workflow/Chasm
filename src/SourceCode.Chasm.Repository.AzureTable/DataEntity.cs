using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository.AzureTable
{
    public sealed class DataEntity : TableEntity
    {
        #region Fields

        internal const string CommitSuffix = "-commit";

        #endregion

        #region Properties

        // This is an EF dto, which requires such patterns
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Content { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        public string Filename { get; set; }

        public string ContentType { get; set; }

        #endregion

        #region Constructors

        private DataEntity(string partitionKey, string rowKey, IChasmBlob blob)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;

            Content = blob?.Content.ToArray(); // TODO: Perf
            Filename = blob?.Metadata?.Filename;
            ContentType = blob?.Metadata?.ContentType;
        }

        private DataEntity(string partitionKey, string rowKey, IChasmBlob blob, string etag)
            : this(partitionKey, rowKey, blob)
        {
            // https://azure.microsoft.com/en-us/blog/managing-concurrency-in-microsoft-azure-storage-2/
            ETag = etag;
        }

        private DataEntity(string partitionKey, string rowKey, IChasmBlob blob, bool forceOverwrite)
            : this(partitionKey, rowKey, blob)
        {
            if (forceOverwrite)
            {
                // https://azure.microsoft.com/en-us/blog/managing-concurrency-in-microsoft-azure-storage-2/
                // "To explicitly disable the concurrency check, you should set the ETag property of the ... object to “*” before you execute the replace operation"
                ETag = "*";
            }
        }

        public DataEntity()
        { }

        #endregion

        #region Factory

        internal static DataEntity Create(Sha1 sha1, IChasmBlob blob, string etag)
        {
            if (blob == null) throw new ArgumentNullException(nameof(blob));

            KeyValuePair<string, string> split = GetPartition(sha1);

            var entity = new DataEntity(split.Key, split.Value, blob, etag);
            return entity;
        }

        internal static DataEntity Create(Sha1 sha1, IChasmBlob blob, bool forceOverwrite)
        {
            if (blob == null) throw new ArgumentNullException(nameof(blob));

            KeyValuePair<string, string> split = GetPartition(sha1);

            var entity = new DataEntity(split.Key, split.Value, blob, forceOverwrite);
            return entity;
        }

        internal static DataEntity Create(Sha1 sha1, IChasmBlob blob)
        {
            if (blob == null) throw new ArgumentNullException(nameof(blob));

            KeyValuePair<string, string> split = GetPartition(sha1);

            var entity = new DataEntity(split.Key, split.Value, blob);
            return entity;
        }

        internal static DataEntity Create(string name, string branch, IChasmBlob blob, string etag)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (blob == null) throw new ArgumentNullException(nameof(blob));

            string partitionKey = GetPartitionKey(name);
            string rowKey = GetRowKey(branch);

            var entity = new DataEntity(partitionKey, rowKey, blob, etag);
            return entity;
        }

        internal static DataEntity Create(string name, string branch, IChasmBlob blob)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (blob == null) throw new ArgumentNullException(nameof(blob));

            string partitionKey = GetPartitionKey(name);
            string rowKey = GetRowKey(branch);

            var entity = new DataEntity(partitionKey, rowKey, blob);
            return entity;
        }

        #endregion

        #region Exists

        private static readonly List<string> s_keys = new List<string> { nameof(ITableEntity.PartitionKey), nameof(ITableEntity.RowKey) };

        internal static TableOperation BuildExistsOperation(Sha1 sha1)
        {
            string partitionKey = GetPartitionKey(sha1);
            string rowKey = GetRowKey(sha1);

            var op = TableOperation.Retrieve<DataEntity>(partitionKey, rowKey, s_keys);
            return op;
        }

        #endregion

        #region Read

        internal static TableOperation BuildReadOperation(Sha1 sha1)
        {
            string partitionKey = GetPartitionKey(sha1);
            string rowKey = GetRowKey(sha1);

            var op = TableOperation.Retrieve<DataEntity>(partitionKey, rowKey);
            return op;
        }

        internal static TableQuery<DataEntity> BuildListQuery()
        {
            var query = new TableQuery<DataEntity>()
            {
                SelectColumns = new[] { "PartitionKey" }
            };

            return query;
        }

        internal static TableQuery<DataEntity> BuildListQuery(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            TableQuery<DataEntity> query = new TableQuery<DataEntity>()
            {
                SelectColumns = new[] { "RowKey", "Content" }
            }.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, name));

            return query;
        }

        internal static TableOperation BuildReadOperation(string name, string branch)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));

            string partitionKey = GetPartitionKey(name);
            string rowKey = GetRowKey(branch);

            var op = TableOperation.Retrieve<DataEntity>(partitionKey, rowKey);
            return op;
        }

        internal static TableBatchOperation BuildReadOperation(IReadOnlyList<Sha1> sha1s)
        {
            var batch = new TableBatchOperation();

            if (sha1s != null && sha1s.Count > 0)
            {
                for (int i = 0; i < sha1s.Count; i++)
                {
                    string partitionKey = GetPartitionKey(sha1s[i]);
                    string rowKey = GetRowKey(sha1s[i]);

                    batch.Retrieve<DataEntity>(partitionKey, rowKey);
                }
            }

            return batch;
        }

        internal static IReadOnlyCollection<TableBatchOperation> BuildReadBatches(IEnumerable<Sha1> sha1s)
        {
            if (sha1s == null || !sha1s.Any()) return Array.Empty<TableBatchOperation>();

            var batches = new Dictionary<string, TableBatchOperation>(StringComparer.Ordinal);

            foreach (Sha1 sha1 in sha1s)
            {
                KeyValuePair<string, string> split = GetPartition(sha1);

                if (!batches.TryGetValue(split.Key, out TableBatchOperation batch))
                {
                    batch = new TableBatchOperation();
                    batches.Add(split.Key, batch);
                }

                var op = TableOperation.Retrieve<DataEntity>(split.Key, split.Value);
                batch.Add(op);
            }

            return batches.Values;
        }

        #endregion

        #region Write

        internal static TableOperation BuildWriteOperation(Sha1 sha1, IChasmBlob blob, bool forceOverwrite)
        {
            if (blob == null) throw new ArgumentNullException(nameof(blob));

            DataEntity entity = Create(sha1, blob, forceOverwrite);

            var op = TableOperation.InsertOrReplace(entity);
            return op;
        }

        internal static TableOperation BuildWriteOperation(Sha1 sha1, IChasmBlob blob, string etag)
        {
            if (blob == null) throw new ArgumentNullException(nameof(blob));

            DataEntity entity = Create(sha1, blob, etag);

            var op = TableOperation.InsertOrReplace(entity);
            return op;
        }

        internal static TableOperation BuildWriteOperation(Sha1 sha1, IChasmBlob blob)
        {
            if (blob == null) throw new ArgumentNullException(nameof(blob));

            DataEntity entity = Create(sha1, blob);

            var op = TableOperation.InsertOrReplace(entity);
            return op;
        }

        internal static TableOperation BuildWriteOperation(string name, string branch, IChasmBlob blob, string etag)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (blob == null) throw new ArgumentNullException(nameof(blob));

            DataEntity entity = Create(name, branch, blob, etag);

            var op = TableOperation.InsertOrReplace(entity);
            return op;
        }

        internal static TableOperation BuildWriteOperation(string name, string branch, IChasmBlob blob)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (blob == null) throw new ArgumentNullException(nameof(blob));

            DataEntity entity = Create(name, branch, blob);

            var op = TableOperation.InsertOrReplace(entity);
            return op;
        }

        internal static IReadOnlyCollection<TableBatchOperation> BuildWriteBatches(IEnumerable<KeyValuePair<Sha1, IChasmBlob>> blobs, bool forceOverwrite)
        {
            if (blobs == null || !blobs.Any())
                return Array.Empty<TableBatchOperation>();

            // TODO: Limit number of items in each TableBatchOperation

            var batches = new Dictionary<string, TableBatchOperation>(StringComparer.Ordinal);

            foreach (KeyValuePair<Sha1, IChasmBlob> blob in blobs)
            {
                DataEntity entity = Create(blob.Key, blob.Value, forceOverwrite);

                if (!batches.TryGetValue(entity.PartitionKey, out TableBatchOperation batch))
                {
                    batch = new TableBatchOperation();
                    batches.Add(entity.PartitionKey, batch);
                }

                var op = TableOperation.InsertOrReplace(entity);
                batch.Add(op);
            }

            return batches.Values;
        }

        #endregion

        #region Delete

        internal static TableOperation BuildDeleteOperation(Sha1 sha1)
        {
            KeyValuePair<string, string> split = GetPartition(sha1);

            var entity = new DataEntity
            {
                PartitionKey = split.Key,
                RowKey = split.Value,
                ETag = "*" // No need to fetch the entity first
            };

            var op = TableOperation.Delete(entity);
            return op;
        }

        internal static TableBatchOperation BuildDeleteOperation(IReadOnlyList<Sha1> sha1s)
        {
            if (sha1s == null) throw new ArgumentNullException(nameof(sha1s));

            var batch = new TableBatchOperation();

            for (int i = 0; i < sha1s.Count; i++)
            {
                TableOperation operation = BuildDeleteOperation(sha1s[i]);
                batch.Add(operation);
            }

            return batch;
        }

        #endregion

        #region Helpers

        private static KeyValuePair<string, string> GetPartition(Sha1 sha1) => sha1.Split(2);

        private static string GetPartitionKey(Sha1 sha1) => GetPartition(sha1).Key;

        private static string GetRowKey(Sha1 sha1) => GetPartition(sha1).Value;

        private static string GetPartitionKey(string branch) => branch.ToUpperInvariant();

        private static string GetRowKey(string name) => $"{name}-commit";

        public static Sha1 FromPartition(ITableEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            string str = $"{entity.PartitionKey}{entity.RowKey}";
            var sha1 = Sha1.Parse(str);

            return sha1;
        }

        #endregion
    }
}
