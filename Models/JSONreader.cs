using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TheCloud.Database;


namespace TheCloud.config
{
    internal class JSONreader
    {
        public string token { get; set; }
        public string prefix { get; set; }

        public string MONGO_DB { get; set; }
        public ulong ChannelID { get; private set; }

        public ulong CloudWatcherRoleID { get; set; }
        public ulong CloudsChannelID { get; set; }
        public string ImageChannelID { get; set; }
        public ulong AnnouncementChannelID { get; set; }



        public string MONGO_URI { get; set; }

        public async Task ReadJSON()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                string json = await sr.ReadToEndAsync();
                JSONStructure data = JsonConvert.DeserializeObject<JSONStructure>(json);

                this.token = data.token;
                this.prefix = data.prefix;
                this.MONGO_URI = data.MONGO_URI;
                this.ChannelID = data.ChannelID;
                this.MONGO_DB = data.MONGO_DB;
                this.ImageChannelID = data.ImageChannelID;
                this.CloudsChannelID = data.CloudsChannelID;
                this.AnnouncementChannelID = data.AnnouncementChannelID;
                this.CloudWatcherRoleID = data.CloudWatcherRoleID;


            }
        }
    }

    public class JSONStructure
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public ulong ChannelID { get; set; }
        public string MONGO_URI { get; set; }
        public string MONGO_DB { get; set; }
        public string ImageChannelID { get; set; }
        public ulong CloudsChannelID { get; set; }
        public ulong AnnouncementChannelID { get; set; }
        public ulong CloudWatcherRoleID { get; set; }
    }
}