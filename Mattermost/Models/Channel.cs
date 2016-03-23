using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Mattermost.Models
{
    class Channel
    {
        static List<Channel> channelList = new List<Channel>();

        public static Channel GetChannelByID(string id)
        {
            return channelList.FirstOrDefault(c => c.ID == id);
        }

        public string ID { get; set; }
        public ChannelType Type { get; set; }
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string Header { get; set; }

        public Channel()
        {
            channelList.Add(this);
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    enum ChannelType
    {
        [EnumMember(Value="O")]
        Public,
        [EnumMember(Value="P")]
        Private,
        [EnumMember(Value="D")]
        Direct
    }
}
