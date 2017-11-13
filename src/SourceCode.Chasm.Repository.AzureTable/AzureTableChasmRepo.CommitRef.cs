#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Clay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.AzureTable
{
    partial class AzureTableChasmRepo // .CommitRef
    {
        #region List

        public override async ValueTask<IReadOnlyList<CommitRef>> GetBranchesAsync(string name, CancellationToken cancellationToken)
        {
            var refsTable = _refsTable.Value;
            var query = DataEntity.BuildListQuery(name);

            var results = new List<CommitRef>();

            TableContinuationToken token = null;
            do
            {
                var result = await refsTable.ExecuteQuerySegmentedAsync(query, token, default, default, cancellationToken).ConfigureAwait(false);
                foreach (var entity in result.Results)
                {
                    if (entity.RowKey.EndsWith(DataEntity.CommitSuffix, StringComparison.OrdinalIgnoreCase))
                    {
                        var branch = entity.RowKey.Substring(0, entity.RowKey.Length - DataEntity.CommitSuffix.Length);
                        var commitId = Serializer.DeserializeCommitId(entity.Content);
                        results.Add(new CommitRef(branch, commitId));
                    }
                }

                token = result.ContinuationToken;
            } while (token != null);

            return results;
        }

        public override async ValueTask<IReadOnlyList<string>> GetNamesAsync(CancellationToken cancellationToken)
        {
            var refsTable = _refsTable.Value;
            var query = DataEntity.BuildListQuery();

            var names = new HashSet<string>(StringComparer.Ordinal);

            TableContinuationToken token = null;
            do
            {
                var result = await refsTable.ExecuteQuerySegmentedAsync(query, token, default, default, cancellationToken).ConfigureAwait(false);
                foreach (var item in result.Results)
                    names.Add(item.PartitionKey);
                token = result.ContinuationToken;
            } while (token != null);

            return names.ToList();
        }

        #endregion

        #region Read

        public override async ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));

            var (found, commitId, _) = await ReadCommitRefImplAsync(name, branch, Serializer, cancellationToken).ConfigureAwait(false);

            // NotFound
            if (!found) return null;

            // Found
            var commitRef = new CommitRef(branch, commitId);
            return commitRef;
        }

        #endregion

        #region Write

        public override async Task WriteCommitRefAsync(CommitId? previousCommitId, string name, CommitRef commitRef, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (commitRef == CommitRef.Empty) throw new ArgumentNullException(nameof(commitRef));

            // Load existing commit ref in order to use its etag
            var (found, existingCommitId, etag) = await ReadCommitRefImplAsync(name, commitRef.Branch, Serializer, cancellationToken).ConfigureAwait(false);

            // Optimistic concurrency check
            if (found)
            {
                // We found a previous commit but the caller didn't say to expect one
                if (!previousCommitId.HasValue)
                    throw BuildConcurrencyException(name, commitRef.Branch, null); // TODO: May need a different error

                // Semantics follow Interlocked.Exchange (compare then exchange)
                if (previousCommitId.HasValue && existingCommitId != previousCommitId.Value)
                {
                    throw BuildConcurrencyException(name, commitRef.Branch, null);
                }

                // Idempotent
                if (existingCommitId == commitRef.CommitId)
                    return;
            }
            // The caller expected a previous commit, but we didn't find one
            else if (previousCommitId.HasValue)
            {
                throw BuildConcurrencyException(name, commitRef.Branch, null); // TODO: May need a different error
            }

            try
            {
                var refsTable = _refsTable.Value;

                // CommitIds are not compressed
                using (var session = Serializer.Serialize(commitRef.CommitId))
                {
                    var op = DataEntity.BuildWriteOperation(name, commitRef.Branch, session.Result, etag); // Note etag access condition

                    await refsTable.ExecuteAsync(op).ConfigureAwait(false);
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                throw BuildConcurrencyException(name, commitRef.Branch, se);
            }
        }

        #endregion

        #region Helpers

        private async ValueTask<(bool found, CommitId, string etag)> ReadCommitRefImplAsync(string name, string branch, IChasmSerializer serializer, CancellationToken cancellationToken)
        {
            var refsTable = _refsTable.Value;
            var operation = DataEntity.BuildReadOperation(name, branch);

            try
            {
                // Read from table
                var result = await refsTable.ExecuteAsync(operation, default, default, cancellationToken).ConfigureAwait(false);

                // NotFound
                if (result.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    return (false, default, default);

                var entity = (DataEntity)result.Result;

                var count = entity.Content?.Length ?? 0;
                if (count < Sha1.ByteLen)
                    throw new SerializationException($"{nameof(CommitRef)} '{name}/{branch}' expected to have byte length {Sha1.ByteLen} but has length {count}");

                // CommitIds are not compressed
                var commitId = serializer.DeserializeCommitId(entity.Content);

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

        #endregion
    }
}
