using Newtonsoft.Json;

namespace Mattermost.Models
{
    class Team
    {
        public string ID { get; set; }
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        public string Name { get; set; }
    }
}
