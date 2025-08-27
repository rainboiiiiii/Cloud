using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

public class MongoLogger
{
    private readonly IMongoCollection<BsonDocument> _collection;
    private string mONGO_URI;
    private string mONGO_DB;
    private string v;

    public MongoLogger(string mONGO_URI, IMongoDatabase db)
    {
        _collection = db.GetCollection<BsonDocument>("BotLogs");
    }

    public MongoLogger(string mONGO_URI, string mONGO_DB, string v)
    {
        this.mONGO_URI = mONGO_URI;
        this.mONGO_DB = mONGO_DB;
        this.v = v;
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

    internal async Task LogEventAsync(string v, bool imagePostFailed, string name)
    {
        throw new NotImplementedException();
    }

    internal async Task LogEventAsync(string v, bool imagePostFailed)
    {
        throw new NotImplementedException();
    }

    internal async Task LogEventAsync(string v)
    {
        throw new NotImplementedException();
    }

    internal async Task LogImagePostAsync(string fileName, bool v, string name)
    {
        throw new NotImplementedException();
    }

    internal async Task LogImagePostAsync(string v1, bool v2)
    {
        throw new NotImplementedException();
    }
}