using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TheCloud.Logging
{
    public static class BotLogger
    {
        private static IMongoCollection<BsonDocument> _logCollection;
        private static string _logFilePath = Path.Combine(AppContext.BaseDirectory, "bot_logs.txt");

        static BotLogger()
        {
            try
            {
                // Load MongoDB URI from environment
                var mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");
                if (!string.IsNullOrEmpty(mongoUri))
                {
                    var client = new MongoClient(mongoUri);
                    var database = client.GetDatabase("MyGameDB");
                    _logCollection = database.GetCollection<BsonDocument>("BotLogs");
                }
                else
                {
                    Console.WriteLine("⚠️ MONGO_URI not set. MongoDB logging disabled.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to initialize MongoDB logging: {ex.Message}");
            }
        }

        // -----------------------------
        // Log a command execution
        // -----------------------------
        public static async Task LogCommandAsync(string commandName, string username, ulong userId, string channelName = null)
        {
            string msg = $"[{DateTime.UtcNow:u}] Command executed: {commandName} by {username} ({userId}) in {channelName ?? "N/A"}";
            Console.WriteLine(msg);
            await File.AppendAllTextAsync(_logFilePath, msg + Environment.NewLine);

            if (_logCollection != null)
            {
                var doc = new BsonDocument
                {
                    ["Timestamp"] = DateTime.UtcNow,
                    ["Type"] = "Command",
                    ["CommandName"] = commandName,
                    ["User"] = username,
                    ["UserId"] = (long)userId,
                    ["Channel"] = channelName ?? "N/A",
                    ["Success"] = true
                };

                await _logCollection.InsertOneAsync(doc);
            }
        }

        // -----------------------------
        // Log an image post
        // -----------------------------
        public static async Task LogImagePostAsync(string fileName, bool success, string channelName = null)
        {
            string msg = $"[{DateTime.UtcNow:u}] Image post {(success ? "SUCCESS" : "FAILED")}: {fileName} in {channelName ?? "N/A"}";
            Console.WriteLine(msg);
            await File.AppendAllTextAsync(_logFilePath, msg + Environment.NewLine);

            if (_logCollection != null)
            {
                var doc = new BsonDocument
                {
                    ["Timestamp"] = DateTime.UtcNow,
                    ["Type"] = "ImagePost",
                    ["FileName"] = fileName,
                    ["Channel"] = channelName ?? "N/A",
                    ["Success"] = success
                };

                await _logCollection.InsertOneAsync(doc);
            }
        }

        // -----------------------------
        // Log a generic bot event
        // -----------------------------
        public static async Task LogEventAsync(string eventDescription)
        {
            string msg = $"[{DateTime.UtcNow:u}] Event: {eventDescription}";
            Console.WriteLine(msg);
            await File.AppendAllTextAsync(_logFilePath, msg + Environment.NewLine);

            if (_logCollection != null)
            {
                var doc = new BsonDocument
                {
                    ["Timestamp"] = DateTime.UtcNow,
                    ["Type"] = "Event",
                    ["Description"] = eventDescription
                };

                await _logCollection.InsertOneAsync(doc);
            }
        }


        private static IMongoCollection<ConversationLog> _conversationCollection;

        public static void Initialize(string mONGO_URI, IMongoDatabase db)
        {
            _conversationCollection = db.GetCollection<ConversationLog>("Conversations");
        }

        public static async Task LogConversationAsync(ulong guildId, ulong channelId, ulong userId, string username, string userMessage, string botResponse)
        {
            if (_conversationCollection == null)
            {
                Console.WriteLine("⚠️ MongoDB conversation logging not initialized.");
                return;
            }

            var log = new ConversationLog
            {
                Timestamp = DateTime.UtcNow,
                GuildId = guildId,
                ChannelId = channelId,
                UserId = userId,
                Username = username,
                UserMessage = userMessage,
                BotResponse = botResponse
            };

            await _conversationCollection.InsertOneAsync(log);
            Console.WriteLine($"💬 Logged conversation: {username} said '{userMessage}' → Bot replied '{botResponse}'");
        }
    }
}
        

    