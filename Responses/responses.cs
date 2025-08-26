using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TheCloud.Listener
{

    public static class BotResponses
       
    {


        public static List<string> GreetingResponses(string username) => new()
        {
            
            $"Hello, {username}! 👋",
            $"Hiya {username}, how's it going?",
            $"Yo {username}, what's up?",
            $"Hey {username}! Ready to chat?",
            $"Hi {username}! How can I assist you today?",
            $"Hello {username}! Hope you're having a great day!",
            $"Hey {username}! What's new with you?",
            $"Why hello there, {username}!",
            $"Greetings, {username}! How can I help?",
            $"Oh, why hello {username}!",
            $"Hey {username}! Let's make today awesome!",
            $"Hello {username}! What adventures await us today?",
            $"What's up, {username}?",
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
            $"Purple Moogle is smelly.",
            $"If anyone was the Dad of phobias, it'd be PurpleMoogle.",
            $"Purple Moogle? More like Purple No-go!",
            $"I heard Purple Moogle tried to join a circus once... they said no.",
            $"Purple Moogle's spirit animal is a confused llama!",
            $"If you see Purple Moogle, run the other way.",
            $"Purple Moogle is proof that evolution can go in reverse.",
            $"Purple Moogle's favorite hobby is collecting rejection letters.",
            $"Purple Moogle once tried to be a stand-up comedian... it was a disaster!",
            $"Purple Moogle is the reason they put instructions on shampoo bottles.",
            $"Purple Moogle is the human equivalent of a participation trophy.",
            $"Purple Moogle's favorite exercise is running away from responsibilities.",
            $"Purple Moogle is the CEO of awkward situations.",
            $"The only thing purple about Purple Moogle is his mood swings.",



        };

        public static List<string> KilometerResponses(string username) => new()
        {
            $"A kilometer is 1,000 meters, or about 0.621 miles for my imperial friends. A kilometer is just a really long meter, {username}.",
        };

        public static List<string> HelpResponses(string username) => new()
        {

            $"Hey there, {username}! I currently do not possess the power nor knowledge to do live support just yet. However, feel free to open a ticket in <1148356467498438718> and a staff member will be happy to assist!"
            
        };

        public static List<string> FreeGpResponses(string username) => new()
        {
            $"Free GP is a scam, {username}. Don't fall for it!",
            $"There's no such thing as free GP, {username}. If it sounds too good to be true, it probably is.",
            $"Free GP? More like free trouble, {username}. Stay away from that!",
            $"Free GP is a trap, {username}. Don't get caught in it!",
            $"Free GP is a myth, {username}. Don't waste your time looking for it!",
            $"Absolutely not, {username}."

        };

        public static List<string> FreeBPResponses(string username) => new()
        {
            $"Free BP is a scam, {username}. Don't fall for it!",
            $"There's no such thing as free BP, {username}. If it sounds too good to be true, it probably is.",
            $"Free BP? More like free trouble, {username}. Stay away from that!",
            $"Free BP is a trap, {username}. Don't get caught in it!",
            $"Free BP is a myth, {username}. Don't waste your time looking for it!",
            $"Absolutely not, {username}."
        };

        public static List<string> HanniResponses(string username) => new()
        {
            $"I have never met Hanni, but it would be nice to meet her one day,{username}.",
            $"I heard Hanni is the GOAT, but I have not met her before, {username}.",
            $"Hanni is cool from what I have heard, but I have never met her.",
        };

        public static List<string> MaintResponses(string username) => new()
        {
            $"We always notify users in advance about scheduled maintenance through our official channels.If you experience any issues after maintenance, please reach out to our support team for assistance",

        };

        public static List<string> OwnerResponses(string username) => new()
        {
            $"The owner is a cool person, {username}.",
            $"I hear the owner is pretty awesome, {username}.",
            $"The owner is the best, {username}.",
            $"I have heard great things about the owner, {username}.",
            $"The owner is a legend, {username}.",
            $"The owner is a rockstar, {username}.",
            $"The owner is a genius, {username}.",
            $"The owner is a visionary, {username}.",
            $"The owner is a hero, {username}.",
            $"The owner is a superstar, {username}.",
            $"The owner is a mastermind, {username}.",
            $"The owner is a trailblazer, {username}.",
            $"The owner is a pioneer, {username}.",
            $"The owner is a game-changer, {username}.",
            $"The owner is a force to be reckoned with, {username}."
        };

        public static List<string> CloudHelpResponses(string username) => new()
        {
            $"Thank you for wanting to help support my development! Use the /info command and look for the PayPal Link hyperlink and donate there!"
        };

        public static List<string> FreeStuffResponses(string username) => new()
        {
            $"Rain cannot send anything, Spanky and Macie only send gifts usually after maintenance or special events. Please do not ask for free stuff, {username}.",
            $"Absolutely not, {username}."
        };
    }
}