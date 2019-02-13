namespace SourceCode.Chasm.Repository
{
    public sealed class Metadata
    {
        public string Filename { get; }

        public string ContentType { get; }

        public Metadata(string filename, string contentType)
        {
            Filename = filename;
            ContentType = contentType;
        }
    }
}
