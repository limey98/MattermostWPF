using Mattermost.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Mattermost.Models
{
    class WebSocketMessage
    {
        [JsonProperty("channel_id")]
        public string ChannelID { get; set; }
        [JsonProperty("user_id")]
        [JsonConverter(typeof(UserConverter))]
        public User UserID { get; set; }
        public MessageAction Action { get; set; }
        [JsonProperty("props")]
        public dynamic Properties { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    enum MessageAction
    {
        Typing,
        Posted,
        [EnumMember(Value = "post_edited")]
        PostEdited,
        [EnumMember(Value = "post_deleted")]
        PostDeleted,
        [EnumMember(Value = "channel_viewed")]
        ChannelViewed,
        [EnumMember(Value = "new_user")]
        NewUser,
        [EnumMember(Value = "user_added")]
        UserAdded,
        [EnumMember(Value = "user_removed")]
        UserRemoved,
        [EnumMember(Value = "preference_changed")]
        PreferenceChanged,
        [EnumMember(Value = "ephemeral_message")]
        EphemeralMessage
    }
}
