using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using DSharpPlus.CommandsNext;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using TheCloud.config;
using TheCloud.Database;
using TheCloud.Logging;
using TheCloud.Utilities;

namespace TheCloud.Commands
{
    public class AdminCommands : ApplicationCommandModule
    {
        private static MongoImages _mongoImagesStatic;
        private const ulong ownerId = 334817817339625475;

        public static void SetMongoImages(MongoImages mongoImages)
        {
            _mongoImagesStatic = mongoImages;
        }

        private static JSONStructure discordConfigData;
        public static JSONStructure GetConfig() => discordConfigData;
        public static DiscordClient GetClient() => _client;
        private static DiscordClient _client;

        public static void SetClient(DiscordClient client)
        {
            _client = client;
        }

        public static void SetConfig(JSONStructure config)
        {
            discordConfigData = config;
        }

        private bool IsAuthorized(InteractionContext ctx) => ctx.User.Id == ownerId;

        private async Task AnnounceAsync(InteractionContext ctx, string action, TimeSpan delay, DateTime scheduledTime)
        {
            var roleMention = $"<@&{discordConfigData.CloudWatcherRoleID}>";
            var channel = await ctx.Client.GetChannelAsync(discordConfigData.AnnouncementChannelID);

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"⚠️ Bot {action} Notice")
                .WithDescription($"{roleMention}, Cloud is **{action.ToLower()}** in **{delay.TotalMinutes:F0} minute(s)**.\nScheduled for **{scheduledTime:HH:mm UTC}**.")
                .WithColor(action == "Restarting" ? DiscordColor.Orange : DiscordColor.Red)
                .WithTimestamp(DateTimeOffset.UtcNow);

            await channel.SendMessageAsync(new DiscordMessageBuilder()
                .WithContent(roleMention)
                .AddEmbed(embed));
        }

        private async Task AnnounceCancelAsync(InteractionContext ctx)
        {
            var roleMention = $"<@&{discordConfigData.CloudWatcherRoleID}>";
            var channel = await ctx.Client.GetChannelAsync(discordConfigData.AnnouncementChannelID);

            var embed = new DiscordEmbedBuilder()
                .WithTitle("❌ Scheduled Action Canceled")
                .WithDescription($"{roleMention}, the previously scheduled shutdown or restart has been **canceled**.")
                .WithColor(DiscordColor.Green)
                .WithTimestamp(DateTimeOffset.UtcNow);

            await channel.SendMessageAsync(new DiscordMessageBuilder()
                .WithContent(roleMention)
                .AddEmbed(embed));
        }

        // ✅ Public info command — accessible to all users
        [SlashCommand("info", "Provides information about Cloud")]
        public async Task InfoCommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle("About Cloud")
                .WithDescription("Cloud is a bot to assist you with your needs in Dark Galaxy! Use /help to see what I can do.")
                .WithColor(DiscordColor.Azure)
                .AddField("Version", "1.0.0", true)
                .AddField("Developer", "Rain", true)
                .AddField("Features", "Greetings, images of clouds, and more!", false)
                .AddField("Code", "Coded using C#", false)
                .AddField("Thanks", "Thanks to the Dark Galaxy Staff for testing me!", false)
                .AddField("Donate", "[Support Cloud](https://paypal.me/ericdavis213)", false)
                .WithFooter("Thank you for using Cloud!");

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("help", "Provides a list of available commands/responses available")]
        public async Task HelpCommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Cloud Help")
                .WithDescription("Here are the commands and responses you can use with me:")
                .WithColor(DiscordColor.Azure)
                .AddField("/info", "Provides information about Cloud", false)
                .AddField("/help", "Provides a list of available commands/responses available", false)
                .AddField("Greetings", "Say 'hey cloud' or 'hello cloud' to get a friendly greeting!", false)
                .AddField("Purple Moogle", "Mention 'Purple Moogle' to hear my thoughts about him!", false)
                .AddField("General Chat", "Chat with Cloud by asking how I'm doing!", false)
                .WithFooter("Thank you for using Cloud!");

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("shutdown", "Admin-only command to shut down the bot")]
        public async Task Shutdown(InteractionContext ctx,
     [Option("hours", "Hours until shutdown")] long hours = 0,
     [Option("minutes", "Minutes until shutdown")] long minutes = 0,
     [Option("seconds", "Seconds until shutdown")] long seconds = 0,
     [Option("instant", "Shutdown immediately")] bool instant = false)
        {
            if (!IsAuthorized(ctx))
            {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                    .WithContent("❌ You are not authorized to use this command.")
                    .AsEphemeral());
                return;
            }

            var totalDelay = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
            if (instant) totalDelay = TimeSpan.Zero;

            if (totalDelay.TotalSeconds <= 0 && !instant)
            {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                    .WithContent("❌ Please specify a delay greater than 0 or use instant.")
                    .AsEphemeral());
                return;
            }

            File.WriteAllText("shutdown.flag", totalDelay.TotalSeconds.ToString());
            await BotLogger.LogCommandAsync("shutdown", ctx.User.Username, ctx.User.Id);

            var shutdownTime = DateTime.UtcNow.Add(totalDelay);
            await AnnounceAsync(ctx, "Shutting Down", totalDelay, shutdownTime);

            string delayMessage = instant
                ? "⏱ Shutting down immediately..."
                : $"✅ Shutdown scheduled in {totalDelay.TotalSeconds:F0} second(s). Announcement sent.";

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                .WithContent(delayMessage)
                .AsEphemeral());

            await BotLogger.LogEventAsync($"Shutdown scheduled for {shutdownTime:u}");

            await Task.Delay(totalDelay);

            await ctx.Client.DisconnectAsync();
            Environment.Exit(0);
        }

        [SlashCommand("restart", "Admin-only command to restart the bot")]
        public async Task Restart(InteractionContext ctx,
    [Option("hours", "Hours until restart")] long hours = 0,
    [Option("minutes", "Minutes until restart")] long minutes = 0,
    [Option("seconds", "Seconds until restart")] long seconds = 0,
    [Option("instant", "Restart immediately")] bool instant = false)
        {
            if (!IsAuthorized(ctx))
            {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                    .WithContent("❌ You are not authorized to use this command.")
                    .AsEphemeral());
                return;
            }

            var totalDelay = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
            if (instant) totalDelay = TimeSpan.Zero;

            if (totalDelay.TotalSeconds <= 0 && !instant)
            {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                    .WithContent("❌ Please specify a delay greater than 0 or use instant.")
                    .AsEphemeral());
                return;
            }

            // Log restart request
            await BotLogger.LogCommandAsync("restart", ctx.User.Username, ctx.User.Id);
            var restartTime = DateTime.UtcNow.Add(totalDelay);

            // Announce the restart
            await AnnounceAsync(ctx, "Restarting", totalDelay, restartTime);

            string delayMessage = instant
                ? "⏱ Restarting immediately..."
                : $"✅ Restart scheduled in {totalDelay.TotalSeconds:F0} second(s). Announcement sent.";

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                .WithContent(delayMessage)
                .AsEphemeral());

            await BotLogger.LogEventAsync($"Restart scheduled for {restartTime:u}");

            // Wait for the scheduled delay
            if (totalDelay.TotalMilliseconds > 0)
            {
                await BotLogger.LogEventAsync($"Waiting {totalDelay.TotalSeconds:F0} second(s) before restarting.");
                await Task.Delay(totalDelay);
            }

            // Log disconnect
            await BotLogger.LogEventAsync("Disconnecting from Discord...");
            await ctx.Client.DisconnectAsync();

            // Log self-restart attempt
            await BotLogger.LogEventAsync("Attempting to restart the bot...");

            try
            {
                // Start a new process for the same bot executable
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true, // ensures it runs as a normal process
                        WorkingDirectory = Environment.CurrentDirectory
                    });
                    await BotLogger.LogEventAsync($"Successfully launched new bot process: {exePath}");
                }
                else
                {
                    await BotLogger.LogEventAsync("❌ Could not find executable path to restart.");
                }
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"❌ Failed to restart bot: {ex}");
            }

            // Exit current process
            await BotLogger.LogEventAsync("Exiting current bot process...");
            Environment.Exit(0);
        }

        [SlashCommand("selfupdate", "Pull latest code and relaunch bot")]
        public async Task SelfUpdateAsync(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync("🔄 Starting self-update...");
            await BotLogger.LogEventAsync("🧪 SelfUpdate: Starting update flow...");

            try
            {
                bool synced = await GitManager.ForceSyncRepoAsync();
                if (!synced)
                {
                    await BotLogger.LogEventAsync("❌ SelfUpdate: Git sync failed.");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("❌ Git sync failed."));
                    return;
                }

                string commitHash = await GitManager.GetLatestCommitHashAsync();
                await BotLogger.LogEventAsync($"🧪 SelfUpdate: Latest commit = {commitHash}");

                var (built, dllPath) = await GitManager.BuildProjectAsync();
                await BotLogger.LogEventAsync($"🧪 SelfUpdate: Build result = {built}, dllPath = {dllPath}");

                if (!built || string.IsNullOrEmpty(dllPath))
                {
                    await BotLogger.LogEventAsync("❌ SelfUpdate: Build failed.");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("❌ Build failed."));
                    return;
                }

                bool relaunched = await GitManager.RelaunchBotAsync(commitHash, dllPath);
                await BotLogger.LogEventAsync($"🧪 SelfUpdate: Relaunch result = {relaunched}");

                if (!relaunched)
                {
                    await BotLogger.LogEventAsync("❌ SelfUpdate: Relaunch failed.");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("❌ Relaunch failed."));
                    return;
                }

                await BotLogger.LogEventAsync("✅ SelfUpdate: Update complete.");
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("✅ Update complete. Relaunching..."));
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"❌ SelfUpdate: Exception occurred: {ex.Message}");
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("❌ Update crashed."));
            }
        }

        [SlashCommand("status", "Check if a shutdown or restart is scheduled")]
        public async Task Status(InteractionContext ctx)
        {
            if (!IsAuthorized(ctx))
            {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                    .WithContent("❌ You are not authorized to use this command.")
                    .AsEphemeral());
                return;
            }

            string status = "";

            if (File.Exists("shutdown.flag"))
            {
                string minutes = File.ReadAllText("shutdown.flag");
                status += $"🛑 Shutdown scheduled in {minutes} minute(s).\n";
            }

            if (File.Exists("restart.flag"))
            {
                string minutes = File.ReadAllText("restart.flag");
                status += $"🔄 Restart scheduled in {minutes} minute(s).\n";
            }

            if (string.IsNullOrEmpty(status))
                status = "✅ No shutdown or restart is currently scheduled.";

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                .WithContent(status)
                .AsEphemeral());
        }
        [SlashCommand("postimage", "Posts a random image from MongoDB (Admin only)")]
        public async Task PostImage(InteractionContext ctx)
        {
            if (!IsAuthorized(ctx))
            {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                    .WithContent("❌ You are not authorized to use this command.")
                    .AsEphemeral());
                return;
            }

            await BotLogger.LogCommandAsync("postimage", ctx.User.Username, ctx.User.Id);

            try
            {
                var (stream, fileName) = await _mongoImagesStatic.GetRandomImageAsync();

                if (stream == null || string.IsNullOrEmpty(fileName))
                {
                    await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                        .WithContent("⚠️ No images available in the database.")
                        .AsEphemeral());

                    await BotLogger.LogImagePostAsync("N/A", false, ctx.Channel.Name);
                    return;
                }

                var messageBuilder = new DiscordMessageBuilder()
                    .WithContent("Here’s a new image!")
                    .AddFile(fileName, stream);

                // Send to first channel (current server)
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                    .WithContent("📤 Posting image...")
                    .AsEphemeral());

                await ctx.Channel.SendMessageAsync(messageBuilder);

                // Send to second channel (other server)
                var secondChannel = await ctx.Client.GetChannelAsync(discordConfigData.CloudsChannelID);
                await secondChannel.SendMessageAsync(new DiscordMessageBuilder()
                    .WithContent("Here’s a new image!")
                    .AddFile(fileName, stream));

                stream.Dispose();

                await BotLogger.LogImagePostAsync(fileName, true, ctx.Channel.Name);
                await BotLogger.LogImagePostAsync(fileName, true, secondChannel.Name);
            }
            catch (Exception ex)
            {
                await BotLogger.LogImagePostAsync("N/A", false, ctx.Channel.Name);
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                    .WithContent($"❌ Failed to post image: {ex.Message}")
                    .AsEphemeral());
            }
        }

        [SlashCommand("updatephoto", "Upload all images from the local folder to MongoDB")]
        public async Task UpdatePhotoCommand(InteractionContext ctx)
        {
            if (!IsAuthorized(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("❌ You are not authorized to use this command.")
                        .AsEphemeral());
                return;
            }

            await BotLogger.LogCommandAsync("updatephoto", ctx.User.Username, ctx.User.Id);
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            try
            {
                var result = await _mongoImagesStatic.UploadImagesAsync("Images");
                await BotLogger.LogEventAsync($"Image upload triggered by {ctx.User.Username}: {result}");

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(result));
            }
            catch (Exception ex)
            {
                await BotLogger.LogEventAsync($"Image upload failed: {ex.Message}");
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"❌ Failed to upload images: {ex.Message}"));
            }
        }
    }
}



           