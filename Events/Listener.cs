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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TheCloud
{
    public class MessageHandler
    {
        public async Task OnMessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot)
                return;

            if (!Program.RateLimiter.CanRespond(e.Author.Id))
                return;

            string userInput = e.Message.Content.ToLower().Trim();
            var random = new Random();

            // ✅ Get server nickname (DisplayName)
            var member = await e.Guild.GetMemberAsync(e.Author.Id);
            string displayName = member.DisplayName;

            // Purple Moogle responses
            var purpleMoogleTriggers = new List<string> { "purple moogle", "purplemoogle" };
            var bestPurpleMoogle = Process.ExtractOne(userInput, purpleMoogleTriggers);

            if (bestPurpleMoogle != null && bestPurpleMoogle.Score > 80)
            {
                var responses = BotResponses.PurpleMoogleResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );
            }

            var inquiries2 = new List<string> { "can I get the new bp for free?" };
            var bestInquiry2 = Process.ExtractOne(userInput, inquiries2);

            if (bestInquiry2 != null && bestInquiry2.Score > 70)
            {
                var responses = BotResponses.FreeBPResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );
            }

            var inquiries11 = new List<string> { "can I get free GP?" };
            var bestInquiry11 = Process.ExtractOne(userInput, inquiries11);

            if (bestInquiry11 != null && bestInquiry11.Score > 93)
            {
                var responses = BotResponses.FreeGPResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );
            }

            var inquiries3 = new List<string> { "what is a kilometer" };
            var bestInquiry3 = Process.ExtractOne(userInput, inquiries3);

            if (bestInquiry3 != null && bestInquiry3.Score > 70)
            {
                var responses = BotResponses.KilometerResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );

            }

            var inquiries4 = new List<string> { "what can you do, cloud?" };
            var bestInquiry4 = Process.ExtractOne(userInput, inquiries4);

            if (bestInquiry4 != null && bestInquiry4.Score > 85)
            {
                var responses = BotResponses.WhatCanCloudDoResponse(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );

            }

            var inquiries5 = new List<string> { "I need help, cloud" };
            var bestInquiry5 = Process.ExtractOne(userInput, inquiries5);

            if (bestInquiry5 != null && bestInquiry5.Score > 85)
            {
                var responses = BotResponses.HelpResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );

            }

            var inquiries6 = new List<string> { "are you friends with Hanni, cloud?" };
            var bestInquiry6 = Process.ExtractOne(userInput, inquiries6);

            if (bestInquiry6 != null && bestInquiry6.Score > 80)
            {
                var responses = BotResponses.HanniResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );

            }

            var inquiries7 = new List<string> { "when is the next maint, cloud?" };
            var bestInquiry7 = Process.ExtractOne(userInput, inquiries7);

            if (bestInquiry7 != null && bestInquiry7.Score > 100)
            {
                var responses = BotResponses.MaintResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );

            }

            var inquiries8 = new List<string> { "i want to help you, cloud" };
            var bestInquiry8 = Process.ExtractOne(userInput, inquiries8);

            if (bestInquiry8 != null && bestInquiry8.Score > 85)
            {
                var responses = BotResponses.CloudHelpResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );

            }

            var inquiries9 = new List<string> { "i want free stuff, cloud" };
            var bestInquiry9 = Process.ExtractOne(userInput, inquiries9);

            if (bestInquiry9 != null && bestInquiry9.Score > 85)
            {
                var responses = BotResponses.FreeStuffResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );

            }

            var inquiries10 = new List<string> { "what do you think of rain, cloud?" };
            var bestInquiry10 = Process.ExtractOne(userInput, inquiries10);

            if (bestInquiry10 != null && bestInquiry10.Score > 85)
            {
                var responses = BotResponses.OwnerResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);
                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );
            }

            // Cloud keyword greetings
            var keywords = new List<string> { "cloud bot" };
            bool keywordFound = keywords.Any(k => userInput.Contains(k));

            if (!keywordFound)
                return;

            var greetings = new List<string> { "cloud bot" };
            var bestGreeting = Process.ExtractOne(userInput, greetings);

            if (bestGreeting != null && bestGreeting.Score > 90)
            {
                var responses = BotResponses.GreetingResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );
                return;
            }

            // Inquiry responses
            var inquiries = new List<string> { "how are you cloud bot" };
            var bestInquiry = Process.ExtractOne(userInput, inquiries);

            if (bestInquiry != null && bestInquiry.Score > 90)
            {
                var responses = BotResponses.InquiryResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);

                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );
            }

            var farewells = new List<string> { "bye cloud bot", "goodbye cloud bot" };

            var bestFarewell = Process.ExtractOne(userInput, farewells);
            if (bestFarewell != null && bestFarewell.Score > 90)
            {
                var responses = BotResponses.ByeResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);
                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );
            }

            var thanks = new List<string> { "thanks cloud bot", "thank you cloud bot" };

            var bestThanks = Process.ExtractOne(userInput, thanks);
            if (bestThanks != null && bestThanks.Score > 90)
            {
                var responses = BotResponses.ThanksResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);
                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );
            }

            var sorrys = new List<string> { "sorry cloud bot", "i'm sorry cloud bot" };
            var bestSorry = Process.ExtractOne(userInput, sorrys);

            if (bestSorry != null && bestSorry.Score > 90)
            {
                var responses = BotResponses.SorryResponses(displayName);
                string selected = responses[random.Next(responses.Count)];
                await e.Message.RespondAsync(selected);
                await BotLogger.LogConversationAsync(
                    e.Guild.Id,
                    e.Channel.Id,
                    e.Author.Id,
                    displayName,
                    e.Message.Content,
                    selected
                );






            }
        }
    }
}