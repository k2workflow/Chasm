using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Chasm.Serializer;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository.AzureTable
{
    partial class AzureTableChasmRepo // .CommitRef
    {
        public override async ValueTask<IReadOnlyList<CommitRef>> GetBranchesAsync(string name, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            requestContext = ChasmRequestContext.Ensure(requestContext);
            var opContext = new OperationContext
            {
                ClientRequestID = requestContext.CorrelationId,
                CustomUserAgent = requestContext.CustomUserAgent
            };

            CloudTable refsTable = _refsTable.Value;
            TableQuery<DataEntity> query = DataEntity.BuildListQuery(name);

            var results = new List<CommitRef>();

            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<DataEntity> result = await refsTable.ExecuteQuerySegmentedAsync(query, token, default, opContext, cancellationToken)
                    .ConfigureAwait(false);

                foreach (DataEntity entity in result.Results)
                {
                    if (entity.RowKey.EndsWith(DataEntity.CommitSuffix, StringComparison.OrdinalIgnoreCase))
                    {
                        string branch = entity.RowKey.Substring(0, entity.RowKey.Length - DataEntity.CommitSuffix.Length);
                        CommitId commitId = Serializer.DeserializeCommitId(entity.Content);
                        results.Add(new CommitRef(branch, commitId));
                    }
                }

                token = result.ContinuationToken;
            } while (token != null);

            return results;
        }

        public override async ValueTask<IReadOnlyList<string>> GetNamesAsync(ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            requestContext = ChasmRequestContext.Ensure(requestContext);
            var opContext = new OperationContext
            {
                ClientRequestID = requestContext.CorrelationId,
                CustomUserAgent = requestContext.CustomUserAgent
            };

            CloudTable refsTable = _refsTable.Value;
            TableQuery<DataEntity> query = DataEntity.BuildListQuery();

            var names = new HashSet<string>(StringComparer.Ordinal);

            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<DataEntity> result = await refsTable.ExecuteQuerySegmentedAsync(query, token, default, opContext, cancellationToken)
                    .ConfigureAwait(false);

                foreach (DataEntity item in result.Results)
                    names.Add(item.PartitionKey);
                token = result.ContinuationToken;
            } while (token != null);

            return names.ToList();
        }

        public override async ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));

            requestContext = ChasmRequestContext.Ensure(requestContext);

            (bool found, CommitId commitId, string _) = await ReadCommitRefImplAsync(name, branch, Serializer, requestContext, cancellationToken)
                .ConfigureAwait(false);

            // NotFound
            if (!found) return null;

            // Found
            var commitRef = new CommitRef(branch, commitId);
            return commitRef;
        }

        public override async Task WriteCommitRefAsync(CommitId? previousCommitId, string name, CommitRef commitRef, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (commitRef == CommitRef.Empty) throw new ArgumentNullException(nameof(commitRef));

            requestContext = ChasmRequestContext.Ensure(requestContext);
            var opContext = new OperationContext
            {
                ClientRequestID = requestContext.CorrelationId,
                CustomUserAgent = requestContext.CustomUserAgent
            };

            // Load existing commit ref in order to use its etag
            (bool found, CommitId existingCommitId, string etag) = await ReadCommitRefImplAsync(name, commitRef.Branch, Serializer, requestContext, cancellationToken)
                .ConfigureAwait(false);

            // Optimistic concurrency check
            if (found)
            {
                // We found a previous commit but the caller didn't say to expect one
                if (!previousCommitId.HasValue)
                    throw BuildConcurrencyException(name, commitRef.Branch, null, requestContext); // TODO: May need a different error

                // Semantics follow Interlocked.Exchange (compare then exchange)
                if (previousCommitId.HasValue && existingCommitId != previousCommitId.Value)
                {
                    throw BuildConcurrencyException(name, commitRef.Branch, null, requestContext);
                }

                // Idempotent
                if (existingCommitId == commitRef.CommitId)
                    return;
            }

            // The caller expected a previous commit, but we didn't find one
            else if (previousCommitId.HasValue)
            {
                throw BuildConcurrencyException(name, commitRef.Branch, null, requestContext); // TODO: May need a different error
            }

            try
            {
                CloudTable refsTable = _refsTable.Value;

                using (IMemoryOwner<byte> owner = Serializer.Serialize(commitRef.CommitId))
                {
                    var blob = new ChasmBlob(owner.Memory, null);
                    TableOperation op = DataEntity.BuildWriteOperation(name, commitRef.Branch, blob, etag); // Note etag access condition

                    await refsTable.ExecuteAsync(op, default, opContext, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                throw BuildConcurrencyException(name, commitRef.Branch, se, requestContext);
            }
        }

        private async ValueTask<(bool found, CommitId, string etag)> ReadCommitRefImplAsync(string name, string branch, IChasmSerializer serializer, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            requestContext = ChasmRequestContext.Ensure(requestContext);
            var opContext = new OperationContext
            {
                ClientRequestID = requestContext.CorrelationId,
                CustomUserAgent = requestContext.CustomUserAgent
            };

            CloudTable refsTable = _refsTable.Value;
            TableOperation operation = DataEntity.BuildReadOperation(name, branch);

            try
            {
                // Read from table
                TableResult result = await refsTable.ExecuteAsync(operation, default, opContext, cancellationToken)
                    .ConfigureAwait(false);

                // NotFound
                if (result.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    return (false, default, default);

                var entity = (DataEntity)result.Result;

                int count = entity.Content?.Length ?? 0;
                if (count < Sha1.ByteLength)
                    throw new SerializationException($"{nameof(CommitRef)} '{name}/{branch}' expected to have byte length {Sha1.ByteLength} but has length {count}");

                CommitId commitId = serializer.DeserializeCommitId(entity.Content);

                // Found
                return (true, commitId, result.Etag);
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                // Try-catch is cheaper than a separate (latent) exists check
                se.Suppress();
            }

            // NotFound
            return (false, default, default);
        }
    }
}
