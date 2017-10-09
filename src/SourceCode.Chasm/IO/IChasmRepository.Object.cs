#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

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

        ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, ParallelOptions parallelOptions);

        #endregion

        #region Write

        Task WriteObjectAsync(Sha1 objectId, ArraySegment<byte> item, bool forceOverwrite, CancellationToken cancellationToken);

        Task WriteObjectBatchAsync(IEnumerable<KeyValuePair<Sha1, ArraySegment<byte>>> items, bool forceOverwrite, ParallelOptions parallelOptions);

        #endregion
    }
}
