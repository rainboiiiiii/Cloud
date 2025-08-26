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

            $"Hey there, {username}! I currently do not possess the power nor knowledge to do live support just yet. However, feel free to open a ticket in <#1148356467498438718> and a staff member will be happy to assist!"

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
            $"We always notify users in advance about scheduled maintenance through our official channels. If you experience any issues after maintenance, please reach out to our support team for assistance",

        };

        public static List<string> OwnerResponses(string username) => new()
        {
            $"I once didn't listen to Rain. I regret not listening to him, so make sure you listen to him, {username}.",
            $"I hear Rain is pretty decent.",
            $"He's ok.",
            $"I have heard great things about Rain, although he isn't afraid to be outspoken, {username}.",
            $"I think Rain deserves slightly more recognition than most realize, {username}.",
            $"The owner is a rockstar, {username}.",
            $"Rain is a genius... 75% of the time.",
            $"He is a visionary, {username}.",
            $"He's my hero, {username}.",
            $"He may not be a superstar, but he's my inspiration!",
            $"I think he is great at helping others, so he's ok I guess.",
            $"Rain is pretty chill with most. Just don't get on his bad side. You'd regret it, {username}.",
            $"Rain is a pioneer, {username}.",
            $"Ask me later, I'm on a date with him right now and I don't want to stroke his ego.",
            $"Rain is genuinely a force to be reckoned with, {username}."
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

        public static List<string> UnknownCommandResponses(string username) => new()
        {
            $"I'm not sure what you mean, {username}. Could you please rephrase?",
            $"Sorry, {username}, I didn't catch that. Can you try again?",
            $"Hmm, that doesn't seem to be a valid command, {username}.",
            $"I'm still learning, {username}. Could you try a different command?",
            $"That command is unfamiliar to me, {username}. Maybe check the help section?",
            $"Oops! I don't recognize that command, {username}.",
            $"That doesn't seem right, {username}. Try using /help to see available commands.",
            $"I'm not programmed to understand that command yet, {username}.",
            $"That command is outside my capabilities, {username}.",
            $"I wish I could help with that, {username}, but I don't know how.",
            $"That command is a mystery to me, {username}.",
            $"I'm sorry, {username}, but I can't process that command.",
            $"That command is beyond my current functions, {username}.",
            $"I don't have the ability to execute that command, {username}.",
            $"That command is not in my repertoire, {username}."
        };

        public static List<string> ThanksResponses(string username) => new()
        {
            $"You're welcome, {username}!",
            $"No problem, {username}!",
            $"Anytime, {username}!",
            $"Glad I could help, {username}!",
            $"My pleasure, {username}!",
            $"Don't mention it, {username}!",
            $"Happy to assist, {username}!",
            $"Always here for you, {username}!",
            $"You got it, {username}!",
            $"Sure thing, {username}!"
        };

        public static List<string> SorryResponses(string username) => new()
        {
            $"No worries, {username}!",
            $"It's all good, {username}!",
            $"Don't sweat it, {username}!",
            $"All forgiven, {username}!",
            $"No harm done, {username}!",
            $"It's okay, {username}!",
            $"We're cool, {username}!",
            $"No big deal, {username}!",
            $"It's fine, {username}!",
            $"All is well, {username}!"
        };

        public static List<string> ByeResponses(string username) => new()
        {
            $"Goodbye, {username}! 👋",
            $"See you later, {username}!",
            $"Take care, {username}!",
            $"Catch you later, {username}!",
            $"Farewell, {username}!",
            $"Until next time, {username}!",
            $"Have a great day, {username}!",
            $"Stay safe, {username}!",
            $"See you soon, {username}!",
            $"Bye for now, {username}!"
        };


        public static List<string> FreeGPResponses(string username) => new()
        {
            $"No way, {username}!",
            $"Absolutely not, {username}!",
            $"Not a chance, {username}!",
            $"Nope, {username}!",
            $"Negative, {username}!",
            $"I don't think so, {username}!",
            $"Certainly not, {username}!",
            $"No can do, {username}!",
            $"Not in a million years, {username}!",
            $"No sir/ma'am, {username}!"
        };

        public static List<string> WhatCanCloudDoResponse(string username) => new()
        {

            $"I'm here to help you, {username}! You can ask me about my features using /info, get assistance with /help, or contact support with /support. I can help as much as my owner allows me to!"
            };
    }
}



    