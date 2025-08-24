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

        public static void SetConfig(JSONStructure config)
        {
            discordConfigData = config;
        }

        private bool IsAuthorized(InteractionContext ctx) => ctx.User.Id == ownerId;

        // 🔔 Announcement helper
        private async Task AnnounceAsync(InteractionContext ctx, string action, int minutes)
        {
            var roleMention = $"<@&{discordConfigData.CloudWatcherRoleID}>";
            var channel = await ctx.Client.GetChannelAsync(discordConfigData.AnnouncementChannelID);

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"⚠️ Bot {action} Notice")
                .WithDescription($"{roleMention}, Cloud is **{action.ToLower()}** in **{minutes} minute(s)**.\nPlease prepare accordingly.")
                .WithColor(action == "Restarting" ? DiscordColor.Orange : DiscordColor.Red)
                .WithTimestamp(DateTimeOffset.UtcNow);

            await channel.SendMessageAsync(new DiscordMessageBuilder()
                .WithContent(roleMention) // ensures the role is pinged
                .AddEmbed(embed));
        }

        [SlashCommand("shutdown", "Admin-only command to shut down the bot")]
        public async Task Shutdown(InteractionContext ctx)
        {
            if (!IsAuthorized(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("❌ You are not authorized to use this command.")
                        .AsEphemeral());
                return;
            }

            await BotLogger.LogCommandAsync("shutdown", ctx.User.Username, ctx.User.Id);
            File.WriteAllText("shutdown.flag", "true");

            await AnnounceAsync(ctx, "Shutting Down", 1);

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                .WithContent("✅ Shutdown initiated. Announcement sent.")
                .AsEphemeral());

            await BotLogger.LogEventAsync("Shutdown command issued.");
            await ctx.Client.DisconnectAsync();
            Environment.Exit(0);
        }

        [SlashCommand("restart", "Admin-only command to restart the bot")]
        public async Task Restart(InteractionContext ctx)
        {
            if (!IsAuthorized(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("❌ You are not authorized to use this command.")
                        .AsEphemeral());
                return;
            }

            await BotLogger.LogCommandAsync("restart", ctx.User.Username, ctx.User.Id);
            File.WriteAllText("restart.flag", "true");

            await AnnounceAsync(ctx, "Restarting", 1);

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                .WithContent("✅ Restart initiated. Announcement sent.")
                .AsEphemeral());

            await BotLogger.LogEventAsync("Restart command issued.");

            try
            {
                string exePath = Assembly.GetEntryAssembly().Location;

                Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{exePath}\"",
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(exePath)
                });

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                    .WithContent($"❌ Failed to restart: {ex.Message}")
                    .AsEphemeral());
            }
        }

        [SlashCommand("postimage", "Posts a random image from MongoDB (Admin only)")]
        public async Task PostImage(InteractionContext ctx)
        {
            if (!IsAuthorized(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
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

                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                    .WithContent("📤 Posting image...")
                    .AsEphemeral());

                await ctx.Channel.SendMessageAsync(messageBuilder);
                stream.Dispose();

                await BotLogger.LogImagePostAsync(fileName, true, ctx.Channel.Name);
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