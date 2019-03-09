using System;
using System.Buffers;
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

        public override Task<bool> ExistsAsync(Sha1 objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            (string filePath, _) = DeriveFileNames(_objectsContainer, objectId);

            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                return Task.FromResult(false);

            bool exists = File.Exists(filePath);
            return Task.FromResult(exists);
        }

        public override async Task<IChasmBlob> ReadObjectAsync(Sha1 objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            (string filePath, string metaPath) = DeriveFileNames(_objectsContainer, objectId);

            using (IMemoryOwner<byte> owned = await ReadFileAsync(filePath, cancellationToken)
                .ConfigureAwait(false))
            {
                if (owned == null || owned.Memory.IsEmpty)
                    return default;

                ChasmMetadata metadata = ReadMetadata(metaPath);

                // HACK: Perf is horrid
                return new ChasmBlob(owned.Memory.ToArray(), metadata);
            }
        }

        public override async Task<IChasmStream> ReadStreamAsync(Sha1 objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            (string filePath, string metaPath) = DeriveFileNames(_objectsContainer, objectId);

            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                return default;

            if (!File.Exists(filePath))
                return default;

            ChasmMetadata metadata = ReadMetadata(metaPath);

            FileStream fileStream = await WaitForFileAsync(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return new ChasmStream(fileStream, metadata);
        }

        #endregion

        #region Write

        /// <summary>
        /// Writes a buffer to the destination, returning the content's <see cref="Sha1"/> value.
        /// </summary>
        /// <param name="buffer">The content to hash and write.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override async Task<WriteResult<Sha1>> WriteObjectAsync(ReadOnlyMemory<byte> buffer, ChasmMetadata metadata, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            requestContext = ChasmRequestContext.Ensure(requestContext);

            var created = true;

            ValueTask AfterWrite(Sha1 sha1, string tempPath)
            {
                created = Rename(sha1, tempPath, metadata, forceOverwrite);
                return default; // Task.CompletedTask
            }

            Sha1 objectId = await WriteFileAsync(buffer, AfterWrite, cancellationToken)
                .ConfigureAwait(false);

            return new WriteResult<Sha1>(objectId, created);
        }

        /// <summary>
        /// Writes a stream to the destination, returning the content's <see cref="Sha1"/> value.
        /// </summary>
        /// <param name="stream">The content to hash and write.</param>
        /// <param name="forceOverwrite">Forces the target to be ovwerwritten, even if it already exists.</param>
        /// <param name="cancellationToken">Allows the operation to be cancelled.</param>
        public override async Task<WriteResult<Sha1>> WriteObjectAsync(Stream stream, ChasmMetadata metadata, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            requestContext = ChasmRequestContext.Ensure(requestContext);

            var created = true;

            ValueTask AfterWrite(Sha1 sha1, string tempPath)
            {
                created = Rename(sha1, tempPath, metadata, forceOverwrite);
                return default; // Task.CompletedTask
            }

            Sha1 objectId = await WriteFileAsync(stream, AfterWrite, cancellationToken)
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
        public override async Task<WriteResult<Sha1>> WriteObjectAsync(Func<Stream, ValueTask> beforeHash, ChasmMetadata metadata, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (beforeHash == null) throw new ArgumentNullException(nameof(beforeHash));

            requestContext = ChasmRequestContext.Ensure(requestContext);

            bool created = true;

            ValueTask AfterWrite(Sha1 sha1, string tempPath)
            {
                created = Rename(sha1, tempPath, metadata, forceOverwrite);
                return default; // Task.CompletedTask
            }

            Sha1 objectId = await StageFileAsync(beforeHash, AfterWrite, cancellationToken)
                .ConfigureAwait(false);

            return new WriteResult<Sha1>(objectId, created);
        }

        private bool Rename(Sha1 objectId, string tempPath, ChasmMetadata metadata, bool forceOverwrite)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(tempPath));

            (string filePath, string metaPath) = DeriveFileNames(_objectsContainer, objectId);
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

            // Create metadata file
            WriteMetadata(metadata, metaPath);

            // Created
            return true;
        }

        #endregion
    }
}
