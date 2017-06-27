using Newtonsoft.Json;
using PixelBot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PixelBot
{
    public class Startup
    {
        public static void Run()
        {
            Directory.CreateDirectory(Config.BotPath + "Twitch\\");
            Directory.CreateDirectory(Config.BotPath + "Users\\");
            
            if (Properties.Settings.Default.Blacklist == null)
            {
                Properties.Settings.Default.Blacklist = new System.Collections.Specialized.StringCollection();
            }
            
            
            ServicePointManager.DefaultConnectionLimit = 6;
        }
    }
}
