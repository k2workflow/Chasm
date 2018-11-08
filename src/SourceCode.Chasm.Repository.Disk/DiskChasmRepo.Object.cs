using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository.Disk
{
    partial class DiskChasmRepo // .Object
    {
        public override async ValueTask<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            string filename = DeriveFileName(objectId);
            string path = Path.Combine(_objectsContainer, filename);

            byte[] bytes = await ReadFileAsync(path, cancellationToken)
                .ConfigureAwait(false);

            if (bytes == null) return default;

            using (var input = new MemoryStream(bytes))
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

        public override async Task WriteObjectAsync(Sha1 objectId, Memory<byte> item, bool forceOverwrite, CancellationToken cancellationToken)
        {
            string filename = DeriveFileName(objectId);
            string path = Path.Combine(_objectsContainer, filename);

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Objects are immutable
            if (File.Exists(path)
                && !forceOverwrite)
                return;

            using (var output = new MemoryStream())
            {
                using (var gz = new GZipStream(output, CompressionLevel, true))
                {
                    gz.Write(item.Span);
                }
                output.Position = 0;

                await WriteFileAsync(path, output, false, cancellationToken)
                    .ConfigureAwait(false); // TODO: Perf
            }
        }
    }
}
