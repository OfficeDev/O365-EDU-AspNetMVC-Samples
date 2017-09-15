using Newtonsoft.Json;

namespace Microsoft.Education
{
    public class Entity
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}