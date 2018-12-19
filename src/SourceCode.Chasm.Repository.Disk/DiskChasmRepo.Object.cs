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
        public override async Task<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            string filename = DeriveFileName(objectId);
            string filePath = Path.Combine(_objectsContainer, filename);

            byte[] bytes = await ReadFileAsync(filePath, cancellationToken)
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

        public override async Task<Stream> ReadStreamAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            string filename = DeriveFileName(objectId);
            string fiePath = Path.Combine(_objectsContainer, filename);

            string dir = Path.GetDirectoryName(fiePath);
            if (!Directory.Exists(dir))
                return default;

            if (!File.Exists(fiePath))
                return default;

            FileStream fileStream = await WaitForFileAsync(fiePath, FileMode.Open, FileAccess.Read, FileShare.Read, cancellationToken)
                .ConfigureAwait(false);

            var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress, false);
            return gzipStream;
        }

        public override async Task<Sha1> WriteObjectAsync(Memory<byte> item, bool forceOverwrite, CancellationToken cancellationToken)
        {
            (Sha1 sha1, string scratchFile) = await WriteScratchFileAsync(_scratchPath, item, forceOverwrite, cancellationToken)
                .ConfigureAwait(false);

            RenameScratchFile(forceOverwrite, sha1, scratchFile);

            return sha1;
        }

        public override async Task<Sha1> WriteObjectAsync(Stream stream, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            (Sha1 sha1, string scratchFile) = await WriteScratchFileAsync(_scratchPath, stream, forceOverwrite, cancellationToken)
                .ConfigureAwait(false);

            RenameScratchFile(forceOverwrite, sha1, scratchFile);

            return sha1;
        }

        private void RenameScratchFile(bool forceOverwrite, Sha1 sha1, string scratchFile)
        {
            try
            {
                string filename = DeriveFileName(sha1);
                string filePath = Path.Combine(_objectsContainer, filename);

                string dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                else if (File.Exists(filePath))
                {
                    // If file already exists then we can be sure it already contains the same content
                    if (!forceOverwrite)
                        return;

                    File.Delete(filePath);
                }

                File.Move(scratchFile, filePath);
            }
            finally
            {
                if (File.Exists(scratchFile))
                    File.Delete(scratchFile);
            }
        }
    }
}
