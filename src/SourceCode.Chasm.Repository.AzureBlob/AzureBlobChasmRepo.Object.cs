using Microsoft.WindowsAzure.Storage;
using SourceCode.Clay;
using SourceCode.Clay.Collections.Generic;
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

        public ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectsAsync(IEnumerable<Sha1> objectIds, ParallelOptions parallelOptions)
        {
            if (objectIds == null)
                return new ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>>(ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>());

            return AsyncParallelUtil.ForEachAsync(objectIds, parallelOptions, async n =>
            {
                var buffer = await ReadObjectAsync(n, parallelOptions.CancellationToken).ConfigureAwait(false);

                return new KeyValuePair<Sha1, ReadOnlyMemory<byte>>(n, buffer);
            });
        }

        #endregion

        #region Write

        public async Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> segment, CancellationToken cancellationToken)
        {
            var objectsContainer = _objectsContainer.Value;

            var blobName = DeriveBlobName(objectId);
            var blobRef = objectsContainer.GetAppendBlobReference(blobName);

            // Required to create blob before appending to it
            await blobRef.CreateOrReplaceAsync().ConfigureAwait(false);

            using (var output = new MemoryStream())
            {
                using (var gz = new GZipStream(output, CompressionLevel, true))
                {
                    gz.Write(segment.Array, segment.Offset, segment.Count);
                }
                output.Position = 0;

                // Append blob. Following seems to be the only safe multi-writer method available
                // http://stackoverflow.com/questions/32530126/azure-cloudappendblob-errors-with-concurrent-access
                await blobRef.AppendBlockAsync(output).ConfigureAwait(false);
            }
        }

        public Task WriteObjectsAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, ParallelOptions parallelOptions)
        {
            if (items == null || !items.Any()) return Task.CompletedTask;

            return AsyncParallelUtil.ForEachAsync(items, parallelOptions, async kvps =>
            {
                await WriteObjectAsync(kvps.Key, kvps.Value, parallelOptions.CancellationToken).ConfigureAwait(false);
            });
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
