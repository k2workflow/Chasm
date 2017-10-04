using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.IO
{
    partial interface IChasmRepository // .Object
    {
        #region Read

        ValueTask<ReadOnlyMemory<byte>> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken);

        ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectsAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken);

        #endregion

        #region Write

        Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> segment, bool forceOverwrite, CancellationToken cancellationToken);

        Task WriteObjectsAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken);

        #endregion
    }
}
