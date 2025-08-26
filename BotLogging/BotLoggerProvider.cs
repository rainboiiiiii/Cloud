using Microsoft.Extensions.Logging;
using MongoDB.Driver;

public class MongoDbLoggerProvider : ILoggerProvider
{
    private readonly IMongoDatabase _db;

    public MongoDbLoggerProvider(IMongoDatabase db)
    {
        _db = db;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new MongoDbLogger(_db, categoryName);
    }

    public void Dispose() { }
}