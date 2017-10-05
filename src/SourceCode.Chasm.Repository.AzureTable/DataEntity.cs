using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SourceCode.Chasm.IO.AzureTable
{
    public sealed class DataEntity : TableEntity
    {
        #region Properties

        public byte[] Content { get; set; }

        public string ObjectType { get; set; }

        #endregion

        #region Constructors

        public DataEntity()
        { }

        private DataEntity(string partitionKey, string rowKey, ArraySegment<byte> segment)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Content = segment.ToArray(); // TODO: Perf
        }

        private DataEntity(string partitionKey, string rowKey, ArraySegment<byte> segment, string etag)
            : this(partitionKey, rowKey, segment)
        {
            // https://azure.microsoft.com/en-us/blog/managing-concurrency-in-microsoft-azure-storage-2/
            ETag = etag;
        }

        #endregion

        #region Factory

        internal static DataEntity Create(Sha1 sha1, ArraySegment<byte> segment, string etag)
        {
            if (sha1 == Sha1.Empty) throw new ArgumentNullException(nameof(sha1));

            var split = GetPartition(sha1);

            var entity = new DataEntity(split.Key, split.Value, segment, etag);
            return entity;
        }

        internal static DataEntity Create(Sha1 sha1, ArraySegment<byte> segment)
        {
            if (sha1 == Sha1.Empty) throw new ArgumentNullException(nameof(sha1));

            var split = GetPartition(sha1);

            var entity = new DataEntity(split.Key, split.Value, segment);
            return entity;
        }

        internal static DataEntity Create(string repo, string name, ArraySegment<byte> segment, string etag)
        {
            if (string.IsNullOrWhiteSpace(repo)) throw new ArgumentNullException(nameof(repo));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var partitionKey = GetPartitionKey(repo);
            var rowKey = GetRowKey(name);

            var entity = new DataEntity(partitionKey, rowKey, segment, etag);
            return entity;
        }

        internal static DataEntity Create(string repo, string name, ArraySegment<byte> segment)
        {
            if (string.IsNullOrWhiteSpace(repo)) throw new ArgumentNullException(nameof(repo));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var partitionKey = GetPartitionKey(repo);
            var rowKey = GetRowKey(name);

            var entity = new DataEntity(partitionKey, rowKey, segment);
            return entity;
        }

        #endregion

        #region Methods

        private static readonly List<string> _cols = new List<string> { "PartitionKey", "RowKey" };

        internal static TableOperation BuildExistsOperation(Sha1 sha1)
        {
            if (sha1 == Sha1.Empty) throw new ArgumentNullException(nameof(sha1));

            var partitionKey = GetPartitionKey(sha1);
            var rowKey = GetRowKey(sha1);

            var op = TableOperation.Retrieve<DataEntity>(partitionKey, rowKey, _cols);
            return op;
        }

        internal static TableOperation BuildReadOperation(Sha1 sha1)
        {
            if (sha1 == Sha1.Empty) throw new ArgumentNullException(nameof(sha1));

            var partitionKey = GetPartitionKey(sha1);
            var rowKey = GetRowKey(sha1);

            var op = TableOperation.Retrieve<DataEntity>(partitionKey, rowKey);
            return op;
        }

        internal static TableOperation BuildReadOperation(string repo, string name)
        {
            if (string.IsNullOrWhiteSpace(repo)) throw new ArgumentNullException(nameof(repo));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var partitionKey = GetPartitionKey(repo);
            var rowKey = GetRowKey(name);

            var op = TableOperation.Retrieve<DataEntity>(partitionKey, rowKey);
            return op;
        }

        internal static TableBatchOperation BuildReadOperation(IReadOnlyList<Sha1> sha1s)
        {
            var batch = new TableBatchOperation();

            if (sha1s != null && sha1s.Count > 0)
            {
                for (var i = 0; i < sha1s.Count; i++)
                {
                    var partitionKey = GetPartitionKey(sha1s[i]);
                    var rowKey = GetRowKey(sha1s[i]);

                    batch.Retrieve<DataEntity>(partitionKey, rowKey);
                }
            }

            return batch;
        }

        internal static IReadOnlyCollection<TableBatchOperation> BuildBatchReadOperation(IEnumerable<Sha1> sha1s)
        {
            if (sha1s == null) return Array.Empty<TableBatchOperation>();

            var batches = new Dictionary<string, TableBatchOperation>(StringComparer.Ordinal);

            foreach (var sha1 in sha1s)
            {
                var split = GetPartition(sha1);

                if (!batches.TryGetValue(split.Key, out var batch))
                {
                    batch = new TableBatchOperation();
                    batches.Add(split.Key, batch);
                }

                var op = TableOperation.Retrieve<DataEntity>(split.Key, split.Value);
                batch.Add(op);
            }

            return batches.Values;
        }

        internal static TableOperation BuildWriteOperation(Sha1 sha1, ArraySegment<byte> segment, string etag)
        {
            if (sha1 == Sha1.Empty) throw new ArgumentNullException(nameof(sha1));

            var entity = Create(sha1, segment, etag);

            var op = TableOperation.InsertOrReplace(entity);
            return op;
        }

        internal static TableOperation BuildWriteOperation(Sha1 sha1, ArraySegment<byte> segment)
        {
            if (sha1 == Sha1.Empty) throw new ArgumentNullException(nameof(sha1));

            var entity = Create(sha1, segment);

            var op = TableOperation.InsertOrReplace(entity);
            return op;
        }

        internal static TableOperation BuildWriteOperation(string repo, string name, ArraySegment<byte> segment, string etag)
        {
            if (string.IsNullOrWhiteSpace(repo)) throw new ArgumentNullException(nameof(repo));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var entity = Create(repo, name, segment, etag);

            var op = TableOperation.InsertOrReplace(entity);
            return op;
        }

        internal static TableOperation BuildWriteOperation(string repo, string name, ArraySegment<byte> segment)
        {
            if (string.IsNullOrWhiteSpace(repo)) throw new ArgumentNullException(nameof(repo));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var entity = Create(repo, name, segment);

            var op = TableOperation.InsertOrReplace(entity);
            return op;
        }

        internal static void BuildBatchWriteOperation(Dictionary<string, TableBatchOperation> batches, IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items)
        {
            if (batches == null) throw new ArgumentNullException(nameof(batches));

            if (items == null) return;

            foreach (var item in items)
            {
                var entity = Create(item.Key, item.Value);

                if (!batches.TryGetValue(entity.PartitionKey, out var batch))
                {
                    batch = new TableBatchOperation();
                    batches.Add(entity.PartitionKey, batch);
                }

                var op = TableOperation.InsertOrReplace(entity);
                batch.Add(op);
            }
        }

        internal static TableOperation BuildDeleteOperation(Sha1 sha1)
        {
            if (sha1 == Sha1.Empty) throw new ArgumentNullException(nameof(sha1));

            var split = GetPartition(sha1);

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

            for (var i = 0; i < sha1s.Count; i++)
            {
                var operation = BuildDeleteOperation(sha1s[i]);
                batch.Add(operation);
            }

            return batch;
        }

        #endregion

        #region Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static KeyValuePair<string, string> GetPartition(Sha1 sha1) => sha1.Split(2);

        private static string GetPartitionKey(Sha1 sha1) => GetPartition(sha1).Key;

        private static string GetRowKey(Sha1 sha1) => GetPartition(sha1).Value;

        private static string GetPartitionKey(string repo) => repo.ToLowerInvariant();

        private static string GetRowKey(string name) => $"{name}-commit";

        public static Sha1 FromPartition(DataEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var str = $"{entity.PartitionKey}{entity.RowKey}";
            var sha1 = Sha1.Parse(str);

            return sha1;
        }

        #endregion
    }
}
