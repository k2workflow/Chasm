#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Microsoft.WindowsAzure.Storage;
using SourceCode.Clay;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.AzureBlob
{
    partial class AzureBlobChasmRepo // .Object
    {
        #region Read

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
                    // TODO: Perf: Use a stream instead of a preceding call to fetch the buffer length
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
            // Try-catch is cheaper than a separate (latent) exists check
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                se.Suppress();
            }

            return Array.Empty<byte>();
        }

        #endregion

        #region Write

        public override async Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> item, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var objectsContainer = _objectsContainer.Value;

            var blobName = DeriveBlobName(objectId);
            var blobRef = objectsContainer.GetAppendBlobReference(blobName);

            // Objects are immutable
            var accessCondition = forceOverwrite ? null : AccessCondition.GenerateIfNotExistsCondition(); // If-None-Match *

            try
            {
                // Required to create blob header before appending to it
                await blobRef.CreateOrReplaceAsync(accessCondition, default, default).ConfigureAwait(false);
            }
            // Try-catch is cheaper than a separate (latent) exists check
            catch (StorageException se) when (!forceOverwrite && se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                se.Suppress();
                return;
            }

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
