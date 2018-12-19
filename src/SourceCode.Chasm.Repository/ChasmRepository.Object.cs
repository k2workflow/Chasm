using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.Repository
{
    partial class ChasmRepository // .Object
    {
        public abstract Task<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken);

        public abstract Task<Stream> ReadStreamAsync(Sha1 objectId, CancellationToken cancellationToken);

        public virtual async Task<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            if (objectIds == null) return ImmutableDictionary<Sha1, ReadOnlyMemory<byte>>.Empty;

            // Enumerate batches
            var dict = new Dictionary<Sha1, Task<ReadOnlyMemory<byte>?>>(Sha1Comparer.Default);
            foreach (Sha1 objectId in objectIds)
            {
                dict[objectId] = ReadObjectAsync(objectId, cancellationToken);
            }

            await Task.WhenAll(dict.Values.ToList())
                .ConfigureAwait(false);

            var dict2 = new Dictionary<Sha1, ReadOnlyMemory<byte>>(Sha1Comparer.Default);
            foreach (var task in dict)
            {
                if (task.Value == null || task.Value.Result.Value.Length == 0)
                    continue;

                dict2[task.Key] = task.Value.Result.Value;
            }

            return dict2;
        }

        public abstract Task<Sha1> WriteObjectAsync(Memory<byte> memory, bool forceOverwrite, CancellationToken cancellationToken);

        public abstract Task<Sha1> WriteObjectAsync(Stream stream, bool forceOverwrite, CancellationToken cancellationToken);

        public virtual async Task WriteObjectBatchAsync(IEnumerable<Memory<byte>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (items == null || !items.Any()) return;

            var tasks = new List<Task>();
            foreach (Memory<byte> item in items)
            {
                Task<Sha1> task = WriteObjectAsync(item, forceOverwrite, cancellationToken);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks)
                .ConfigureAwait(false);
        }

        #region Helpers

        protected async Task<(Sha1 Sha1, string Path)> WriteScratchFileAsync(string path, Memory<byte> item, bool forceOverwrite, CancellationToken cancellationToken)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(path));

            string tempPath = Path.Combine(path, Path.GetTempFileName()); // Note that an empty file is created

            using (var fs = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            using (var gz = new GZipStream(fs, CompressionLevel, true))
            using (var ct = crypt.SHA1.Create())
            {
                using (var cs = new crypt.CryptoStream(gz, ct, crypt.CryptoStreamMode.Write))
                {
                    await cs.WriteAsync(item, cancellationToken)
                        .ConfigureAwait(false);
                }

                var sha1 = new Sha1(ct.Hash);
                return (sha1, tempPath);
            }
        }

        protected async Task<(Sha1 Sha1, string Path)> WriteScratchFileAsync(string path, Stream stream, bool forceOverwrite, CancellationToken cancellationToken)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(path));
            Debug.Assert(stream != null);

            string tempPath = Path.Combine(path, Path.GetTempFileName()); // Note that an empty file is created

            using (var fs = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            using (var gz = new GZipStream(fs, CompressionLevel, true))
            using (var ct = crypt.SHA1.Create())
            {
                using (var cs = new crypt.CryptoStream(gz, ct, crypt.CryptoStreamMode.Write))
                {
                    await stream.CopyToAsync(cs, cancellationToken)
                        .ConfigureAwait(false);
                }

                var sha1 = new Sha1(ct.Hash);
                return (sha1, tempPath);
            }
        }

        #endregion
    }
}
