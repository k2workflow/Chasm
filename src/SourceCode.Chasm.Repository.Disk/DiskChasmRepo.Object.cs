#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO.Disk
{
    partial class DiskChasmRepo // .Object
    {
        #region Read

        public override async ValueTask<ReadOnlyMemory<byte>> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            var filename = DeriveFileName(objectId);
            var path = Path.Combine(_objectsContainer, filename);

            var bytes = await ReadFileAsync(path, cancellationToken).ConfigureAwait(false);
            if (bytes == null)
                return default;

            using (var input = new MemoryStream(bytes.ToArray())) // TODO: Perf
            using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
            using (var output = new MemoryStream())
            {
                input.Position = 0; // Else gzip returns []
                gzip.CopyTo(output);

                if (output.Length > 0)
                {
                    return output.ToArray(); // TODO: Perf
                }
            }

            return default;
        }

        #endregion

        #region Write

        public override async Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> item, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var filename = DeriveFileName(objectId);
            var path = Path.Combine(_objectsContainer, filename);

            // Objects are immutable
            if (!File.Exists(path)
                && !forceOverwrite)
                return;

            using (var output = new MemoryStream())
            {
                using (var gz = new GZipStream(output, CompressionLevel, true))
                {
                    gz.Write(item.Array, item.Offset, item.Count);
                }
                output.Position = 0;

                await WriteFileAsync(path, output, cancellationToken, false).ConfigureAwait(false); // TODO: Perf
            }
        }

        #endregion
    }
}
