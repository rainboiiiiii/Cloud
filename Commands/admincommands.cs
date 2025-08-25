using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
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


        [SlashCommand("selfupdate", "Pull latest code, build, and relaunch the bot")]
        public async Task SelfUpdateAsync(InteractionContext ctx)
        {
            // ✅ Match shutdown/cancel permission style
            if (!IsAuthorized(ctx))
            {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                    .WithContent("❌ You are not authorized to use this command.")
                    .AsEphemeral());
                return;
            }

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                .WithContent("🔄 Cloud is updating...")
                .AsEphemeral());

            bool pulled = await GitManager.ForceSyncRepoAsync();
            if (!pulled)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("❌ Git pull failed."));
                return;
            }

            var (built, tempDllPath) = await GitManager.BuildProjectAsync();
            if (!built || string.IsNullOrWhiteSpace(tempDllPath))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("❌ Build failed."));
                return;
            }

            string hash = await GitManager.GetLatestCommitHashAsync();

            bool relaunched = await GitManager.RelaunchBotAsync(hash, tempDllPath);
            if (!relaunched)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("❌ Relaunch failed."));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("✅ Cloud is restarting..."));
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