using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace TheCloud.Logging.BotLogger
{
    public static class BotLoggerV2
    {
        private static IMongoCollection<BsonDocument> _logCollection;

        public static void Initialize(string MONGO_URI, string MONGO_DB)
        {
            var client = new MongoClient(MONGO_URI);
            var database = client.GetDatabase(MONGO_DB);
            _logCollection = database.GetCollection<BsonDocument>("cloud_logs");
        }

        public static async Task LogEventAsync(string message, string type = "event")
        {
            if (_logCollection == null) return;

            var doc = new BsonDocument
            {
                { "timestamp", DateTime.UtcNow },
                { "type", type },
                { "message", message }
            };

            await _logCollection.InsertOneAsync(doc);
        }

        public static async Task LogImagePostAsync(string fileName, bool success, string channelName = "N/A")
        {
            if (_logCollection == null) return;

            var doc = new BsonDocument
            {
                { "timestamp", DateTime.UtcNow },
                { "type", "image_post" },
                { "fileName", fileName },
                { "success", success },
                { "channel", channelName }
            };

            await _logCollection.InsertOneAsync(doc);
        }

        public static async Task LogConversationAsync(
            ulong userId,
            string username,
            string message,
            string context = "general",
            ulong channelId = 0,
            ulong guildId = 0)
        {
            if (_logCollection == null) return;

            var doc = new BsonDocument
            {
                { "timestamp", DateTime.UtcNow },
                { "type", "conversation" },
                { "userId", BsonValue.Create(userId) },
                { "username", username },
                { "message", message },
                { "context", context },
                { "channelId", BsonValue.Create(channelId) },
                { "guildId", BsonValue.Create(guildId) }
            };

            await _logCollection.InsertOneAsync(doc);
        }

        public static async Task LogCommandAsync(
    string commandName,
    string username,
    ulong userId,
    string context = "slash_command",
    ulong channelId = 0,
    ulong guildId = 0)
        {
            if (_logCollection == null) return;

            var doc = new BsonDocument
    {
        { "timestamp", DateTime.UtcNow },
        { "type", "command" },
        { "commandName", commandName },
        { "username", username },
        { "userId", BsonValue.Create(userId) },
        { "context", context },
        { "channelId", BsonValue.Create(channelId) },
        { "guildId", BsonValue.Create(guildId) }
    };

            await _logCollection.InsertOneAsync(doc);
        }
    }
}
  