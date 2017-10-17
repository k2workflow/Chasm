#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Chasm.Tests.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Tests
{
    /// <summary>
    /// Values used for testing.
    /// </summary>
    public static class TestValues
    {
        #region Fields

        public static readonly CancellationToken CancellationToken = new CancellationToken(false);

        public static readonly ParallelOptions ParallelOptions = new ParallelOptions()
        {
            CancellationToken = CancellationToken
        };

        public static readonly ReadOnlyMemory<byte> ReadOnlyMemory = new ReadOnlyMemory<byte>(RandomHelper.ByteArray);

        #endregion
    }
}
