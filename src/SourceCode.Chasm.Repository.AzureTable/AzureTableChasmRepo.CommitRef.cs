#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SourceCode.Clay;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.AzureTable
{
    partial class AzureTableChasmRepo // .CommitRef
    {
        #region Read

        private static async ValueTask<(bool found, CommitId, string)> ReadCommitRefImplAsync(CloudTable refsTable, TableOperation operation, IChasmSerializer serializer, CancellationToken cancellationToken)
        {
            var commitId = CommitId.Empty;
            try
            {
                // Read from table
                var result = await refsTable.ExecuteAsync(operation, default, default, cancellationToken).ConfigureAwait(false);

                // NotFound
                if (result.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    return (false, default, default);

                var entity = (DataEntity)result.Result;

                // Sha1s are not compressed
                if (entity.Content?.Length > 0)
                {
                    var sha1 = serializer.DeserializeSha1(entity.Content);
                    commitId = new CommitId(sha1);
                }

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

        public async ValueTask<CommitRef> ReadCommitRefAsync(string branch, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refsTable = _refsTable.Value;
            var operation = DataEntity.BuildReadOperation(branch, name);

            var (_, commitId, _) = await ReadCommitRefImplAsync(refsTable, operation, Serializer, cancellationToken).ConfigureAwait(false);

            var commitRef = new CommitRef(name, commitId);
            return commitRef;
        }

        #endregion

        #region Write

        public async Task WriteCommitRefAsync(CommitId? previousCommitId, string branch, CommitRef commitRef, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (commitRef == CommitRef.Empty) throw new ArgumentNullException(nameof(commitRef));

            var refsTable = _refsTable.Value;
            var operation = DataEntity.BuildReadOperation(branch, commitRef.Name);

            // Load existing commit ref in order to use its etag
            var (found, existingCommitId, etag) = await ReadCommitRefImplAsync(refsTable, operation, Serializer, cancellationToken).ConfigureAwait(false);

            // Optimistic concurrency check
            if (found)
            {
                // Semantics follow Interlocked.Exchange (compare then exchange)
                if (previousCommitId.HasValue && existingCommitId != previousCommitId.Value)
                {
                    throw BuildConcurrencyException(branch, commitRef.Name, null);
                }

                // Idempotent
                if (existingCommitId == commitRef.CommitId)
                    return;
            }

            try
            {
                // Sha1s are not compressed
                using (var session = Serializer.Serialize(commitRef.CommitId.Sha1))
                {
                    var op = DataEntity.BuildWriteOperation(branch, commitRef.Name, session.Result, etag); // Note etag access condition

                    await refsTable.ExecuteAsync(op).ConfigureAwait(false);
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
            {
                throw BuildConcurrencyException(branch, commitRef.Name, se);
            }
        }

        #endregion

        #region Helpers

        private static ChasmConcurrencyException BuildConcurrencyException(string branch, string name, Exception innerException)
            => new ChasmConcurrencyException($"Concurrent write detected on {nameof(CommitRef)} {branch}/{name}", innerException);

        #endregion
    }
}
