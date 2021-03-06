using System;
using System.Threading;
using SourceCode.Chasm.Serializer;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.Repository
{
    public abstract partial class ChasmRepository : IChasmRepository
    {
        // Use a thread-local instance of the underlying crypto algorithm.
        private static readonly ThreadLocal<crypt.SHA1> s_hasher = new ThreadLocal<crypt.SHA1>(crypt.SHA1.Create);
        protected static crypt.SHA1 Hasher => s_hasher.Value;

        public IChasmSerializer Serializer { get; }

        protected ChasmRepository(IChasmSerializer serializer)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        protected static ChasmConcurrencyException BuildConcurrencyException(string name, string branch, Exception innerException, ChasmRequestContext chasmContext)
            => new ChasmConcurrencyException($"Concurrent write detected on {name}/{branch} ({chasmContext?.CorrelationId})", innerException);
    }
}
