using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository.Disk
{
    partial class DiskChasmRepo // .Object
    {
        #region Read

        public override async Task<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            string filename = DeriveFileName(objectId);
            string filePath = Path.Combine(_objectsContainer, filename);

            byte[] bytes = await ReadFileAsync(filePath, cancellationToken)
                .ConfigureAwait(false);

            if (bytes == null) return default;
            return bytes;

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

            return fileStream;
        }

        #endregion

        #region Write

        public override Task<Sha1> WriteObjectAsync(Memory<byte> buffer, bool forceOverwrite, CancellationToken cancellationToken)
            => WriteFileAsync(buffer, (sha1, tempPath) => RenameFile(tempPath, sha1, forceOverwrite), true, cancellationToken);

        public override Task<Sha1> WriteObjectAsync(Stream stream, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            return WriteFileAsync(stream, (sha1, tempPath) => RenameFile(tempPath, sha1, forceOverwrite), true, cancellationToken);
        }

        private Task RenameFile(string tempPath, Sha1 sha1, bool forceOverwrite)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(tempPath));

            string filename = DeriveFileName(sha1);
            string filePath = Path.Combine(_objectsContainer, filename);

            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // If file already exists then we can be sure it already contains the same content
            else if (File.Exists(filePath))
            {
                if (!forceOverwrite)
                    return Task.CompletedTask;

                File.Delete(filePath);
            }

            File.Move(tempPath, filePath);

            return Task.CompletedTask;
        }

        #endregion
    }
}
