using Newtonsoft.Json;

namespace SourceCode.Chasm.Repository.Disk
{
    [JsonObject(Id = "metadata")]
    internal sealed class JsonMetadata
    {
        private const string ContentTypeKey = "ct";
        private const string FilenameKey = "fn";

        [JsonProperty(PropertyName = ContentTypeKey, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ContentType { get; set; }

        [JsonProperty(PropertyName = FilenameKey, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Filename { get; set; }

        public JsonMetadata(string contentType, string filename)
        {
            ContentType = contentType;
            Filename = filename;
        }
        public JsonMetadata()
        { }

        public string ToJson()
            => JsonConvert.SerializeObject(this);

        public static JsonMetadata FromJson(string json)
            => JsonConvert.DeserializeObject<JsonMetadata>(json);
    }
}
