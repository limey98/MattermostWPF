using Mattermost.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Mattermost.Models
{
    class Post
    {
        static List<Post> postList = new List<Post>();

        public static Post GetPostByID(string id)
        {
            return postList.FirstOrDefault(p => p.ID == id);
        }

        public string ID { get; set; }

        [JsonProperty("user_id")]
        [JsonConverter(typeof(UserConverter))]
        public User User { get; set; }

        [JsonProperty("channel_id")]
        [JsonConverter(typeof(ChannelConverter))]
        public Channel Channel { get; set; }

        [JsonProperty("create_at")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Created { get; set; }

        [JsonProperty("update_at")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Updated { get; set; }

        [JsonProperty("root_id")]
        [JsonConverter(typeof(PostReferenceConverter))]
        public PostReference RootPost { get; set; }

        [JsonProperty("parent_id")]
        [JsonConverter(typeof(PostReferenceConverter))]
        public PostReference ParentPost { get; set; }

        [JsonProperty("original_id")]
        [JsonConverter(typeof(PostReferenceConverter))]
        public PostReference OriginalPost { get; set; }

        public PostType Type { get; set; }

        public string Message { get; set; }

        public Post()
        {
            postList.Add(this);
        }
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

    class PostReference
    {
        public string ID { get; set; }

        public Post Resolve()
        {
            return Post.GetPostByID(ID);
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
