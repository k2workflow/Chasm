using SourceCode.Chasm.Serializer;
using System;
using System.IO.Compression;

namespace SourceCode.Chasm.Repository.Hybrid
{
    public sealed partial class HybridChasmRepo : ChasmRepository
    {
        public IChasmRepository[] Chain { get; }

        public HybridChasmRepo(IChasmRepository repository1, IChasmRepository repository2, IChasmRepository repository3, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
            : base(serializer, compressionLevel, maxDop)
        {
            if (repository1 == null) throw new ArgumentNullException(nameof(repository1));
            if (repository2 == null) throw new ArgumentNullException(nameof(repository2));
            if (repository3 == null) throw new ArgumentNullException(nameof(repository3));

            Chain = new IChasmRepository[3] { repository1, repository2, repository3 };
        }

        public HybridChasmRepo(IChasmRepository repository1, IChasmRepository repository2, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
            : base(serializer, compressionLevel, maxDop)
        {
            if (repository1 == null) throw new ArgumentNullException(nameof(repository1));
            if (repository2 == null) throw new ArgumentNullException(nameof(repository2));

            Chain = new IChasmRepository[2] { repository1, repository2 };
        }

        public HybridChasmRepo(IChasmRepository repository, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
            : base(serializer, compressionLevel, maxDop)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));

            Chain = new IChasmRepository[1] { repository };
        }

        public HybridChasmRepo(IChasmRepository[] chain, IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
            : base(serializer, compressionLevel, maxDop)
        {
            if (chain == null || chain.Length == 0) throw new ArgumentNullException(nameof(chain));

            for (int i = 0; i < chain.Length; i++)
                if (chain[i] == null) throw new ArgumentNullException(nameof(chain));

            Chain = chain;
        }

        // TODO: Provide a builder that enables policy-based hybrid pipelines
        // TODO: Reading from a remote should propagate content to local
    }
}
