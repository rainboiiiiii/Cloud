using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCloud.Commands
{
    public class Testcommands : BaseCommandModule
    {
        internal static object listeners;

        [Command("test")]
        public async Task TestCommand(CommandContext ctx)
        {
            await ctx.RespondAsync("This is a test command!");
        }
        [Command("ping")]
        public async Task PingCommand(CommandContext ctx)
        {
            await ctx.RespondAsync("Pong!");
        }
        [Command("echo")]
        public async Task EchoCommand(CommandContext ctx, [RemainingText] string message)
        {
            await ctx.RespondAsync(message);
        }

    }
}
