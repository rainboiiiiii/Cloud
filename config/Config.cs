using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCloud
{
    public class DiscordConfig
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public ulong ChannelId { get; set; }
        
        public string CloudinaryCloudName { get; set; }
        public string CloudinaryApiKey { get; set; }   
        public string CloudinaryApiSecret { get; set; }
    }
}

