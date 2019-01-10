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

        public override Task<bool> ExistsAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            string filename = DeriveFileName(objectId);
            string filePath = Path.Combine(_objectsContainer, filename);

            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                return Task.FromResult(false);

            bool exists = File.Exists(filePath);
            return Task.FromResult(exists);
        }

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

        /// <summary>
        /// Writes a buffer to the destination, returning the content's <see cref="Sha1"/> value.
        /// </summary>
        /// <param name="buffer">The content to hash and write.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override async Task<WriteResult<Sha1>> WriteObjectAsync(Memory<byte> buffer, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var created = true;

            ValueTask AfterHash(Sha1 sha1, string tempPath)
            {
                created = Rename(sha1, tempPath, forceOverwrite);
                return default; // Task.CompletedTask
            }

            Sha1 objectId = await WriteFileAsync(buffer, AfterHash, cancellationToken)
                .ConfigureAwait(false);

            return new WriteResult<Sha1>(objectId, created);
        }

        /// <summary>
        /// Writes a stream to the destination, returning the content's <see cref="Sha1"/> value.
        /// </summary>
        /// <param name="stream">The content to hash and write.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override async Task<WriteResult<Sha1>> WriteObjectAsync(Stream stream, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var created = true;

            ValueTask AfterHash(Sha1 sha1, string tempPath)
            {
                created = Rename(sha1, tempPath, forceOverwrite);
                return default; // Task.CompletedTask
            }

            Sha1 objectId = await WriteFileAsync(stream, AfterHash, cancellationToken)
                .ConfigureAwait(false);

            return new WriteResult<Sha1>(objectId, created);
        }

        /// <summary>
        /// Writes a stream to the destination, returning the content's <see cref="Sha1"/> value.
        /// The <paramref name="beforeHash"/> function permits a transformation operation
        /// on the source value before calculating the hash and writing to the destination.
        /// For example, the source stream may be encoded as Json.
        /// </summary>
        /// <param name="beforeHash">An action to take on the internal stream, before calculating the hash.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override async Task<WriteResult<Sha1>> WriteObjectAsync(Func<Stream, ValueTask> beforeHash, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (beforeHash == null) throw new ArgumentNullException(nameof(beforeHash));

            bool created = true;

            ValueTask AfterHash(Sha1 sha1, string tempPath)
            {
                created = Rename(sha1, tempPath, forceOverwrite);
                return default; // Task.CompletedTask
            }

            Sha1 objectId = await WriteFileAsync(beforeHash, AfterHash, cancellationToken)
                .ConfigureAwait(false);

            return new WriteResult<Sha1>(objectId, created);
        }

        private bool Rename(Sha1 objectId, string tempPath, bool forceOverwrite)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(tempPath));

            string filename = DeriveFileName(objectId);
            string filePath = Path.Combine(_objectsContainer, filename);
            string dir = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            // If file already exists then we can be sure it already contains the same content
            else if (File.Exists(filePath))
            {
                // Not created (already existed)
                if (!forceOverwrite)
                    return false;

                // TODO: Possible race-condition on delete+create if concurrent read access
                File.Delete(filePath);
            }

            // TODO: Possible race-condition if concurrent writes
            File.Move(tempPath, filePath);

            // Created
            return true;
        }

        #endregion
    }
}
