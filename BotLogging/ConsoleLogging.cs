using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create logger factory (console for simplicity)
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug); // Show debug + errors
        });

        // Configure MongoClient with logging
        var settings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
        settings.LoggingSettings = new LoggingSettings(loggerFactory);

        var client = new MongoClient(settings);
        var db = client.GetDatabase("DiscordBot");

        var collection = db.GetCollection<dynamic>("Logs");

        // Example insert
        await collection.InsertOneAsync(new { Message = "Hello from bot", Timestamp = DateTime.UtcNow });

        Console.WriteLine("Insert complete. Check logs above.");
    }
}