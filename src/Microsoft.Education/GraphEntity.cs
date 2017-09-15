using Newtonsoft.Json;

namespace Microsoft.Education
{
    public abstract class GraphEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}