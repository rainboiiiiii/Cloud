using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCloud.UserCommands
{
    public class Testcommands : ApplicationCommandModule
    {
        internal static object listeners;

        [SlashCommand("info", "Provides information about Cloud")]

        public async Task InfoCommand(InteractionContext ctx)
        {
            var embed = new DSharpPlus.Entities.DiscordEmbedBuilder
            {
                Title = "About Cloud",
                Description = "Cloud is a bot to assist you with you needs in Dark Galaxy! If you are trying to figure out what it is I can do and how to invoke a response, use my help command!",
                Color = DSharpPlus.Entities.DiscordColor.Azure
            };
            embed.AddField("Version", "1.0.0", true);
            embed.AddField("Developer", "Rain", true);
            embed.AddField("Features", "Greetings, images of clouds, and more!", false);
            embed.AddField("Code", "Coded using C#", false);
            embed.AddField("Thanks","Thanks to the Dark Galaxy Staff for testing me!", false);
            embed.AddField("Donate", "[Support Cloud](https://paypal.me/ericdavis213)", false);
            embed.WithFooter("Thank you for using Cloud!");

            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                new DSharpPlus.DiscordWebBuilder().AddEmbed(embed));
        }

        [SlashCommand("help", "Provides a list of available commands/responses available")]

        public async Task HelpCommand(InteractionContext ctx)
        {
            var embed = new DSharpPlus.Entities.DiscordEmbedBuilder
            {
                Title = "Cloud Help",
                Description = "Here are the commands and responses you can use with me:",
                Color = DSharpPlus.Entities.DiscordColor.Azure
            };
            embed.AddField("/info", "Provides information about Cloud", false);
            embed.AddField("/help", "Provides a list of available commands/responses available", false);
            embed.AddField("Greetings", "Say 'hey cloud' or 'hello cloud' to get a friendly greeting!", false);
            embed.AddField("Purple Moogle", "Mention 'Purple Moogle' to hear my thoughts about him!", false);
            embed.AddField("General Chat", "Chat with Cloud by asking how I'm doing!", false);
            embed.WithFooter("Thank you for using Cloud!");
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                new DSharpPlus.DiscordWebBuilder().AddEmbed(embed));
        }
    }
}
