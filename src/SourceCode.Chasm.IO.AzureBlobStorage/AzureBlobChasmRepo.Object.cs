using Microsoft.WindowsAzure.Storage;
using SourceCode.Clay;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.AzureBlobStorage
{
    partial class AzureBlobChasmRepo // .Object
    {
        #region Read

        public override ReadOnlyMemory<byte> ReadObject(Sha1 objectId)
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
                    blobRef.DownloadToStreamAsync(input).Wait();

                    using (var output = new MemoryStream())
                    {
                        input.Position = 0; // Else gzip returns []
                        gzip.CopyTo(output);

                        var bytes = output.ToArray();
                        return bytes;
                    }
                }
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                // Try-catch is cheaper than a separate exists check
                se.Suppress();
            }

            return Array.Empty<byte>();
        }

        public override async ValueTask<ReadOnlyMemory<byte>> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
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

                        var bytes = output.ToArray();
                        return bytes;
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

        #endregion

        #region Write

        public override void WriteObject(Sha1 objectId, ArraySegment<byte> segment, bool forceOverwrite)
        {
            var objectsContainer = _objectsContainer.Value;

            var blobName = DeriveBlobName(objectId);
            var blobRef = objectsContainer.GetAppendBlobReference(blobName);

            // Objects are immutable
            if (!forceOverwrite)
            {
                var exists = blobRef.ExistsAsync().Result;
                if (exists)
                    return;
            }

            // Required to create blob before appending to it
            blobRef.CreateOrReplaceAsync().Wait();

            using (var output = new MemoryStream())
            {
                using (var gz = new GZipStream(output, CompressionLevel, true))
                {
                    gz.Write(segment.Array, segment.Offset, segment.Count);
                }
                output.Position = 0;

                // Append blob. Following seems to be the only safe multi-writer method available
                // http://stackoverflow.com/questions/32530126/azure-cloudappendblob-errors-with-concurrent-access
                blobRef.AppendBlockAsync(output).Wait();
            }
        }

        public override async Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> segment, bool forceOverwrite, CancellationToken cancellationToken)
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
