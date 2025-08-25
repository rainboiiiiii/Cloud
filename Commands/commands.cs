using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Interactivity.Extensions;

namespace TheCloud.UserCommands
{
    public class Testcommands : ApplicationCommandModule
    {
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
            var embed = new DiscordEmbedBuilder
            {
                Title = "Cloud Help",
                Description = "Here are the commands and responses you can use with me:",
                Color = DiscordColor.Azure
            };
            embed.AddField("/info", "Provides information about Cloud", false);
            embed.AddField("/help", "Provides a list of available commands/responses available", false);
            embed.AddField("Greetings", "Say 'hey cloud' or 'hello cloud' to get a friendly greeting!", false);
            embed.AddField("Purple Moogle", "Mention 'Purple Moogle' to hear my thoughts about him!", false);
            embed.AddField("General Chat", "Chat with Cloud by asking how I'm doing!", false);
            embed.WithFooter("Thank you for using Cloud!");

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed));

        }
    }
}