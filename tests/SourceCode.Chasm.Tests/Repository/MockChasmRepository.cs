using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Chasm.Serializer;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository.Tests
{
    internal class MockChasmRepository : ChasmRepository
    {
        internal static ChasmConcurrencyException MockBuildConcurrencyException(string name, string branch, Exception innerException)
        {
            return BuildConcurrencyException(branch, name, innerException);
        }

        public MockChasmRepository(IChasmSerializer serializer, int maxDop)
            : base(serializer, maxDop)
        { }

        public override ValueTask<CommitRef?> ReadCommitRefAsync(string name, string branch, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> ExistsAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<IChasmBlob> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<IChasmStream> ReadStreamAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<IReadOnlyDictionary<Sha1, IChasmBlob>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task WriteCommitRefAsync(CommitId? previousCommitId, string branch, CommitRef commitRef, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<WriteResult<Sha1>> WriteObjectAsync(ReadOnlyMemory<byte> item, Metadata metadata, bool forceOverwrite, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<WriteResult<Sha1>> WriteObjectAsync(Stream stream, Metadata metadata, bool forceOverwrite, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<WriteResult<Sha1>> WriteObjectAsync(Func<Stream, ValueTask> writeAction, Metadata metadata, bool forceOverwrite, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<IReadOnlyList<WriteResult<Sha1>>> WriteObjectsAsync(IEnumerable<IChasmBlob> blobs, bool forceOverwrite, CancellationToken cancellationToken)
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
