using SourceCode.Clay.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial class ChasmRepository // .Object
    {
        #region Constants

        protected const int ConcurrentThreshold = 3;

        #endregion

        #region Read

        public abstract ReadOnlyMemory<byte> ReadObject(Sha1 objectId);

        public abstract ValueTask<ReadOnlyMemory<byte>> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken);

        public virtual IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>> ReadObjects(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            if (objectIds == null) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();

            if (objectIds is ICollection<Sha1> sha1s)
            {
                if (sha1s.Count == 0) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();

                // For small count, run non-concurrent
                if (sha1s.Count <= ConcurrentThreshold)
                {
                    var dict = new Dictionary<Sha1, ReadOnlyMemory<byte>>(sha1s.Count);

                    foreach (var sha1 in sha1s)
                    {
                        var buffer = ReadObject(sha1);

                        dict[sha1] = buffer;
                    }

                    return dict;
                }
            }

            var result = ReadConcurrent(objectIds, cancellationToken);
            return result;
        }

        public virtual async ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectsAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            if (objectIds == null) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();

            if (objectIds is ICollection<Sha1> sha1s)
            {
                if (sha1s.Count == 0) return ReadOnlyDictionary.Empty<Sha1, ReadOnlyMemory<byte>>();

                // For small count, run non-concurrent
                if (sha1s.Count <= ConcurrentThreshold)
                {
                    var dict = new Dictionary<Sha1, ReadOnlyMemory<byte>>(sha1s.Count);

                    foreach (var sha1 in sha1s)
                    {
                        var buffer = await ReadObjectAsync(sha1, cancellationToken).ConfigureAwait(false);

                        dict[sha1] = buffer;
                    }

                    return dict;
                }
            }

            // Run concurrent
            var result = ReadConcurrent(objectIds, cancellationToken);
            return result;
        }

        private IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>> ReadConcurrent(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = MaxDop
            };

            var dict = new ConcurrentDictionary<Sha1, ReadOnlyMemory<byte>>();

            Parallel.ForEach(objectIds, options, sha1 =>
            {
                // Bad practice to use async within Parallel
                var buffer = ReadObject(sha1);

                dict[sha1] = buffer;
            });

            return dict;
        }

        #endregion

        #region Write

        public abstract void WriteObject(Sha1 objectId, ArraySegment<byte> segment, bool forceOverwrite);

        public abstract Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> segment, bool forceOverwrite, CancellationToken cancellationToken);

        public virtual void WriteObjects(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (items == null) return;

            if (items is ICollection<KeyValuePair<Sha1, ArraySegment<byte>>> coll)
            {
                if (coll.Count == 0) return;

                // For small count, run non-concurrent
                if (coll.Count <= ConcurrentThreshold)
                {
                    foreach (var kvp in coll)
                    {
                        WriteObject(kvp.Key, kvp.Value, forceOverwrite);
                    }

                    return;
                }
            }

            // Run concurrent
            WriteConcurrent(items, forceOverwrite, cancellationToken);
        }

        public virtual async Task WriteObjectsAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (items == null) return;

            if (items is ICollection<KeyValuePair<Sha1, ArraySegment<byte>>> coll)
            {
                if (coll.Count == 0) return;

                // For small count, run non-concurrent
                if (coll.Count <= ConcurrentThreshold)
                {
                    foreach (var kvp in coll)
                    {
                        await WriteObjectAsync(kvp.Key, kvp.Value, forceOverwrite, cancellationToken).ConfigureAwait(false);
                    }

                    return;
                }
            }

            // Run concurrent
            WriteConcurrent(items, forceOverwrite, cancellationToken);
        }

        private void WriteConcurrent(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = MaxDop
            };

            Parallel.ForEach(items, options, kvp =>
            {
                // Bad practice to use async within Parallel
                WriteObject(kvp.Key, kvp.Value, forceOverwrite);
            });
        }

        #endregion
    }
}
