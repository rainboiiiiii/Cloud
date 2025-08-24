using Discord;
using DSharpPlus;
using DSharpPlus.EventArgs;
using FuzzySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheCloud;
using TheCloud.Listener;
using TheCloud.Logging;

namespace TheCloud
{
    public class MessageHandler
    {
        public async Task OnMessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot)
                return;

            // Access the rate limiter properly as public static
            if (!Program.RateLimiter.CanRespond(e.Author.Id))
                return;

            string userInput = e.Message.Content.ToLower().Trim();
            var random = new Random();

            // Purple Moogle responses
            var purpleMoogleTriggers = new List<string> { "purple moogle", "purplemoogle" };
            var bestPurpleMoogle = Process.ExtractOne(userInput, purpleMoogleTriggers);

            if (bestPurpleMoogle != null && bestPurpleMoogle.Score > 80)
            {
                var responses = BotResponses.PurpleMoogleResponses(e.Author.Username);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    e.Author.Username,
                    e.Message.Content,
                    selected
                );
            }

            // Cloud keyword greetings
            var keywords = new List<string> { "cloud" };
            bool keywordFound = keywords.Any(k => userInput.Contains(k));

            if (!keywordFound)
                return;

            var greetings = new List<string> { "hey cloud", "hello cloud" };
            var bestGreeting = Process.ExtractOne(userInput, greetings);

            if (bestGreeting != null && bestGreeting.Score > 85)
            {
                var responses = BotResponses.GreetingResponses(e.Author.Username);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    e.Author.Username,
                    e.Message.Content,
                    selected
                );
                return;
            }

            // Inquiry responses
            var inquiries = new List<string> { "how are you cloud" };
            var bestInquiry = Process.ExtractOne(userInput, inquiries);

            if (bestInquiry != null && bestInquiry.Score > 85)
            {
                var responses = BotResponses.InquiryResponses(e.Author.Username);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    e.Author.Username,
                    e.Message.Content,
                    selected
                );
            }
        }
    }
}