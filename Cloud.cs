using dotenv.net;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.IO;
using System.Threading.Tasks;
using TheCloud.Commands;
using TheCloud.config;
using TheCloud.Database;
using TheCloud.Logging;

namespace TheCloud
{
    internal class Program
    {
        private static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }
        private static MongoImages _mongoImages;
        private static JSONStructure discordConfigData;
        

        public static RateLimiter RateLimiter = new RateLimiter(TimeSpan.FromSeconds(3));

        static async Task Main(string[] args)
        {
            DotEnv.Load();

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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to initialize MongoImages: {ex.Message}");
                    _mongoImages = null;
                }
            }

            Console.WriteLine($"🔍 MONGO_URI: {discordConfigData.MONGO_URI}");
            Console.WriteLine($"🔍 ChannelID: {discordConfigData.ChannelID}");
            Console.WriteLine($"🔍 Prefix: {discordConfigData.prefix}");
         
            var discordConfig = new DiscordConfiguration()
            {
                Token = discordConfigData.token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All | DiscordIntents.GuildMembers,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);
            AdminCommands.SetConfig(discordConfigData);

            var slash = Client.UseSlashCommands();
            slash.RegisterCommands<AdminCommands>();

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { discordConfigData.prefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                EnableDefaultHelp = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Client.Ready += async (sender, e) =>
            {
                await BotLogger.LogEventAsync("Bot is online and ready.");
                await PostRandomImageAsync();
            };

            Client.MessageCreated += async (client, e) =>
            {
                if (e.Author.IsBot) return;
                if (e.Message.Content == "hello Cloud")
                {
                    await e.Message.RespondAsync($"Hi there, {e.Author.Username}!");
                }
            };

            var timer = new System.Timers.Timer(TimeSpan.FromHours(3).TotalMilliseconds);
            timer.Elapsed += async (sender, e) =>
            {
                await PostRandomImageAsync();
            };
            timer.AutoReset = true;
            timer.Start();

            if (File.Exists("restart.flag"))
            {
                File.Delete("restart.flag");
                await BotLogger.LogEventAsync("Bot restarted successfully.");
                await (await Client.GetChannelAsync(discordConfigData.ChannelID))
                    .SendMessageAsync("✅ Cloud restarted successfully.");
            }

            if (File.Exists("shutdown.flag"))
            {
                File.Delete("shutdown.flag");
                await BotLogger.LogEventAsync("Bot shutdown and restarted.");
                await (await Client.GetChannelAsync(discordConfigData.ChannelID))
                    .SendMessageAsync("🛑 Cloud shut down and restarted.");
            }

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task PostRandomImageAsync()
        {
            if (_mongoImages == null)
            {
                Console.WriteLine("⚠️ MongoImages not initialized. Skipping image post.");
                return;
            }

            try
            {
                var (stream, fileName) = await _mongoImages.GetRandomImageAsync();
                ulong channelId = ulong.Parse(discordConfigData.ImageChannelID);
                var channel = await Client.GetChannelAsync(channelId);

                var messageBuilder = new DiscordMessageBuilder()
                    .WithContent("Here’s a new image!")
                    .AddFile(fileName, stream);

                await channel.SendMessageAsync(messageBuilder);
                stream.Dispose();

                await BotLogger.LogImagePostAsync(fileName, true, channel.Name);
            }
            catch (Exception ex)
            {
                await BotLogger.LogImagePostAsync("N/A", false);
                Console.WriteLine($"❌ Failed to post image: {ex.Message}");
            }
        }
    }
}