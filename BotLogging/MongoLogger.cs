using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace TheCloud.Logging.BotLogger
{
    public static class MongoLogger
    {
        private static IMongoCollection<BsonDocument> _logCollection;

        public static void Initialize(string mongoUri, string dbName)
        {
            var client = new MongoClient(mongoUri);
            var database = client.GetDatabase(dbName);
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
    }
}