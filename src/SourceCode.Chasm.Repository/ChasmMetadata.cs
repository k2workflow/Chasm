namespace SourceCode.Chasm.Repository
{
    public sealed class ChasmMetadata
    {
        public string Filename { get; }

        public string ContentType { get; }

        public ChasmMetadata(string contentType, string filename)
        {
            Filename = filename;
            ContentType = contentType;
        }
    }
}
