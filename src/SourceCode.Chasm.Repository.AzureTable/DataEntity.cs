#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Clay;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SourceCode.Chasm.IO.AzureTable
{
    public sealed class DataEntity : TableEntity
    {
        #region Fields

        internal const string CommitSuffix = "-commit";

        #endregion

        #region Properties

        public byte[] Content { get; set; }

        public string ObjectType { get; set; }

        #endregion

        #region Constructors

        private DataEntity(string partitionKey, string rowKey, ArraySegment<byte> content)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Content = content.ToArray(); // TODO: Perf
        }

        private DataEntity(string partitionKey, string rowKey, ArraySegment<byte> content, string etag)
            : this(partitionKey, rowKey, content)
        {
            // https://azure.microsoft.com/en-us/blog/managing-concurrency-in-microsoft-azure-storage-2/
            ETag = etag;
        }

        private DataEntity(string partitionKey, string rowKey, ArraySegment<byte> content, bool forceOverwrite)
            : this(partitionKey, rowKey, content)
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

        internal static DataEntity Create(Sha1 sha1, ArraySegment<byte> content, string etag)
        {
            var split = GetPartition(sha1);

            var entity = new DataEntity(split.Key, split.Value, content, etag);
            return entity;
        }

        internal static DataEntity Create(Sha1 sha1, ArraySegment<byte> content, bool forceOverwrite)
        {
            var split = GetPartition(sha1);

            var entity = new DataEntity(split.Key, split.Value, content, forceOverwrite);
            return entity;
        }

        internal static DataEntity Create(Sha1 sha1, ArraySegment<byte> content)
        {
            var split = GetPartition(sha1);

            var entity = new DataEntity(split.Key, split.Value, content);
            return entity;
        }

        internal static DataEntity Create(string name, string branch, ArraySegment<byte> content, string etag)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));

            var partitionKey = GetPartitionKey(name);
            var rowKey = GetRowKey(branch);

            var entity = new DataEntity(partitionKey, rowKey, content, etag);
            return entity;
        }

        internal static DataEntity Create(string name, string branch, ArraySegment<byte> content)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));

            var partitionKey = GetPartitionKey(name);
            var rowKey = GetRowKey(branch);

            var entity = new DataEntity(partitionKey, rowKey, content);
            return entity;
        }

        #endregion

        #region Exists

        private static readonly List<string> _cols = new List<string> { nameof(ITableEntity.PartitionKey), nameof(ITableEntity.RowKey) };

        internal static TableOperation BuildExistsOperation(Sha1 sha1)
        {
            var partitionKey = GetPartitionKey(sha1);
            var rowKey = GetRowKey(sha1);

            var op = TableOperation.Retrieve<DataEntity>(partitionKey, rowKey, _cols);
            return op;
        }

        #endregion

        #region Read

        internal static TableOperation BuildReadOperation(Sha1 sha1)
        {
            var partitionKey = GetPartitionKey(sha1);
            var rowKey = GetRowKey(sha1);

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

            var query = new TableQuery<DataEntity>()
            {
                SelectColumns = new[] { "RowKey", "Content" }
            }.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, name));

            return query;
        }

        internal static TableOperation BuildReadOperation(string name, string branch)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));

            var partitionKey = GetPartitionKey(name);
            var rowKey = GetRowKey(branch);

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

        internal static IReadOnlyCollection<TableBatchOperation> BuildReadBatches(IEnumerable<Sha1> sha1s)
        {
            if (sha1s == null || !sha1s.Any()) return Array.Empty<TableBatchOperation>();

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

        #endregion

        #region Write

        internal static TableOperation BuildWriteOperation(Sha1 sha1, ArraySegment<byte> content, bool forceOverwrite)
        {
            var entity = Create(sha1, content, forceOverwrite);

            var op = TableOperation.InsertOrReplace(entity);
            return op;
        }

        internal static TableOperation BuildWriteOperation(Sha1 sha1, ArraySegment<byte> content, string etag)
        {
            var entity = Create(sha1, content, etag);

            var op = TableOperation.InsertOrReplace(entity);
            return op;
        }

        internal static TableOperation BuildWriteOperation(Sha1 sha1, ArraySegment<byte> content)
        {
            var entity = Create(sha1, content);

            var op = TableOperation.InsertOrReplace(entity);
            return op;
        }

        internal static TableOperation BuildWriteOperation(string name, string branch, ArraySegment<byte> content, string etag)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));

            var entity = Create(name, branch, content, etag);

            var op = TableOperation.InsertOrReplace(entity);
            return op;
        }

        internal static TableOperation BuildWriteOperation(string name, string branch, ArraySegment<byte> content)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));

            var entity = Create(name, branch, content);

            var op = TableOperation.InsertOrReplace(entity);
            return op;
        }

        internal static IReadOnlyCollection<TableBatchOperation> BuildWriteBatches(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> contents, bool forceOverwrite)
        {
            if (contents == null || !contents.Any()) return Array.Empty<TableBatchOperation>();

            // TODO: Limit number of items in each TableBatchOperation

            var batches = new Dictionary<string, TableBatchOperation>(StringComparer.Ordinal);

            foreach (var content in contents)
            {
                var entity = Create(content.Key, content.Value, forceOverwrite);

                if (!batches.TryGetValue(entity.PartitionKey, out var batch))
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

        private static KeyValuePair<string, string> GetPartition(Sha1 sha1) => sha1.Split(2);

        private static string GetPartitionKey(Sha1 sha1) => GetPartition(sha1).Key;

        private static string GetRowKey(Sha1 sha1) => GetPartition(sha1).Value;

        private static string GetPartitionKey(string branch) => branch.ToLowerInvariant();

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
