#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System.IO.Compression;

namespace SourceCode.Chasm.IO
{
    public partial interface IChasmRepository
    {
        #region Properties

        IChasmSerializer Serializer { get; }

        CompressionLevel CompressionLevel { get; }

        int MaxDop { get; }

        #endregion
    }
}
