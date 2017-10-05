using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SourceCode.Clay;
using SourceCode.Clay.Collections.Generic;
using System;
using System.Collections.Concurrent;
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
        #region Constants

        private const int ConcurrentThreshold = 3;

        #endregion

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

        public async ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectsAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            if (objectIds == null) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();

            if (objectIds is ICollection<Sha1> sha1s)
            {
                if (sha1s.Count == 0) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();

                // For small count, run non-concurrent
                if (sha1s.Count <= ConcurrentThreshold)
                {
                    var dict = new Dictionary<Sha1, ReadOnlyMemory<byte>>(sha1s.Count);

                    foreach (var sha1 in sha1s)
                    {
                        var buffer = await ReadObjectAsync(sha1, cancellationToken).ConfigureAwait(false);

                        dict[sha1] = buffer;
                    }

                    return dict;
                }
            }
            else if (!objectIds.Any())
            {
                return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();
            }

            // Run concurrent
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = MaxDop
            };

            var objectsContainer = _objectsContainer.Value;

            var result = ReadConcurrentImpl(objectsContainer, objectIds, options);
            return result;
        }

        private static IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>> ReadConcurrentImpl(CloudBlobContainer objectsContainer, IEnumerable<Sha1> objectIds, ParallelOptions options)
        {
            var dict = new ConcurrentDictionary<Sha1, ReadOnlyMemory<byte>>();

            Parallel.ForEach(objectIds, options, sha1 =>
            {
                var blobName = DeriveBlobName(sha1);
                var blobRef = objectsContainer.GetAppendBlobReference(blobName);

                try
                {
                    using (var input = new MemoryStream())
                    using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
                    {
                        // Perf: Use a stream instead of a preceding call to fetch the buffer length

                        // Bad practice to use async within Parallel
                        blobRef.DownloadToStreamAsync(input).Wait();

                        using (var output = new MemoryStream())
                        {
                            input.Position = 0; // Else gzip returns []
                            gzip.CopyTo(output);

                            var buffer = output.ToArray(); // TODO: Perf
                            dict[sha1] = buffer;
                            return;
                        }
                    }
                }
                catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                {
                    // Try-catch is cheaper than a separate exists check
                    se.Suppress();
                    dict[sha1] = Array.Empty<byte>(); // TODO: Is this sufficient. Maybe use null or NotFound?
                }
            });

            return dict;
        }

        #endregion

        #region Write

        public async Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> segment, bool forceOverwrite, CancellationToken cancellationToken)
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
                    gz.Write(segment.Array, segment.Offset, segment.Count);
                }
                output.Position = 0;

                // Append blob. Following seems to be the only safe multi-writer method available
                // http://stackoverflow.com/questions/32530126/azure-cloudappendblob-errors-with-concurrent-access
                await blobRef.AppendBlockAsync(output).ConfigureAwait(false);
            }
        }

        public async Task WriteObjectsAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (items == null) return;

            if (items is ICollection<KeyValuePair<Sha1, ArraySegment<byte>>> coll)
            {
                if (coll.Count == 0) return;

                // For small count, run non-concurrent
                if (coll.Count <= ConcurrentThreshold)
                {
                    foreach (var kvp in coll)
                    {
                        await WriteObjectAsync(kvp.Key, kvp.Value, forceOverwrite, cancellationToken).ConfigureAwait(false);
                    }

                    return;
                }
            }

            // Run concurrent
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = MaxDop
            };

            WriteConcurrentImpl(_objectsContainer.Value, items, forceOverwrite, CompressionLevel, options);
        }

        private static void WriteConcurrentImpl(CloudBlobContainer objectsContainer, IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CompressionLevel compressionLevel, ParallelOptions options)
        {
            Parallel.ForEach(items, options, kvp =>
            {
                var blobName = DeriveBlobName(kvp.Key);
                var blobRef = objectsContainer.GetAppendBlobReference(blobName);

                // Objects are immutable
                if (!forceOverwrite)
                {
                    var exists = blobRef.ExistsAsync().Result;
                    if (exists)
                        return;
                }

                // Bad practice to use async within Parallel

                // Required to create blob before appending to it
                blobRef.CreateOrReplaceAsync().Wait();

                using (var output = new MemoryStream())
                {
                    using (var gz = new GZipStream(output, compressionLevel, true))
                    {
                        gz.Write(kvp.Value.Array, kvp.Value.Offset, kvp.Value.Count);
                    }
                    output.Position = 0;

                    // Append blob. Following seems to be the only safe multi-writer method available
                    // http://stackoverflow.com/questions/32530126/azure-cloudappendblob-errors-with-concurrent-access
                    blobRef.AppendBlockAsync(output).Wait();
                }
            });
        }

        public Task WriteObjectsAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, int maxDop, CancellationToken cancellationToken)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (maxDop < -1 || maxDop == 0) throw new ArgumentOutOfRangeException(nameof(maxDop));

            return AsyncParallelUtil.ForEach(items, kvps =>
            {
                var task = WriteObjectAsync(kvps.Key, kvps.Value, false, cancellationToken);
                return task;
            },
            maxDop,
            cancellationToken);
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
