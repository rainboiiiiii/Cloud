using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TheCloud.Logging
{
    public class ConversationLog
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }

        [BsonElement("guildId")]
        public ulong GuildId { get; set; }

        [BsonElement("channelId")]
        public ulong ChannelId { get; set; }

        [BsonElement("userId")]
        public ulong UserId { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("userMessage")]
        public string UserMessage { get; set; }

        [BsonElement("botResponse")]
        public string BotResponse { get; set; }
    }
}