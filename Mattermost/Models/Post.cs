using Mattermost.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace Mattermost.Models
{
    class Post
    {
        public string ID { get; set; }

        [JsonProperty("user_id")]
        [JsonConverter(typeof(UserConverter))]
        public User User { get; set; }

        [JsonProperty("create_at")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Timestamp { get; set; }

        public PostType Type { get; set; }

        public string Message { get; set; }
    }

    class CreatePost
    {
        [JsonProperty("channel_id")]
        [JsonConverter(typeof(ChannelConverter))]
        public Channel Channel { get; set; }

        [JsonProperty("user_id")]
        [JsonConverter(typeof(UserConverter))]
        public User User { get; set; }

        [JsonProperty("create_at")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Created { get; set; }

        public string Message { get; set; }

        [JsonProperty("pending_post_id")]
        public string PendingPostID
        {
            get
            {
                return string.Format("{0}:{1}", User.ID, Created.ToUnixTime());
            }
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    enum PostType
    {
        [EnumMember(Value ="")]
        Default,
        [EnumMember(Value = "system_generic")]
        Generic,
        [EnumMember(Value = "system_join_leave")]
        JoinLeave,
        [EnumMember(Value = "join_leave")]
        OldJoinLeave,
        [EnumMember(Value = "system_header_change")]
        HeaderChange,
        [EnumMember(Value = "system_ephemeral")]
        Ephemeral
    }
}
