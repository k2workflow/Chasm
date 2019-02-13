using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;
using SourceCode.Clay.Collections.Generic;

namespace SourceCode.Chasm.Repository
{
    partial class ChasmRepository // .Object
    {
        #region Read

        public abstract Task<bool> ExistsAsync(Sha1 objectId, CancellationToken cancellationToken);

        public abstract Task<IChasmBlob> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken);

        public abstract Task<IChasmStream> ReadStreamAsync(Sha1 objectId, CancellationToken cancellationToken);

        public virtual async Task<IReadOnlyDictionary<Sha1, IChasmBlob>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            if (objectIds == null)
                return EmptyMap<Sha1, IChasmBlob>.Empty;

            // Enumerate
            var dict = new Dictionary<Sha1, Task<IChasmBlob>>(Sha1Comparer.Default);
            foreach (Sha1 objectId in objectIds)
            {
                dict[objectId] = ReadObjectAsync(objectId, cancellationToken);
            }

            // Await
            await Task.WhenAll(dict.Values)
                .ConfigureAwait(false);

            // Return
            var dict2 = new Dictionary<Sha1, IChasmBlob>(Sha1Comparer.Default);
            foreach (KeyValuePair<Sha1, Task<IChasmBlob>> task in dict)
            {
                if (task.Value == null || task.Value.Result == null)
                    continue;

                dict2[task.Key] = task.Value.Result;
            }

            return dict2;
        }

        #endregion

        #region Write

        public abstract Task<WriteResult<Sha1>> WriteObjectAsync(ReadOnlyMemory<byte> buffer, Metadata metadata, bool forceOverwrite, CancellationToken cancellationToken);

        public abstract Task<WriteResult<Sha1>> WriteObjectAsync(Stream stream, Metadata metadata, bool forceOverwrite, CancellationToken cancellationToken);

        public abstract Task<WriteResult<Sha1>> WriteObjectAsync(Func<Stream, ValueTask> beforeHash, Metadata metadata, bool forceOverwrite, CancellationToken cancellationToken);

        public virtual async Task<IReadOnlyList<WriteResult<Sha1>>> WriteObjectsAsync(IEnumerable<IChasmBlob> blobs, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (blobs == null || !blobs.Any())
                return Array.Empty<WriteResult<Sha1>>();

            var tasks = new List<Task<WriteResult<Sha1>>>();
            foreach (IChasmBlob blob in blobs)
            {
                // Concurrency: instantiate tasks without await
                Task<WriteResult<Sha1>> task = WriteObjectAsync(blob.Content, blob.Metadata, forceOverwrite, cancellationToken);
                tasks.Add(task);
            }

            // Await the tasks
            var list = new WriteResult<Sha1>[tasks.Count];
            for (var i = 0; i < tasks.Count; i++)
            {
                list[i] = await tasks[i]
                    .ConfigureAwait(false);
            }

            return list;
        }

        #endregion
    }
}
