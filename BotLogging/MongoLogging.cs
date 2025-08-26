using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

public class MongoLogger
{
    private readonly IMongoCollection<BsonDocument> _collection;

    public MongoLogger(IMongoDatabase db)
    {
        _collection = db.GetCollection<BsonDocument>("BotLogs");
    }

    public async Task LogEventAsync(string type, string message)
    {
        var doc = new BsonDocument
        {
            { "Type", type },
            { "Message", message },
            { "Timestamp", DateTime.UtcNow }
        };

        await _collection.InsertOneAsync(doc);
        Console.WriteLine($"[MongoLog] {type}: {message}");
    }
}