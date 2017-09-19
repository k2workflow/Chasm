using System;
using System.IO.Compression;

namespace SourceCode.Chasm.IO
{
    public abstract partial class ChasmRepository : IChasmRepository
    {
        #region Properties

        public IChasmSerializer Serializer { get; }

        public CompressionLevel CompressionLevel { get; }

        public int MaxDop { get; }

        #endregion

        #region Constructors

        protected ChasmRepository(IChasmSerializer serializer, CompressionLevel compressionLevel, int maxDop)
        {
            if (maxDop < -1 || maxDop == 0) throw new ArgumentOutOfRangeException(nameof(maxDop));
            if (!Enum.IsDefined(typeof(CompressionLevel), compressionLevel)) throw new ArgumentOutOfRangeException(nameof(compressionLevel));

            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            CompressionLevel = compressionLevel;
            MaxDop = maxDop;
        }

        protected ChasmRepository(IChasmSerializer serializer, CompressionLevel compressionLevel)
            : this(serializer, compressionLevel, -1)
        { }

        protected ChasmRepository(IChasmSerializer serializer)
            : this(serializer, CompressionLevel.Optimal)
        { }

        #endregion
    }
}
