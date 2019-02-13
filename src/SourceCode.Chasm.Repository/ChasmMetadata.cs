namespace SourceCode.Chasm.Repository
{
    public sealed class ChasmMetadata
    {
        public string Filename { get; }

        public string ContentType { get; }

        public ChasmMetadata(string filename, string contentType)
        {
            Filename = filename;
            ContentType = contentType;
        }
    }
}
