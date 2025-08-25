using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCloud.Listener
{
    public static class BotResponses
    {


        public static List<string> GreetingResponses(string username) => new()
        {
            $"Hey there, {username}!",
            $"Hello, {username}! 👋",
            $"Hiya {username}, how's it going?",
            $"Greetings, {username}!",
            $"Yo {username}, what's up?",
            $"Hey {username}! Ready to chat?",
            $"Hi {username}! How can I assist you today?",
            $"Hello {username}! Hope you're having a great day!",
            $"Hey {username}! What's new with you?",
            $"Hi {username}! Always a pleasure to see you!",
            $"Hey {username}! Let's make today awesome!",
            $"Hello {username}! What adventures await us today?",
            $"What's up, {username}!",
            $"Hey {username}! Long time no see!",
            $"Hi {username}! Ready to rock and roll?"

        };

        public static List<string> InquiryResponses(string username) => new()
        {
            $"I'm feeling funky, thanks for asking {username}!",
            $"Doing great — just vibing in the clouds ☁️",
            $"Still in development, but emotionally stable 😄",
            $"I'm good! How about you, {username}?",
            $"Running at 99.9% uptime and 100% sass."
        };

        public static List<string> PurpleMoogleResponses(string username) => new()
        {
            $"Purple Moogle is a cunt.",
            $"Did someone summon the Purple Moogle? Absolutely not...",
            $"Purple Moogle is smelly.",
            $"If anyone was the Dad of phobias, it'd be PurpleMoogle.",
            $"Feeling magical today, especially since PurpleMoogle ain't here!",
            $"Purple Moogle? More like Purple No-go!",
            $"Purple Moogle is a legend in his own mind!",
            $"I heard Purple Moogle tried to join a circus once... they said no.",
            $"Purple Moogle's spirit animal is a confused llama!",
            $"If you see Purple Moogle, run the other way.",
            $"Purple Moogle is proof that evolution can go in reverse.",
            $"Purple Moogle's favorite hobby is collecting rejection letters.",
            $"Purple Moogle once tried to be a stand-up comedian... it was a disaster!",
            $"Purple Moogle is the reason they put instructions on shampoo bottles.",
            $"Purple Moogle's life motto: 'Why fit in when you were born to stand out... awkwardly?'",
            $"Purple Moogle is the human equivalent of a participation trophy.",
            $"Purple Moogle's favorite exercise is running away from responsibilities.",
            $"Purple Moogle is the CEO of awkward situations.",
        


        };
    }
}