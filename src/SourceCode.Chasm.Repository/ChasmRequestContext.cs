using System;

namespace SourceCode.Chasm.Repository
{
    public sealed class ChasmRequestContext
    {
        public string CorrelationId { get; set; }

        public string CustomUserAgent { get; set; }

        public static ChasmRequestContext Ensure(ChasmRequestContext requestContext)
        {
            if (requestContext == null)
                return new ChasmRequestContext { CorrelationId = $"{Guid.NewGuid():D}" };

            if (string.IsNullOrWhiteSpace(requestContext.CorrelationId))
                requestContext.CorrelationId = $"{Guid.NewGuid():D}";

            return requestContext;
        }
    }
}
