using LiteDB;
using Newtonsoft.Json;

namespace Mattermost.Models
{
    public class Team
    {
        [BsonIndex(true)]
        public string ID { get; set; }
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        public string Name { get; set; }

        public Team() { }
    }
}
