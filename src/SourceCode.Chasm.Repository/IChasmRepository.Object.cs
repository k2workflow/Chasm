using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository
{
    partial interface IChasmRepository // .Object
    {
        ValueTask<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken);

        Task<Stream> ReadStreamAsync(Sha1 objectId, CancellationToken cancellationToken);

        ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken);

        Task WriteObjectAsync(Sha1 objectId, Memory<byte> item, bool forceOverwrite, CancellationToken cancellationToken);

        Task<Sha1> HashObjectAsync(Memory<byte> item, bool forceOverwrite, CancellationToken cancellationToken);

        Task WriteObjectBatchAsync(IEnumerable<KeyValuePair<Sha1, Memory<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken);

        Task<Sha1> HashObjectAsync(Stream stream, bool forceOverwrite, CancellationToken cancellationToken);
    }
}
