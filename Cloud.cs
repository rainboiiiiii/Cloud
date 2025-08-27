using dotenv.net;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheCloud.Commands;
using TheCloud.config;
using TheCloud.Database;
using TheCloud.Logging.BotLogger;
using TheCloud.UserCommands;
using TheCloud.Utilities;

namespace TheCloud
{
    internal class CloudProgram
    {
        private static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }
        private const bool ImagePostFailed = false;
        private static MongoImages _mongoImages;
        private static JSONStructure discordConfigData;

        public static ulong ticketChannelId = 123456789012345678; // Replace with your actual channel ID
        public static RateLimiter RateLimiter = new RateLimiter(TimeSpan.FromSeconds(3));

        static async Task Main(string[] args)
        {
            DotEnv.Load();

            Console.WriteLine("✅ .env loaded");
            Console.WriteLine($"CONFIG_KEY = {Environment.GetEnvironmentVariable("CONFIG_KEY")}");

            var configKey = Environment.GetEnvironmentVariable("CONFIG_KEY");
            if (string.IsNullOrEmpty(configKey))
            {
                Console.WriteLine("❌ CONFIG_KEY not found in .env");
                return;
            }

            if (!File.Exists("config.enc"))
            {
                Console.WriteLine("❌ Encrypted config.enc not found.");
                return;
            }

            var encryptor = new ConfigEncryptor();
            try
            {
                discordConfigData = encryptor.DecryptConfig("config.enc", configKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to decrypt config: {ex.Message}");
                return;
            }

            if (string.IsNullOrEmpty(discordConfigData.MONGO_URI))
            {
                Console.WriteLine("❌ MONGO_URI not found in decrypted config. MongoDB features disabled.");
                _mongoImages = null;
            }
            else
            {
                try
                {
                    _mongoImages = new MongoImages(discordConfigData.MONGO_URI, discordConfigData.MONGO_DB);
                    AdminCommands.SetMongoImages(_mongoImages);

                    // 🔹 Initialize logger
                    BotLoggerV2.Initialize(discordConfigData.MONGO_URI, discordConfigData.MONGO_DB);
                    await BotLoggerV2.LogEventAsync("Startup", "✅ MongoDB logger initialized.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to initialize Mongo services: {ex.Message}");
                    _mongoImages = null;
                }
            }

            Console.WriteLine($"🔍 MONGO_URI: {discordConfigData.MONGO_URI}");
            Console.WriteLine($"🔍 ChannelID: {discordConfigData.ChannelID}");
            Console.WriteLine($"🔍 Prefix: {discordConfigData.prefix}");

            var discordConfig = new DiscordConfiguration
            {
                Token = discordConfigData.token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All | DiscordIntents.GuildMembers | DiscordIntents.MessageContents,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);
            AdminCommands.SetConfig(discordConfigData);

            var slash = Client.UseSlashCommands();
            slash.RegisterCommands<AdminCommands>(1139654090763276378);
            slash.RegisterCommands<AdminCommands>(1408428084763299880);
            await BotLoggerV2.LogEventAsync("✅ Slash commands registered: Testcommands");

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { discordConfigData.prefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                EnableDefaultHelp = true
            };

            Client.Ready += async (sender, e) =>
            {
                await BotLoggerV2.LogEventAsync("Bot is online and ready.");
            };

            var timer = new System.Timers.Timer(TimeSpan.FromHours(3).TotalMilliseconds);
            timer.Elapsed += async (sender, e) =>
          
            timer.AutoReset = true;
            timer.Start();

            if (File.Exists("restart.flag"))
            {
                File.Delete("restart.flag");
                await BotLoggerV2.LogEventAsync("Bot restarted successfully.");
                var restartChannel = await Client.GetChannelAsync(discordConfigData.ChannelID);
                await restartChannel.SendMessageAsync("✅ Cloud restarted successfully.");
            }

            if (File.Exists("shutdown.flag"))
            {
                File.Delete("shutdown.flag");
                await BotLoggerV2.LogEventAsync("Bot shutdown and restarted.");
                var shutdownChannel = await Client.GetChannelAsync(discordConfigData.ChannelID);
                await shutdownChannel.SendMessageAsync("🛑 Cloud shut down and restarted.");
            }

            await Client.ConnectAsync();
           
            await Task.Delay(-1);
        }

        public static async Task AnnounceStartupAsync()
        {
            var client = AdminCommands.GetClient();
            var config = AdminCommands.GetConfig();

            if (client == null || config == null) return;

            try
            {
                var channel = await client.GetChannelAsync(config.AnnouncementChannelID);
                var version = await GitManager.GetLatestCommitHashAsync();
                await channel.SendMessageAsync($"✅ Cloud is now running.\nVersion: `{version}`");
            }
            catch (Exception ex)
            {
                await BotLoggerV2.LogEventAsync($"⚠️ CloudProgram: Failed to announce startup: {ex.Message}");
            }
        }

        private static async Task PostRandomImageAsync()
        {
            if (_mongoImages == null)
            {
                Console.WriteLine("⚠️ Oh no! MongoImages not initialized. Skipping image post.");
                return;
            }

            try
            {
                var (imageBytes, fileName) = await _mongoImages.GetRandomImageBytesAsync();
                if (imageBytes == null || string.IsNullOrEmpty(fileName))
                {
                    Console.WriteLine("⚠️ No image found in MongoDB.");
                    await BotLoggerV2.LogEventAsync("ImagePost", "No image found.");
                    return;
                }

                var primaryChannel = await Client.GetChannelAsync(discordConfigData.CloudsChannelID);
                var secondaryChannel = await Client.GetChannelAsync(discordConfigData.ChannelID);

                using var stream1 = new MemoryStream(imageBytes);
                await primaryChannel.SendMessageAsync(new DiscordMessageBuilder()
                    .WithContent("Here’s a new image!")
                    .AddFile(fileName, stream1));

                await BotLoggerV2.LogImagePostAsync(fileName, true, primaryChannel.Name);

                using var stream2 = new MemoryStream(imageBytes);
                await secondaryChannel.SendMessageAsync(new DiscordMessageBuilder()
                    .WithContent("Here’s a new image!")
                    .AddFile(fileName, stream2));

                await BotLoggerV2.LogImagePostAsync(fileName, true, secondaryChannel.Name);
            }
            catch (Exception ex)
            {
                await BotLoggerV2.LogImagePostAsync("N/A", false);
                Console.WriteLine($"❌ Failed to post image: {ex.Message}");
            }
        }
    }
}