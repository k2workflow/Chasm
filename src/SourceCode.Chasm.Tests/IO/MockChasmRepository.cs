using SourceCode.Chasm.Repository;
using SourceCode.Chasm.Serializer;
using SourceCode.Clay;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Repository.Tests
{
    internal class MockChasmRepository : ChasmRepository
    {
        internal static ChasmConcurrencyException MockBuildConcurrencyException(string name, string branch, Exception innerException)
        {
            return BuildConcurrencyException(branch, name, innerException);
        }

        public MockChasmRepository(IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
            : base(serializer, compressionLevel, maxDop)
        { }

        public override ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<IReadOnlyDictionary<Sha1, ReadOnlyMemory<byte>>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task WriteCommitRefAsync(CommitId? previousCommitId, string branch, CommitRef commitRef, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task WriteObjectAsync(Sha1 objectId, Memory<byte> item, bool forceOverwrite, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task WriteObjectBatchAsync(IEnumerable<KeyValuePair<Sha1, Memory<byte>>> items, bool forceOverwrite, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<IReadOnlyList<string>> GetNamesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<IReadOnlyList<CommitRef>> GetBranchesAsync(string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
