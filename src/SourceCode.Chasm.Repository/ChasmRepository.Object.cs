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

        public abstract Task<bool> ExistsAsync(Sha1 objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default);

        public abstract Task<IChasmBlob> ReadObjectAsync(Sha1 objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default);

        public abstract Task<IChasmStream> ReadStreamAsync(Sha1 objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default);

        public virtual async Task<IReadOnlyDictionary<Sha1, IChasmBlob>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (objectIds == null)
                return EmptyDictionary.Empty<Sha1, IChasmBlob>();

            requestContext = ChasmRequestContext.Ensure(requestContext);

            // Enumerate
            var dict = new Dictionary<Sha1, Task<IChasmBlob>>(Sha1Comparer.Default);
            foreach (Sha1 objectId in objectIds)
            {
                dict[objectId] = ReadObjectAsync(objectId, requestContext, cancellationToken);
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

        public abstract Task<WriteResult<Sha1>> WriteObjectAsync(ReadOnlyMemory<byte> buffer, ChasmMetadata metadata, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default);

        public abstract Task<WriteResult<Sha1>> WriteObjectAsync(Stream stream, ChasmMetadata metadata, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default);

        public abstract Task<WriteResult<Sha1>> WriteObjectAsync(Func<Stream, ValueTask> beforeHash, ChasmMetadata metadata, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default);

        public virtual async Task<IReadOnlyList<WriteResult<Sha1>>> WriteObjectsAsync(IEnumerable<IChasmBlob> blobs, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            if (blobs == null || !blobs.Any())
                return Array.Empty<WriteResult<Sha1>>();

            requestContext = ChasmRequestContext.Ensure(requestContext);

            var tasks = new List<Task<WriteResult<Sha1>>>();
            foreach (IChasmBlob blob in blobs)
            {
                // Concurrency: instantiate tasks without await
                Task<WriteResult<Sha1>> task = WriteObjectAsync(blob.Content, blob.Metadata, forceOverwrite, requestContext, cancellationToken);
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
