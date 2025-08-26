using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

public class MongoDbLogger : ILogger
{
    private readonly IMongoCollection<BsonDocument> _logCollection;
    private readonly string _categoryName;

    public MongoDbLogger(IMongoDatabase db, string categoryName)
    {
        _logCollection = db.GetCollection<BsonDocument>("BotLogs");
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        var message = formatter(state, exception);

        var doc = new BsonDocument
        {
            { "Category", _categoryName },
            { "Level", logLevel.ToString() },
            { "Message", message },
            { "Exception", exception?.ToString() ?? "" },
            { "Timestamp", DateTime.UtcNow }
        };

        _logCollection.InsertOne(doc);
    }
}