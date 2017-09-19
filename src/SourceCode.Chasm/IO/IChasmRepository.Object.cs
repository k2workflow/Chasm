using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial interface IChasmRepository // .Object
    {
        #region Read

        ReadOnlyBuffer<byte> ReadObject(Sha1 objectId);

        ValueTask<ReadOnlyBuffer<byte>> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken);

        IReadOnlyDictionary<Sha1, ReadOnlyBuffer<byte>> ReadObjects(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken);

        ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyBuffer<byte>>> ReadObjectsAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken);

        #endregion

        #region Write

        void WriteObject(Sha1 objectId, ArraySegment<byte> segment, bool forceOverwrite);

        Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> segment, bool forceOverwrite, CancellationToken cancellationToken);

        void WriteObjects(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken);

        Task WriteObjectsAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken);

        #endregion
    }
}
