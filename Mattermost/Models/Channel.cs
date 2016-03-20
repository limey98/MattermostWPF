using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mattermost.Models
{
    class Channel
    {
        static Dictionary<string, Channel> channelList = new Dictionary<string, Channel>();

        public static Channel GetChannelByID(string id)
        {
            if (!channelList.ContainsKey(id))
                return null;

            return channelList[id];
        }

        string id;

        public string ID
        {
            get { return id; }
            set
            {
                if (id == value)
                    return;

                if (id != null && channelList.ContainsKey(id))
                    channelList.Remove(id);

                id = value;

                if (id != null)
                    channelList.Add(id, this);
            }
        }
        public ChannelType Type { get; set; }
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string Header { get; set; }
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
