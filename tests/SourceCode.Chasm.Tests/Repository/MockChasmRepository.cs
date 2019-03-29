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
        internal static ChasmConcurrencyException MockBuildConcurrencyException(string name, string branch, Exception innerException, ChasmRequestContext requestContext)
        {
            return BuildConcurrencyException(branch, name, innerException, requestContext);
        }

        public MockChasmRepository(IChasmSerializer serializer, int maxDop)
            : base(serializer, maxDop)
        { }

        public override Task<bool> ExistsAsync(Sha1 objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<IChasmBlob> ReadObjectAsync(Sha1 objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<IChasmStream> ReadStreamAsync(Sha1 objectId, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<IReadOnlyDictionary<Sha1, IChasmBlob>> ReadObjectBatchAsync(IEnumerable<Sha1> objectIds, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<WriteResult<Sha1>> WriteObjectAsync(ReadOnlyMemory<byte> item, ChasmMetadata metadata, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<WriteResult<Sha1>> WriteObjectAsync(Stream stream, ChasmMetadata metadata, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<WriteResult<Sha1>> WriteObjectAsync(Func<Stream, ValueTask> writeAction, ChasmMetadata metadata, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<IReadOnlyList<WriteResult<Sha1>>> WriteObjectsAsync(IEnumerable<IChasmBlob> blobs, bool forceOverwrite, ChasmRequestContext requestContext = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
