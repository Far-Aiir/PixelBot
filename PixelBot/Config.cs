using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Bot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public class Config
    {
        public static bool DevMode = true;
        public static string BotName = "PixelBot";
        public static string Prefix = "p/";
        public static string DevPrefix = "tp/";
        public static string ClientID = "";
        public static string BotPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/PixelBot/";
        public static string PathBlacklist = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Blacklist/";
        public static ConfigClass _Configs = new ConfigClass();
        
        public static ulong ChatlogGuild = 0;

        public static void ConfigLoad()
        {
            ConfigClass NewConfig = new ConfigClass();
            using (StreamWriter file = File.CreateText(BotPath + "Config-Example" + ".json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, NewConfig);

            }
            if (File.Exists(Config.BotPath + "Config.json"))
            {
                using (StreamReader reader = new StreamReader(BotPath + "Config.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    _Configs = (ConfigClass)serializer.Deserialize(reader, typeof(ConfigClass));
                }
            }
        }
    }
    public class ConfigClass
    {
        public string Discord { get; set; } = "";
        public string MysqlHost { get; set; } = "";
        public string MysqlUser { get; set; } = "";
        public string MysqlPass { get; set; } = "";
        public string Twitch { get; set; } = "";
        public string TwitchAuth { get; set; } = "";
        public string Steam { get; set; } = "";
        public string Osu { get; set; } = "";
        public string Xbox { get; set; } = "";
        public string Vainglory { get; set; } = "";
        public string Youtube { get; set; } = "";
        public string Dbots { get; set; } = "";
        public string DbotsV2 { get; set; } = "";
        public string Riot { get; set; } = "";
        public string Wargaming { get; set; } = "";
    }
    public class BotServices
    {
        public static IServiceProvider AddServices(DiscordSocketClient _Client, CommandService _CommandService, CommandHandler _CommandHandler)
        {
            IServiceProvider _Services = new ServiceCollection()
               .AddSingleton(_Client)
               .AddSingleton(new PruneService())
               .AddSingleton(new PaginationFull(_Client))
               .AddSingleton(_CommandHandler)
               .AddSingleton(_CommandService)
               .AddSingleton(new Stats(_Client))
               .AddSingleton(new Twitch(_Client))
               .BuildServiceProvider();
            return _Services;
        }
    }
}
