using Mattermost.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Mattermost.Utils
{
    public class UnixDateTimeConverter : DateTimeConverterBase
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            long val;

            if (value is DateTime)
            {
                val = ((DateTime)value).ToUnixTime();
            }
            else
            {
                throw new Exception("Expected date object value.");
            }

            writer.WriteValue(val);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Integer)
                throw new Exception("Wrong Token Type");

            long ticks = (long)reader.Value;

            return ticks.FromUnixTime();
        }
    }

    public class UserConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(User);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
                throw new Exception("Wrong token type");

            return User.GetUserByID((string)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is User)
                writer.WriteValue(((User)value).ID);
            else
                throw new Exception("Expected User object");
        }
    }

    public class ChannelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Channel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
                throw new Exception("Wrong token type");

            return Channel.GetChannelByID((string)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Channel)
                writer.WriteValue(((Channel)value).ID);
            else
                throw new Exception("Expected User object");
        }
    }

    public class PostReferenceConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PostReference);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
                throw new Exception("Wrong token type");

            string postReference = (string)reader.Value;

            if (!string.IsNullOrWhiteSpace(postReference))
                return new PostReference() { ID = postReference };
            else
                return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteValue("");
            else if (value is PostReference)
                writer.WriteValue(((PostReference)value).ID);
            else
                throw new Exception("Expected PostReference object or null.");
        }
    }
}
