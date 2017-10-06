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

        private static async ValueTask<(CommitId, string)> ReadCommitRefImplAsync(CloudTable refsTable, TableOperation operation, IChasmSerializer serializer, CancellationToken cancellationToken)
        {
            var commitId = CommitId.Empty;
            string etag = null;
            try
            {
                // Read from table
                var result = await refsTable.ExecuteAsync(operation, default, default, cancellationToken).ConfigureAwait(false);
                etag = result.Etag;

                if (result.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    return (commitId, etag);

                var entity = (DataEntity)result.Result;

                // CommitRefs are not compressed

                var sha1 = serializer.DeserializeSha1(entity.Content);
                commitId = new CommitId(sha1);
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                // Try-catch is cheaper than a separate exists check
                se.Suppress();
            }

            return (commitId, etag);
        }

        public async ValueTask<CommitId> ReadCommitRefAsync(string branch, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refsTable = _refsTable.Value;
            var operation = DataEntity.BuildReadOperation(branch, name);

            var (commitId, _) = await ReadCommitRefImplAsync(refsTable, operation, Serializer, cancellationToken).ConfigureAwait(false);
            return commitId;
        }

        #endregion

        #region Write

        public async Task WriteCommitRefAsync(CommitId? previousCommitId, string branch, string name, CommitId newCommitId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refsTable = _refsTable.Value;
            var operation = DataEntity.BuildReadOperation(branch, name);

            // Load existing commit ref in order to use its ETAG
            var (existingCommitId, etag) = await ReadCommitRefImplAsync(refsTable, operation, Serializer, cancellationToken).ConfigureAwait(false);

            if (existingCommitId != CommitId.Empty)
            {
                // Idempotent
                if (existingCommitId == newCommitId)
                    return;

                // Optimistic concurrency check
                if (previousCommitId.HasValue
                    && existingCommitId != previousCommitId.Value)
                {
                    throw BuildConcurrencyException(branch, name, null);
                }
            }

            try
            {
                // CommitRefs are not compressed
                using (var session = Serializer.Serialize(newCommitId.Sha1))
                {
                    var op = DataEntity.BuildWriteOperation(branch, name, session.Result, etag); // Note ETAG access condition

                    await refsTable.ExecuteAsync(op).ConfigureAwait(false);
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
            {
                throw BuildConcurrencyException(branch, name, se);
            }
        }

        #endregion

        #region Helpers

        private static ChasmConcurrencyException BuildConcurrencyException(string branch, string name, Exception innerException)
            => new ChasmConcurrencyException($"Concurrent write detected on {nameof(CommitRef)} {branch}/{name}", innerException);

        #endregion
    }
}
