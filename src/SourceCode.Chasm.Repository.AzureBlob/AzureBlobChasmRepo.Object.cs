#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Microsoft.WindowsAzure.Storage;
using SourceCode.Clay;
using SourceCode.Clay.Collections.Generic;
using SourceCode.Clay.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.AzureBlob
{
    partial class AzureBlobChasmRepo // .Object
    {
        #region Read

        public async ValueTask<ReadOnlyMemory<byte>> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            var objectsContainer = _objectsContainer.Value;

            var blobName = DeriveBlobName(objectId);
            var blobRef = objectsContainer.GetAppendBlobReference(blobName);

            try
            {
                using (var input = new MemoryStream())
                using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
                {
                    // Perf: Use a stream instead of a preceding call to fetch the buffer length
                    await blobRef.DownloadToStreamAsync(input).ConfigureAwait(false);

                    using (var output = new MemoryStream())
                    {
                        input.Position = 0; // Else gzip returns []
                        gzip.CopyTo(output);

                        var buffer = output.ToArray(); // TODO: Perf
                        return buffer;
                    }
                }
            }
            // Try-catch is cheaper than a separate exists check
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                se.Suppress();
            }

            return Array.Empty<byte>();
        }

        public ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, ParallelOptions parallelOptions)
        {
            if (objectIds == null)
                return new ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>>(ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>());

            // Execute batches
            var task = ParallelAsync.ForEachAsync(objectIds, parallelOptions, async n =>
            {
                // Execute batch
                var buffer = await ReadObjectAsync(n, parallelOptions.CancellationToken).ConfigureAwait(false);

                // Transform batch result
                var kvp = new KeyValuePair<Sha1, ReadOnlyMemory<byte>>(n, buffer);
                return kvp;
            });

            return task;
        }

        #endregion

        #region Write

        public async Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> item, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var objectsContainer = _objectsContainer.Value;

            var blobName = DeriveBlobName(objectId);
            var blobRef = objectsContainer.GetAppendBlobReference(blobName);

            // Objects are immutable
            if (!forceOverwrite)
            {
                var exists = await blobRef.ExistsAsync().ConfigureAwait(false);
                if (exists)
                    return;
            }

            // Required to create blob before appending to it
            await blobRef.CreateOrReplaceAsync().ConfigureAwait(false);

            using (var output = new MemoryStream())
            {
                using (var gz = new GZipStream(output, CompressionLevel, true))
                {
                    gz.Write(item.Array, item.Offset, item.Count);
                }
                output.Position = 0;

                // Append blob. Following seems to be the only safe multi-writer method available
                // http://stackoverflow.com/questions/32530126/azure-cloudappendblob-errors-with-concurrent-access
                await blobRef.AppendBlockAsync(output).ConfigureAwait(false);
            }
        }

        public Task WriteObjectBatchAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, ParallelOptions parallelOptions)
        {
            if (items == null || !items.Any()) return Task.CompletedTask;

            // Execute batches
            var task = ParallelAsync.ForEachAsync(items, parallelOptions, async kvps =>
            {
                // Execute batch
                await WriteObjectAsync(kvps.Key, kvps.Value, forceOverwrite, parallelOptions.CancellationToken).ConfigureAwait(false);
            });

            return task;
        }

        #endregion

        #region Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DeriveBlobName(Sha1 sha1)
        {
            var tokens = sha1.Split(2);

            var blobName = $@"{tokens.Key}/{tokens.Value}";
            blobName = Uri.EscapeUriString(blobName);

            return blobName;
        }

        #endregion
    }
}
