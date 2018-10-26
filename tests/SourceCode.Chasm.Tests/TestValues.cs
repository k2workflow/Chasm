using System;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Chasm.Tests.TestObjects;

namespace SourceCode.Chasm.Tests
{
    /// <summary>
    /// Values used for testing.
    /// </summary>
    public static class TestValues
    {
        public static readonly CancellationToken CancellationToken = new CancellationToken(false);

        public static readonly ParallelOptions ParallelOptions = new ParallelOptions()
        {
            CancellationToken = CancellationToken
        };

        public static readonly ReadOnlyMemory<byte> ReadOnlyMemory = new ReadOnlyMemory<byte>(RandomHelper.ByteArray);
    }
}
