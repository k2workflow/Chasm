using Microsoft.WindowsAzure.Storage;
using SourceCode.Clay;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.AzureTableStorage
{
    partial class AzureTableChasmRepo // .CommitRef
    {
        #region Read

        public override CommitId ReadCommitRef(string branch, string name)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var (commitId, _) = ReadCommitRefImpl(branch, name);
            return commitId;
        }

        public override async ValueTask<CommitId> ReadCommitRefAsync(string branch, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var (commitId, _) = await ReadCommitRefImplAsync(branch, name, cancellationToken);
            return commitId;
        }

        public (CommitId, string) ReadCommitRefImpl(string branch, string name)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refsTable = _refsTable.Value;
            var op = DataEntity.BuildReadOperation(branch, name);

            var commitId = CommitId.Empty;
            string etag = null;
            try
            {
                // Read from table
                var result = refsTable.ExecuteAsync(op).Result;
                etag = result.Etag;

                if (result.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    return (commitId, etag);

                var bytes = (byte[])result.Result;
                using (var input = new MemoryStream(bytes))
                using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
                {
                    using (var output = new MemoryStream())
                    {
                        input.Position = 0; // Else gzip returns []
                        gzip.CopyTo(output);

                        if (output.Length > 0)
                        {
                            var sha1 = Serializer.DeserializeSha1(output.ToArray()); // TODO: Perf
                            commitId = new CommitId(sha1);
                        }
                    }
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                // Try-catch is cheaper than a separate exists check
                se.Suppress();
            }

            return (commitId, etag);
        }

        public async ValueTask<(CommitId, string)> ReadCommitRefImplAsync(string branch, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refsTable = _refsTable.Value;
            var op = DataEntity.BuildReadOperation(branch, name);

            var commitId = CommitId.Empty;
            string etag = null;
            try
            {
                // Read from table
                var result = await refsTable.ExecuteAsync(op, default, default, cancellationToken).ConfigureAwait(false);
                etag = result.Etag;

                if (result.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    return (commitId, etag);

                var bytes = (byte[])result.Result;
                using (var input = new MemoryStream(bytes))
                using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
                {
                    using (var output = new MemoryStream())
                    {
                        input.Position = 0; // Else gzip returns []
                        gzip.CopyTo(output);

                        if (output.Length > 0)
                        {
                            var sha1 = Serializer.DeserializeSha1(output.ToArray()); // TODO: Perf
                            commitId = new CommitId(sha1);
                        }
                    }
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                // Try-catch is cheaper than a separate exists check
                se.Suppress();
            }

            return (commitId, etag);
        }

        #endregion

        #region Write

        public override void WriteCommitRef(CommitId? previousCommitId, string branch, string name, CommitId newCommitId)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refsTable = _refsTable.Value;

            // Load existing commit ref in order to use its ETAG
            var (existingCommitId, etag) = ReadCommitRefImpl(branch, name);

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
                using (var output = new MemoryStream())
                {
                    using (var gz = new GZipStream(output, CompressionLevel, true))
                    using (var session = Serializer.Serialize(newCommitId.Sha1))
                    {
                        var result = session.Result;
                        gz.Write(result.Array, result.Offset, result.Count);
                    }
                    output.Position = 0;

                    var segment = new ArraySegment<byte>(output.ToArray()); // TODO: Perf
                    var op = DataEntity.BuildWriteOperation(branch, name, segment, etag); // Note ETAG access condition

                    var result1 = refsTable.ExecuteAsync(op).Result;
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
            {
                throw BuildConcurrencyException(branch, name, se);
            }
        }

        // TODO: CommitRef should use pessimistic concurrency control
        public override async Task WriteCommitRefAsync(CommitId? previousCommitId, string branch, string name, CommitId newCommitId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(branch)) throw new ArgumentNullException(nameof(branch));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var refsTable = _refsTable.Value;

            // Load existing commit ref in order to use its ETAG
            var (existingCommitId, etag) = ReadCommitRefImpl(branch, name);

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
                using (var output = new MemoryStream())
                {
                    using (var gz = new GZipStream(output, CompressionLevel, true))
                    using (var session = Serializer.Serialize(newCommitId.Sha1))
                    {
                        var result = session.Result;
                        gz.Write(result.Array, result.Offset, result.Count);
                    }
                    output.Position = 0;

                    var segment = new ArraySegment<byte>(output.ToArray()); // TODO: Perf
                    var op = DataEntity.BuildWriteOperation(branch, name, segment, etag); // Note ETAG access condition

                    var result1 = await refsTable.ExecuteAsync(op).ConfigureAwait(false);
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
            {
                throw BuildConcurrencyException(branch, name, se);
            }
        }

        #endregion
    }
}
