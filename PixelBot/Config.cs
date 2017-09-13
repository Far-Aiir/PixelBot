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
        public static string PruneHelp = "";

        
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
        }
        public static void SetupHelpMenu(CommandService Commands)
        {
          List<string> MiscList = new List<string>();
        List<string> GameList = new List<string>();
        List<string> MediaList = new List<string>();
        List<string> PruneList = new List<string>();
            foreach (var CMD in Commands.Commands.Where(x => x.Module.Name == "Misc"))
            {
                try
                {
                    CMD.Summary.Trim();
                    MiscList.Add($"[ p/{CMD.Remarks} ][ {CMD.Summary} ]");
                }
                catch
                {

                }

            }
            foreach (var CMD in Commands.Commands.Where(x => x.Module.Name == "Game"))
            {
                try
                {
                    CMD.Summary.Trim();
                    GameList.Add($"[ p/{CMD.Remarks} ][ {CMD.Summary} ]");
                }
                catch
                {

                }
            }
            foreach (var CMD in Commands.Commands.Where(x => x.Module.Name == "Media"))
            {
                try
                {
                    CMD.Summary.Trim();
                    MediaList.Add($"[ p/{CMD.Remarks} ][ {CMD.Summary} ]");
                }
                catch
                {

                }
            }
            foreach (var CMD in Commands.Commands.Where(x => x.Module.Name == "prune"))
            {
                try
                {
                    CMD.Summary.Trim();
                    PruneList.Add($"[ p/{CMD.Remarks} ][ {CMD.Summary} ]");
                }
                catch
                {

                }
            }
            MiscHelp = string.Join(Environment.NewLine, MiscList);
            GameHelp = string.Join(Environment.NewLine, GameList);
            MediaHelp = string.Join(Environment.NewLine, MediaList);
            PruneHelp = string.Join(Environment.NewLine, PruneList);
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
                   //.AddSingleton(new DiscordStatus(Client))
                   .BuildServiceProvider();
            
        }
    }

}