using System;
using System.IO;
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

        public override async Task<Sha1> WriteObjectAsync(Memory<byte> memory, bool forceOverwrite, CancellationToken cancellationToken)
        {
            Task MoveAction(Sha1 sha, string tempPath)
            {
                Rename(forceOverwrite, sha, tempPath);
                return Task.CompletedTask;
            }

            Sha1 sha1 = await WriteFileAsync(memory, MoveAction, forceOverwrite, cancellationToken)
                .ConfigureAwait(false);

            return sha1;
        }

        public override async Task<Sha1> WriteObjectAsync(Stream stream, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            Task MoveAction(Sha1 sha, string tempPath)
            {
                Rename(forceOverwrite, sha, tempPath);
                return Task.CompletedTask;
            }

            Sha1 sha1 = await WriteFileAsync(stream, MoveAction, forceOverwrite, cancellationToken)
                .ConfigureAwait(false);

            return sha1;
        }

        private void Rename(bool forceOverwrite, Sha1 sha1, string tempFile)
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

                // If file already exists then we can be sure it already contains the same content
                else if (File.Exists(filePath))
                {
                    if (!forceOverwrite)
                        return;

                    File.Delete(filePath);
                }

                File.Move(tempFile, filePath);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}
