using Bot.Services;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Bot
{
    public class _Config
    {
        public static bool DevMode = true;
        public static string BotName = "PixelBot";
        public static string Prefix = "p/";
        public static string DevPrefix = "tp/";
        public static string Github = "https://github.com/ArchboxDev/PixelBot";
        public static string BotPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/PixelBot/";
        public static Class Tokens = new Class();
        public static bool Ready = false;

        public static string MiscHelp = "";
        public static string GameHelp = "";
        public static string MediaHelp = "";
        public static string ModHelp = "";
        public static string DevHelp = "";
        
        public class Class
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
            public string GameInfo = "";
        }

        public static void SetupHelpMenu(CommandService Commands)
        {
          List<string> MiscList = new List<string>();
            List<string> GameList = new List<string>();
            List<string> MediaList = new List<string>();
            List<string> ModList = new List<string>();
            List<string> DevList = new List<string>();

            foreach (var i in Commands.Modules)
            {
                try
                {
                    i.Summary.Trim();
                    switch (i.Parent.Name)
                    {
                        case "Misc":
                            MiscList.Add($"[ p/{i.Name.ToLower()} ][ {i.Summary} ]");
                            break;
                        case "Game":
                        GameList.Add($"[ p/{i.Name.ToLower()} ][ {i.Summary} ]");
                            break;
                        case "Media":
                            MediaList.Add($"[ p/{i.Name.ToLower()} ][ {i.Summary} ]");
                            break;
                        case "Mod":
                            ModList.Add($"[ p/{i.Name.ToLower()} ][ {i.Summary} ]");
                            break;
                        case "Dev":
                            DevList.Add($"[ p/{i.Name.ToLower()} ][ {i.Summary} ]");
                            break;

                    }
                }
                catch
                {

                }
            }
            foreach (var i in Commands.Commands)
            {
                try
                {
                    i.Summary.Trim();
                    switch (i.Module.Name)
                    {
                        case "Misc":
                            MiscList.Add($"[ p/{i.Remarks} ][ {i.Summary} ]");
                            break;
                        case "Game":
                            GameList.Add($"[ p/{i.Remarks} ][ {i.Summary} ]");
                            break;
                        case "Media":
                            MediaList.Add($"[ p/{i.Remarks} ][ {i.Summary} ]");
                            break;
                        case "Mod":
                            ModList.Add($"[ p/{i.Remarks} ][ {i.Summary} ]");
                            break;
                        case "Dev":
                            DevList.Add($"[ p/{i.Remarks} ][ {i.Summary} ]");
                            break;
                    }
                }
                catch
                {

                }
            }

            MiscHelp = string.Join(Environment.NewLine, MiscList);
            GameHelp = string.Join(Environment.NewLine, GameList);
            MediaHelp = string.Join(Environment.NewLine, MediaList);
            ModHelp = string.Join(Environment.NewLine, ModList);
            DevHelp = string.Join(Environment.NewLine, DevList);
    }
            public static IServiceProvider AddServices(_Bot ThisBot, DiscordSocketClient Client, CommandService CommandService)
        {
            return new ServiceCollection()
                   .AddSingleton<DiscordSocketClient>(Client)
                   .AddSingleton<_Bot>(ThisBot)
                   .AddSingleton<PruneService>(new PruneService())
                   .AddSingleton<PaginationFull>(new PaginationFull(Client))
                   .AddSingleton<CommandHandler>(new CommandHandler(ThisBot, Client))
                   .AddSingleton<CommandService>(new CommandService())
                   .AddSingleton(new Stats(Client))
                   .AddSingleton(new Twitch(Client))
                   .BuildServiceProvider();
            
        }
    }

}