﻿using Bot.Utils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
namespace Bot
{
    #region Logger
    public class _Log
    {
        /// <summary>
        /// [Bot] Test
        /// </summary>
        public static void Bot(string Message)
        {
            Task.Run(() =>
            {
                if (Console.ForegroundColor != ConsoleColor.White)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine($"[{_Config.BotName}] {Message}");
            });
        }
        /// <summary>
        /// [Command] /test + Color Cyan
        /// </summary>
        public static void Command(string Message)
        {
            Task.Run(() =>
            {
                if (Console.ForegroundColor != ConsoleColor.Cyan)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                Console.WriteLine($"[Command] {Message}");
                Console.ForegroundColor = ConsoleColor.White;
            });
        }
        /// <summary>
        /// [Bot] Custom Text + Color Green
        /// </summary>
        public static void Ok(string Message)
        {
            Task.Run(() =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{_Config.BotName}] {Message}");
                Console.ForegroundColor = ConsoleColor.White;
            });
        }
        /// <summary>
        /// [Blacklist] Test + Color Magenta
        /// </summary>
        public static void Blacklist(string Message)
        {
            Task.Run(() =>
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[Blacklist] {Message}");
                Console.ForegroundColor = ConsoleColor.White;
            });
        }

        /// <summary>
        /// [Joined] Guild + Color Green
        /// </summary>
        public static void GuildJoined(string Message)
        {
            Task.Run(() =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[Joined] {Message}");
                Console.ForegroundColor = ConsoleColor.White;
            });
        }
        /// <summary>
        /// [Left] Guild + Color Green
        /// </summary>
        public static void GuildLeft(string Message)
        {
            Task.Run(() =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[Left] {Message}");
                Console.ForegroundColor = ConsoleColor.White;
            });
        }
        /// <summary>
        /// [Selfbot] Warning! + Color Yellow
        /// </summary>
        public static void Warning(string Message)
        {
            Task.Run(() =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{_Config.BotName}] {Message}");
                Console.ForegroundColor = ConsoleColor.White;
            });
        }
        /// <summary>
        /// [Error] Test + Color Red
        /// </summary>
        public static void Error(string Message)
        {
            Task.Run(() =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Error] {Message}");
                Console.ForegroundColor = ConsoleColor.White;
            });
        }
        /// <summary>
        /// [Custom Text] Test + ConsoleColor
        /// </summary>
        public static void Custom(string Message, ConsoleColor Color = ConsoleColor.White)
        {
            Task.Run(() =>
            {
                if (Color != ConsoleColor.White)
                {
                    Console.ForegroundColor = Color;
                }
                Console.WriteLine(Message);
                if (Console.ForegroundColor != ConsoleColor.White)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
            });
        }
    }
    #endregion

    public class _Bot
    {
        #region ConsoleFix
        private class DisableConsoleQuickEdit
        {
            const uint ENABLE_QUICK_EDIT = 0x0040;
            const int STD_INPUT_HANDLE = -10;
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr GetStdHandle(int nStdHandle);
            [DllImport("kernel32.dll")]
            static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
            [DllImport("kernel32.dll")]
            static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
            /// <summary>
            /// Fix the console from freezing the bot due to checking for readinput in the console
            /// </summary>
            internal static bool Go()
            {
                IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
                uint consoleMode;
                if (!GetConsoleMode(consoleHandle, out consoleMode))
                {
                    return false;
                }
                consoleMode &= ~ENABLE_QUICK_EDIT;
                if (!SetConsoleMode(consoleHandle, consoleMode))
                {
                    return false;
                }
                return true;
            }
        }
        #endregion

        public static DiscordSocketClient _Client;


        public static IServiceProvider _Services;
        public static CommandService _Commands;
        public static Dictionary<ulong, IGuildUser> BotCache = new Dictionary<ulong, IGuildUser>();
        private static string Status = "";
        public static string GetStatus()
        {
            if (Status == "")
            {
                return $"{_Config.Prefix}help [{_Client.Guilds.Count}] www.blaze.ml";
            }
            else
            {
                return Status;
            }
        }
        public static void SetStatus(string Message = "")
        {
            if (Message == "")
            {
                Status = "";
                _Client.SetGameAsync($"{_Config.Prefix}help [{_Client.Guilds.Count}] www.blaze.ml").GetAwaiter();
            }
            else
            {
                Status = Message;
                _Client.SetGameAsync(Message).GetAwaiter();
            }
        }
        public static void Main()
        {
            DisableConsoleQuickEdit.Go();
            Console.Title = _Config.BotName;
            Console.ForegroundColor = ConsoleColor.White;
            if (File.Exists($"{_Config.BotPath}LIVE.txt"))
            {
                _Config.DevMode = false;
                _Log.Bot("Loading in LIVE mode");
            }
            else
            {
                Console.Title = $"[DevMode] {_Config.BotName}";
                _Log.Bot("Loading in DEV mode");
            }
            CreateTempConfig();
            if (!File.Exists($"{_Config.BotPath}Config.json"))
            {
                _Log.Error("Config file not found");
                while (true)
                { }
            }
            LoadConfig();
            if (_Config.Tokens.Discord == "")
            {
                _Log.Error("Discord token not set");
                while (true)
                { }
            }
            _Blacklist.Load();
            _Whitelist.Load();
            _Client = new DiscordSocketClient(new DiscordSocketConfig { AlwaysDownloadUsers = true });
            _Commands = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Sync,
            });
            new _Bot().RunBot().GetAwaiter().GetResult();
        }

        #region Config Functions
        /// <summary>
        /// Create a template of the config
        /// </summary>
        private static void CreateTempConfig()
        {
            if (!Directory.Exists(_Config.BotPath))
            {
                Directory.CreateDirectory(_Config.BotPath);
            }
            _Config.Class NewConfig = new _Config.Class();
            using (StreamWriter file = File.CreateText(_Config.BotPath + "Config-Example" + ".json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, NewConfig);

            }
        }
        /// <summary>
        /// Load the config file
        /// </summary>
        private static void LoadConfig()
        {
            using (StreamReader reader = new StreamReader(_Config.BotPath + "Config.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                _Config.Tokens = (_Config.Class)serializer.Deserialize(reader, typeof(_Config.Class));
            }
        }
        #endregion

        #region Whitelist
        /// <summary>
        /// Manage the bot whitelist
        /// </summary>
        public class _Whitelist
        {
            public static string Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Whitelist/";
            private static HashSet<ulong> List = new HashSet<ulong>();
            /// <summary>
            /// Check if whitelist has a guild ID
            /// </summary>
            public static bool Check(ulong ID)
            {
                if (List.Contains(ID))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            /// <summary>
            /// Count how many items are in the whitelist
            /// </summary>
            public static int Count()
            {
                return List.Count();
            }
            /// <summary>
            /// Get all whitelist items
            /// </summary>
            public static HashSet<ulong> GetAll()
            {
                return List;
            }
            /// <summary>
            /// Add a guild ID to the whitelist
            /// </summary>
            public static void Add(ulong ID)
            {
                List.Add(ID);
                File.Create(Path + ID);
            }
            /// <summary>
            /// Remove a guild ID from the whitelist
            /// </summary>
            public static void Remove(ulong ID)
            {
                List.Remove(ID);
                if (File.Exists(Path + ID))
                {
                    File.Delete(Path + ID);
                }
            }
            /// <summary>
            /// Load the whitelist
            /// </summary>
            public static void Load()
            {
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
                foreach (var Item in Directory.GetFiles(Path))
                {
                    List.Add(Convert.ToUInt64(Item.Replace(Path, "")));
                }
            }
            /// <summary>
            /// Reload the whitelist
            /// </summary>
            public static void Reload()
            {
                HashSet<ulong> NewList = new HashSet<ulong>();
                foreach (var Item in Directory.GetFiles(Path))
                {
                    NewList.Add(Convert.ToUInt64(Item.Replace(Path, "")));
                }
                List.Clear();
                List = NewList;
            }
        }
        #endregion

        #region Blacklist
        /// <summary>
        /// Manage the bot blacklist
        /// </summary>
        public class _Blacklist
        {
            public static string Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Blacklist/";
            private static List<Item> List = new List<Item>();
            /// <summary>
            /// Blacklist Item | Name, ID, Reason, UsersToBots
            /// </summary>
            public class Item
            {
                public string GuildName = "";
                public ulong GuildID = 0;
                public string Reason = "";
                public string UsersToBots = "";
            }
            /// <summary>
            /// Count how many items are in the blacklist
            /// </summary>
            public static int Count()
            {
                return List.Count();
            }
            /// <summary>
            /// Get a blacklist item
            /// </summary>
            public static Item Get(ulong ID)
            {
                var GetItem = List.Where(x => x.GuildID == ID).First();
                if (GetItem != null)
                {
                    return GetItem;
                }
                else
                {
                    return null;
                }
            }
            /// <summary>
            /// Get all whitelist items
            /// </summary>
            public static List<Item> GetAll()
            {
                return List;
            }
            /// <summary>
            /// Check if blacklist has a guild ID
            /// </summary>
            public static bool Check(ulong ID)
            {
                if (List.Exists(x => x.GuildID == ID))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            /// <summary>
            /// Load the blacklist
            /// </summary>
            public static void Load()
            {
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
                foreach (var File in Directory.GetFiles(Path))
                {
                    using (StreamReader reader = new StreamReader(File))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        List.Add((Item)serializer.Deserialize(reader, typeof(Item)));
                    }
                }
                Timer Timer = new Timer()
                {
                    Interval = 300000
                };
                Timer.Elapsed += UpdateBlacklist;
                Timer.Start();
            }
            /// <summary>
            /// Add a guild ID to the blacklist
            /// </summary>
            public static void Add(string GuildName = "", ulong ID = 0, string Reason = "", string UsersBots = "")
            {
                Item NewBlacklist = new Item()
                {
                    GuildID = ID,
                    Reason = Reason,
                    GuildName = GuildName,
                    UsersToBots = UsersBots
                };
                List.Add(NewBlacklist);
                using (StreamWriter file = File.CreateText(Path + $"{ID.ToString()}.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, NewBlacklist);
                }
            }
            /// <summary>
            /// Remove a guild ID from the blacklist
            /// </summary>
            /// <param name="ID"></param>
            public static void Remove(ulong ID)
            {
                List.RemoveAll(x => x.GuildID == ID);
                if (File.Exists(Path + $"{ID.ToString()}.json"))
                {
                    File.Delete(Path + $"{ID.ToString()}.json");
                }
            }
            public static void UpdateBlacklist(object sender, ElapsedEventArgs e)
            {
                List<Item> BlacklistCache = new List<Item>();
                foreach (var File in Directory.GetFiles(_Blacklist.Path))
                {
                    using (StreamReader reader = new StreamReader(File.Replace(_Blacklist.Path, "").Replace(".json", "")))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        BlacklistCache.Add((Item)serializer.Deserialize(reader, typeof(Item)));
                    }

                }
                List.Clear();
                List = BlacklistCache;
            }
        }
        #endregion

        public async Task RunBot()
        {

            _Client.Connected += Client_Connected;
            _Client.Disconnected += Client_Disconnected;

            _Client.JoinedGuild += (g) => { var _ = Task.Run(() => Client_JoinedGuildAsync(g)); return Task.CompletedTask; };

            _Client.LeftGuild += (g) => { var _ = Task.Run(() => Client_LeftGuild(g)); return Task.CompletedTask; };

            _Client.GuildAvailable += (g) => { var _ = Task.Run(() => Client_GuildAvailable(g)); return Task.CompletedTask; };

            _Client.GuildUnavailable += (g) => { var _ = Task.Run(() => Client_GuildUnavailable(g)); return Task.CompletedTask; };

            _Client.Ready += Client_Ready;

            await _Config.ConfigureServices(_Client, _Commands, _Services);
            await _Client.LoginAsync(TokenType.Bot, _Config.Tokens.Discord);
            await _Client.StartAsync();

            await Task.Delay(-1);
        }
        
        #region ClientEvents
        private async Task Client_JoinedGuildAsync(SocketGuild g)
        {
            if (!BotCache.Keys.Contains(g.Id))
            {
                BotCache.Add(g.Id, g.CurrentUser);
            }

            if (!_Config.DevMode)
            {
                if (g.Owner == null)
                {
                    _Log.Warning($"[Joined] ({_Client.Guilds.Count}) {g.Name} - NULL OWNER - {g.Users.Where(x => !x.IsBot).Count()}/{g.Users.Where(x => x.IsBot).Count()} Users/Bots");
                    if (!_Bot._Whitelist.Check(g.Id))
                    {
                        try
                        {
                            await g.DefaultChannel.SendMessageAsync("This guild has no owner :( i have to leave. If this is an issue contact xXBuilderBXx#9113");
                        }
                        catch { }
                        await g.LeaveAsync();
                    }
                }
                else
                {
                    _Log.GuildJoined($"[{_Client.Guilds.Count}] {g.Name} - {g.Owner.Username}#{g.Owner.Discriminator} - {g.Users.Where(x => !x.IsBot).Count()}/{g.Users.Where(x => x.IsBot).Count()} Users/Bots");
                }
                if (_Bot._Whitelist.Check(g.Id))
                {
                    await _Client.SetGameAsync(GetStatus());
                    return;
                }
                int Users = g.Users.Where(x => !x.IsBot).Count();
                int Bots = g.Users.Where(x => x.IsBot).Count();
                if (_Bot._Blacklist.Check(g.Id))
                {
                    _Bot._Blacklist.Item BlacklistItem = _Bot._Blacklist.Get(g.Id);
                    _Log.Blacklist($"Removed {g.Name} - {g.Id}" + Environment.NewLine + $"    Reason: {BlacklistItem.Reason}");
                    try
                    {
                        await g.DefaultChannel.SendMessageAsync($"Removed guild > {BlacklistItem.Reason}");
                    }
                    catch { }
                    await g.LeaveAsync();
                }
                else if (Bots * 100 / g.Users.Count() > 85)
                {

                    _Log.Blacklist($"Removed {g.Name} - {g.Id}" + Environment.NewLine + $"    Reason: Bot collection guild");
                    try
                    {
                        await g.DefaultChannel.SendMessageAsync($"Removed guild > Bot collection guild");
                    }
                    catch { }
                    _Bot._Blacklist.Add(g.Name, g.Id, "Bot collection guild", $"{Users}/{Bots}");
                    await g.LeaveAsync();

                }
                else
                {
                    await _Client.SetGameAsync(GetStatus());
                }

            }
        }

        private async Task Client_LeftGuild(SocketGuild g)
        {
            _Log.GuildLeft($"{g.Name}");
            if (BotCache.Keys.Contains(g.Id))
            {
                BotCache.Remove(g.Id);
            }
            if (!_Config.DevMode && _Bot._Blacklist.Check(g.Id))
            {
                await _Client.SetGameAsync(GetStatus());
            }
        }

        private Task Client_Connected()
        {
            _Log.Bot("CONNECTED!");
            if (_Config.DevMode)
            {
                Console.Title = $"[DevMode] {_Config.BotName} - Online";
            }
            else
            {
                Console.Title = $"{_Config.BotName} - Online";
            }
            return Task.CompletedTask;
        }
        private Task Client_Disconnected(Exception ex)
        {
            _Log.Bot("DISCONNECTED!");
            if (_Config.DevMode)
            {
                Console.Title = $"[DevMode] {_Config.BotName} - Offline";
            }
            else
            {
                Console.Title = $"{_Config.BotName} - Offline";
            }
            return Task.CompletedTask;
        }

        private async Task Client_GuildAvailable(SocketGuild g)
        {
            if (!BotCache.Keys.Contains(g.Id))
            {
                BotCache.Add(g.Id, g.CurrentUser);
            }
            if (!_Config.DevMode)
            {
                if (_Bot._Whitelist.Check(g.Id)) return;
                if (g.Owner == null)
                {
                    _Log.Warning($"Null Owner > {g.Name}");
                    try
                    {
                        await g.DefaultChannel.SendMessageAsync("This guild has no owner :( i have to leave. If this is an issue contact xXBuilderBXx#9113");
                    }
                    catch { }
                    await g.LeaveAsync();
                }
                else
                {
                    int Users = g.Users.Where(x => !x.IsBot).Count();
                    int Bots = g.Users.Where(x => x.IsBot).Count();
                    if (_Bot._Blacklist.Check(g.Id))
                    {
                        _Bot._Blacklist.Item BlacklistItem = _Bot._Blacklist.Get(g.Id);
                        _Log.Blacklist($"Removed {g.Name} - {g.Id}" + Environment.NewLine + $"    Reason: {BlacklistItem.Reason}");
                        try
                        {
                            await g.DefaultChannel.SendMessageAsync($"Removed guild > {BlacklistItem.Reason}");
                        }
                        catch { }
                        await g.LeaveAsync();
                    }
                    else if (Bots * 100 / g.Users.Count() > 85)
                    {
                        _Bot._Blacklist.Add(g.Name, g.Id, "Bot collection guild", $"{Users}/{Bots}");
                        _Log.Blacklist($"Removed {g.Name} - {g.Id}" + Environment.NewLine + $"    Reason: Bot collection guild");
                        try
                        {
                            await g.DefaultChannel.SendMessageAsync($"Removed guild > Bot collection guild");
                        }
                        catch { }
                        await g.LeaveAsync();
                    }
                }
            }
        }

        private Task Client_GuildUnavailable(SocketGuild g)
        {
            if (BotCache.Keys.Contains(g.Id))
            {
                BotCache.Remove(g.Id);
            }
            return Task.CompletedTask;
        }

        private async Task Client_Ready()
        {
            _Log.Ok($"Ready in {_Client.Guilds.Count} guilds");
            _Config.Ready = true;
            if (!_Config.DevMode)
            {
                await _Client.SetGameAsync(GetStatus());
            }
        }
        #endregion
    }

    #region CommandService
    public class CommandHandler
    {
        private readonly DiscordSocketClient _Client;
        private readonly CommandService _Commands;
        private IServiceProvider _Services;
        public CommandHandler(DiscordSocketClient Client, CommandService Commands)
        {
            _Client = Client;
            _Commands = Commands;
            StartHandle();
        }
        public void Setup(IServiceProvider services)
        {
            _Services = services;
        }
        public Task StartHandle()
        {

            _Client.MessageReceived += (msg) => { var _ = Task.Run(() => RunCommand(msg)); return Task.CompletedTask; };
            return Task.CompletedTask;
        }
        public async Task RunCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.IsBot) return;
            int argPos = 0;
            if (_Config.DevMode == false)
            {
                if (!(message.HasStringPrefix(_Config.Prefix, ref argPos) || message.HasMentionPrefix(_Client.CurrentUser, ref argPos))) return;
                var context = new CommandContext(_Client, message);

                var result = await _Commands.ExecuteAsync(context, argPos, _Services);
                if (result.IsSuccess)
                {
                    if (context.Channel is IPrivateChannel)
                    {
                        _Log.Command($"DM" + Environment.NewLine + $"     {context.Message.Author}: {context.Message.Content}");
                    }
                    else
                    {
                        _Log.Command($"{context.Guild.Name} #{context.Channel.Name}" + Environment.NewLine + $"     {context.Message.Author}: {context.Message.Content}");
                    }
                }
                else
                {
                    if (result.ErrorReason.Contains("Bot requires guild permission") || result.ErrorReason.Contains("User requires guild permission") || result.ErrorReason.Contains("Bot requires channel permission") || result.ErrorReason.Contains("User requires channel permission"))
                    {
                        await context.Channel.SendMessageAsync($"`{result.ErrorReason}`");
                    }
                }
            }
            else
            {
                if (!(message.HasStringPrefix(_Config.DevPrefix, ref argPos))) return;
                var context = new CommandContext(_Client, message);

                var result = await _Commands.ExecuteAsync(context, argPos, _Services);
                if (result.IsSuccess)
                {
                    _Log.Command($"Executed > {context.Message.Content}");
                }
                else
                {
                    _Log.Custom($"[Command] Error > {context.Message.Content}" + Environment.NewLine + $"     Error: {result.ErrorReason}");
                }
            }
        }
    }
    #endregion
}
namespace Bot.Commands
{
    public class _Utils
    {
        public static int BotPercentage(int AllUsers, int BotUsers)
        {
            if (BotUsers == 0)
            {
                return 0;
            }
            else
            {
                return BotUsers * 100 / AllUsers;
            }
        }
        public static IGuild GetGuild(DiscordSocketClient Client, ulong ID)
        {
            IGuild Guild = null;
            try
            {
                Guild = Client.Guilds.ElementAt((int)ID - 1);
            }
            catch
            {
                Guild = Client.GetGuild(ID);
            }

            return Guild;
        }

        public static ITextChannel GetChannel(DiscordSocketClient Client, List<ITextChannel> Channels, ulong ID)
        {
            ITextChannel Chan = null;
            try
            {
                Chan = Channels.ElementAt((int)ID - 1);
            }
            catch
            {
                Chan = Channels.Where(x => x.Id == ID).First();
            }
            return Chan;
        }
    }
}
namespace Bot.Commands
{
    #region CoreCommands
    public class Core : ModuleBase
    {
        private DiscordSocketClient _Client;
        public Core(DiscordSocketClient Client)
        {
            _Client = Client;
        }

        [Command("ping")]
        public async Task Ping()
        {
            System.Net.NetworkInformation.PingReply PingDiscord = new System.Net.NetworkInformation.Ping().Send("discordapp.com");
            System.Net.NetworkInformation.PingReply PingGoogle = new System.Net.NetworkInformation.Ping().Send("google.com");
            await ReplyAsync($"`PONG > Discord: {PingDiscord.RoundtripTime} ms Google: {PingGoogle.RoundtripTime} ms Gateway: {_Client.Latency} ms`");
        }

        [Command("prefix")]
        public async Task Prefix()
        {
            await ReplyAsync($"My prefix is `{_Config.Prefix}` custom prefix option coming soon");
        }

        

        [Command("invite")]
        public async Task Invite()
        {
            var embed = new EmbedBuilder()
            {
                Description = $"[Add {_Client.CurrentUser.Username} to your server/guild](https://discordapp.com/oauth2/authorize?&client_id=" + Context.Client.CurrentUser.Id + "&scope=bot&permissions=0)"
            };
            if (Context.Guild == null)
            {
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                _Bot.BotCache.TryGetValue(Context.Guild.Id, out IGuildUser GU);
                if (!GU.GuildPermissions.EmbedLinks)
                {
                    await Context.Channel.SendMessageAsync($"Add {_Client.CurrentUser.Username} to your server/guild" + Environment.NewLine + "https://discordapp.com/oauth2/authorize?&client_id=" + Context.Client.CurrentUser.Id + "&scope=bot&permissions=0");
                }
                else
                {

                    await Context.Channel.SendMessageAsync("", false, embed);
                }
            }
        }

        [Command("website")]
        public async Task Website()
        {
            await ReplyAsync("https://blaze.ml");
        }

        [Command("github")]
        public async Task Github()
        {
            if (_Config.Github == "")
            {
                await ReplyAsync("This bot does not have a github");
                return;
            }
            var embed = new EmbedBuilder()
            {
                Description = $"Please report issues/suggestions to [Github]({_Config.Github})",
                Color = DiscordUtils.GetRoleColor(Context.Channel as ITextChannel)
            };
            if (Context.Guild == null)
            {
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                _Bot.BotCache.TryGetValue(Context.Guild.Id, out IGuildUser GU);
                if (!GU.GuildPermissions.EmbedLinks)
                {
                    await Context.Channel.SendMessageAsync($"Please report issues/suggestions to {_Config.Github}");
                }
                else
                {

                    await Context.Channel.SendMessageAsync("", false, embed);
                }
            }
        }

        [Group("o")]
        [Alias("owner")]
        public class Owner : ModuleBase
        {
            private CommandService _Commands;
            private DiscordSocketClient _Client;
            public Owner(CommandService Commands, DiscordSocketClient Client)
            {
                _Client = Client;
                _Commands = Commands;
            }
            [Command]
            public async Task O()
            {
                if (Context.User.Id != 190590364871032834)
                {
                    await ReplyAsync("The owner is xXBuilderBXx#9113 - 190590364871032834" + Environment.NewLine + $"For more info about the bot do {_Config.Prefix}help");
                }
                else
                {
                    List<string> OwnerCommands = new List<string>();
                    foreach (var CMD in _Commands.Commands.Where(x => x.Module.Name == "o"))
                    {
                        if (CMD.Remarks != null)
                        {
                            try
                            {
                                CMD.Remarks.Trim();
                                OwnerCommands.Add(CMD.Remarks);
                            }
                            catch { }
                        }
                    }
                    await ReplyAsync($"`{string.Join(" | ", OwnerCommands)} | blacklist | whitelist`").ConfigureAwait(false);
                }
            }

            [Command("invite"), Remarks("invite (GID/NUM)")]
            [RequireOwner]
            public async Task Invite(ulong ID)
            {
                if (ID == 0)
                {
                    ID = Context.Guild.Id;
                }
                IGuild Guild = _Utils.GetGuild(_Client, ID);
                Console.WriteLine(Guild.Name);
                if (Guild == null)
                {
                    await ReplyAsync($"`Cannot find guild {ID}`");
                    return;
                }
                var Invites = await Guild.GetInvitesAsync();
                if (Invites.Count != 0)
                {
                    await ReplyAsync(Invites.First().Code);
                    return;
                }
                IGuildChannel Chan = await Guild.GetDefaultChannelAsync();
                var Invite = await Chan.CreateInviteAsync();
                if (Invite != null)
                {
                    await ReplyAsync(Invite.Code);
                }
                else
                {
                    await ReplyAsync($"`Could not create invite for guild {ID}`");
                    return;
                }
            }

            [Command("info"), Remarks("info (GID/NUM)")]
            [RequireOwner]
            public async Task Oinfo(ulong ID = 0)
            {
                if (ID == 0)
                {
                    ID = Context.Guild.Id;
                }
                IGuild Guild = _Utils.GetGuild(_Client, ID);
                if (Guild == null)
                {
                    await ReplyAsync($"`Cannot find guild {ID}`");
                    return;
                }
                string Owner = "NO OWNER";
                var Users = await Guild.GetUsersAsync();
                IGuildUser ThisOwner = await Guild.GetOwnerAsync().ConfigureAwait(false);
                if (ThisOwner != null)
                {
                    Owner = $"{ThisOwner.Username}#{ThisOwner.Discriminator} - {ThisOwner.Id}";
                }
                var embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = $"{Guild.Name}",
                        IconUrl = Guild.IconUrl
                    },
                    Description = "```md" + Environment.NewLine + $"<ID {Guild.Id}>" + Environment.NewLine + $"<Owner {Owner}>" + Environment.NewLine + $"<Users {Users.Where(x => !x.IsBot).Count()}> <Bots {Users.Where(x => x.IsBot).Count()}> <BotPercentage {_Utils.BotPercentage(Users.Count(), Users.Where(x => x.IsBot).Count())}>```",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Created {Guild.CreatedAt.Day}/{Guild.CreatedAt.Month}/{Guild.CreatedAt.Year}"
                    },
                    Color = DiscordUtils.GetRoleColor(Context.Channel as ITextChannel)
                };
                await ReplyAsync("", false, embed);
            }

            [Command("leavehere"), Remarks("leavehere")]
            [RequireOwner]
            public async Task Leavehere()
            {
                await Context.Guild.LeaveAsync();
            }

            [Command("clearcon"), Remarks("clearcon")]
            [RequireOwner]
            public async Task Clear()
            {
                await Context.Message.DeleteAsync().ConfigureAwait(false);
                Console.Clear();
                _Log.Custom("Console cleared", ConsoleColor.Cyan);
                await Context.Channel.SendMessageAsync("`Console cleared`").ConfigureAwait(false);
            }

            [Command("botcol"), Remarks("botcol (*int)")]
            [RequireOwner]
            public async Task Botcol(int Number = 85)
            {
                List<string> GuildList = new List<string>();
                foreach (var Guild in _Client.Guilds.Where(x => !_Bot._Whitelist.Check(x.Id)))
                {
                    var Users = Guild.Users;
                    IGuildUser Owner = Guild.Owner;
                    if (Owner != null)
                    {

                        if (Users.Where(x => x.IsBot).Count() * 100 / Users.Count() > Number)
                        {
                            GuildList.Add($"{Guild.Name} ({Guild.Id}) - Owner: {Owner.Username} ({Owner.Id}) - {Users.Where(x => !x.IsBot).Count()}/{Users.Where(x => x.IsBot).Count()}");
                        }
                    }
                    else
                    {
                        GuildList.Add($"{Guild.Name} ({Guild.Id}) - NO OWNER! - {Users.Where(x => !x.IsBot).Count()}/{Users.Where(x => x.IsBot).Count()}");
                    }
                }
                if (GuildList.Count == 0)
                {
                    await ReplyAsync("`No bot collection guilds found :D`");
                }
                else
                {
                    string AllGuilds = string.Join(Environment.NewLine, GuildList.ToArray());
                    await ReplyAsync("```" + Environment.NewLine + AllGuilds + "```");
                }
            }

            [Command("leave"), Remarks("leave (GID/NUM)")]
            [RequireOwner]
            public async Task Leave(ulong ID)
            {
                try
                {
                    await Context.Message.DeleteAsync();
                }
                catch { }
                IGuild Guild = _Utils.GetGuild(_Client, ID);
                if (Guild == null)
                {
                    await ReplyAsync($"`Could not find guild by id {ID}`");
                    return;
                }
                try
                {
                    IGuildUser Owner = await Guild.GetOwnerAsync();
                    await Guild.LeaveAsync();
                    await ReplyAsync($"`Left guild {Guild.Name} - {Guild.Id} | Owned by {Owner.Username}#{Owner.Discriminator}`");
                }
                catch
                {
                    await Guild.LeaveAsync();
                    await ReplyAsync($"`Left guild {Guild.Name} - {Guild.Id}`");
                }
            }

            [Group("whitelist")]
            public class WhitelistGroup : ModuleBase
            {
                private DiscordSocketClient _Client;
                public WhitelistGroup(CommandService Commands, DiscordSocketClient Client)
                {
                    _Client = Client;
                }
                [Command]
                [RequireOwner]
                public async Task Whitelist()
                {

                    await ReplyAsync("`Whitelist > add (ID) | reload | remove (ID) | list`");
                }

                [Command("add")]
                [RequireOwner]
                public async Task WhitelistAdd(ulong ID)
                {
                    if (_Bot._Whitelist.Check(ID))
                    {
                        await Context.Channel.SendMessageAsync($"`{ID} is already in the whitelist`").ConfigureAwait(false);
                    }
                    else
                    {
                        IGuild Guild = _Client.GetGuild(ID);
                        if (Guild != null)
                        {
                            _Bot._Whitelist.Add(ID);
                            await ReplyAsync($"`Adding {Guild.Name} - {ID} to whitelist`");
                        }
                        else
                        {
                            _Bot._Whitelist.Add(ID);
                            await ReplyAsync($"`Adding {ID} to whitelist`");
                        }
                    }
                }

                [Command("reload")]
                [RequireOwner]
                public async Task WhitelistReload()
                {
                    _Bot._Whitelist.Reload();
                    await ReplyAsync("`Whitelist reloaded`").ConfigureAwait(false);
                }

                [Command("remove")]
                [RequireOwner]
                public async Task WhitelistRemove(ulong ID)
                {
                    if (!_Bot._Whitelist.Check(ID))
                    {
                        await ReplyAsync($"`Could not find {ID} in whitelist`").ConfigureAwait(false);
                    }
                    else
                    {
                        _Bot._Whitelist.Remove(ID);
                        await ReplyAsync($"`Removed {ID} from whitelist`").ConfigureAwait(false);
                    }
                }

                [Command("list")]
                [RequireOwner]
                public async Task WhitelistList()
                {
                    if (_Bot._Whitelist.Count() == 0)
                    {
                        await ReplyAsync("`Whitelist is empty`").ConfigureAwait(false);
                    }
                    else
                    {
                        await ReplyAsync("```" + Environment.NewLine + string.Join(Environment.NewLine, _Bot._Whitelist.GetAll()) + "```").ConfigureAwait(false);
                    }
                }
            }

            [Group("blacklist")]
            public class BlacklistGroup : ModuleBase
            {
                private DiscordSocketClient _Client;
                public BlacklistGroup(CommandService Commands, DiscordSocketClient CLient)
                {
                    _Client = CLient;
                }
                [Command]
                [RequireOwner]
                public async Task Blacklist()
                {

                    await ReplyAsync("`Blacklist > add (ID) | reload | remove (ID) | list | info (ID)`");
                }

                [Command("add")]
                [RequireOwner]
                public async Task BlacklistAdd(ulong ID, [Remainder] string Reason = "")
                {
                    if (_Bot._Blacklist.Check(ID))
                    {
                        await Context.Channel.SendMessageAsync($"`{ID} is already in the blacklist`").ConfigureAwait(false);
                    }
                    else
                    {
                        IGuild Guild = _Client.GetGuild(ID);
                        if (Guild != null)
                        {
                            var Users = await Guild.GetUsersAsync();
                            _Bot._Blacklist.Add(Guild.Name, ID, Reason, $"{Users.Where(x => !x.IsBot).Count()}/{Users.Where(x => x.IsBot).Count()}");
                            await ReplyAsync($"`Adding {Guild.Name} - {ID} to blacklist`");
                            await Guild.LeaveAsync();
                        }
                        else
                        {
                            _Bot._Blacklist.Add("", ID, Reason);
                            await ReplyAsync($"`Adding {ID} to blacklist`");
                        }
                    }
                }

                [Command("reload")]
                [RequireOwner]
                public async Task BlacklistReload()
                {
                    _Bot._Blacklist.UpdateBlacklist(null, null);
                    await ReplyAsync("`Blacklist reloaded`").ConfigureAwait(false);
                }

                [Command("remove")]
                [RequireOwner]
                public async Task BlacklistRemove(ulong ID)
                {
                    if (!_Bot._Blacklist.Check(ID))
                    {
                        await ReplyAsync($"`Could not find {ID} in blacklist`").ConfigureAwait(false);
                    }
                    else
                    {
                        _Bot._Blacklist.Item Item = _Bot._Blacklist.Get(ID);
                        _Bot._Blacklist.Remove(ID);
                        await ReplyAsync($"`Removed {Item.GuildName} - {Item.GuildID} from blacklist`").ConfigureAwait(false);
                    }
                }

                [Command("list")]
                [RequireOwner]
                public async Task BlacklistList()
                {
                    if (_Bot._Blacklist.Count() == 0)
                    {
                        await ReplyAsync("`Blacklist is empty`").ConfigureAwait(false);
                    }
                    else
                    {
                        List<string> BlacklistItems = new List<string>();
                        foreach (var i in _Bot._Blacklist.GetAll())
                        {
                            BlacklistItems.Add($"{i.GuildName} - {i.GuildID} ({i.UsersToBots})");
                        }
                        await ReplyAsync("```" + Environment.NewLine + string.Join(Environment.NewLine, BlacklistItems) + "```").ConfigureAwait(false);
                    }
                }

                [Command("info")]
                [RequireOwner]
                public async Task BlacklistInfo(ulong ID)
                {
                    if (_Bot._Blacklist.Check(ID))
                    {
                        await ReplyAsync("`Could not find guild ID`").ConfigureAwait(false);
                    }
                    else
                    {

                        _Bot._Blacklist.Item Item = _Bot._Blacklist.Get(ID);
                        await ReplyAsync($"{Item.GuildName} - {Item.GuildID} ({Item.UsersToBots})" + Environment.NewLine + Item.Reason).ConfigureAwait(false);
                    }
                }
            }

            [Command("say"), Remarks("say (Text)")]
            [RequireOwner]
            public async Task Say([Remainder]string Message)
            {
                _Bot.BotCache.TryGetValue(Context.Guild.Id, out IGuildUser GU);
                if (GU.GuildPermissions.ManageMessages)
                {
                    await Context.Message.DeleteAsync();
                }
                if (GU.GuildPermissions.EmbedLinks)
                {
                    var embed = new EmbedBuilder()
                    {
                        Description = Message,
                        Color = DiscordUtils.GetRoleColor(Context.Channel as ITextChannel)
                    };
                    await ReplyAsync("", false, embed);
                }
                else
                {
                    await ReplyAsync(Message);
                }
            }

            [Command("csay"), Remarks("csay (CID) (Text)")]
            [RequireOwner]
            public async Task SayChannel(ulong CID, [Remainder]string Message)
            {
                _Bot.BotCache.TryGetValue(Context.Guild.Id, out IGuildUser GU);
                if (GU.GuildPermissions.ManageMessages)
                {
                    await Context.Message.DeleteAsync();
                }
                try
                {
                    ITextChannel Chan = (ITextChannel)_Utils.GetChannel(_Client, (await Context.Guild.GetTextChannelsAsync()).ToList(), CID);
                    if (Chan == null)
                    {
                        await ReplyAsync($"Could not find channel `{CID}`");
                        return;
                    }
                    if (GU.GuildPermissions.EmbedLinks)
                    {
                        var embed = new EmbedBuilder()
                        {
                            Description = Message,
                            Color = DiscordUtils.GetRoleColor(Chan as ITextChannel)
                        };
                        await Chan.SendMessageAsync("", false, embed);
                    }
                    else
                    {
                        await Chan.SendMessageAsync(Message);
                    }
                }
                catch (Exception ex)
                {
                    await ReplyAsync(ex.Message);
                }
            }

            [Command("gsay"), Remarks("gsay (GID/NUM) (CID) (Text)")]
            [RequireOwner]
            public async Task SayGuild(ulong GID, ulong CID, [Remainder]string Message)
            {
                IGuild Guild = _Utils.GetGuild(_Client, GID);

                if (Guild == null)
                {
                    await ReplyAsync($"Could not find guild `{GID}`");
                    return;
                }
                ITextChannel Chan = (ITextChannel)_Utils.GetChannel(_Client, (await Guild.GetTextChannelsAsync()).ToList(), CID);
                if (Chan == null)
                {
                    await ReplyAsync($"Could not find channel `{CID}`");
                    return;
                }
                _Bot.BotCache.TryGetValue(Guild.Id, out IGuildUser GU);
                if (GU.GuildPermissions.ManageMessages)
                {
                    await Context.Message.DeleteAsync();
                }
                try
                {
                    if (GU.GuildPermissions.EmbedLinks)
                    {
                        var embed = new EmbedBuilder()
                        {
                            Description = Message,
                            Color = DiscordUtils.GetRoleColor(Chan as ITextChannel)
                        };
                        await Chan.SendMessageAsync("", false, embed);
                    }
                    else
                    {
                        await Chan.SendMessageAsync(Message);
                    }
                }
                catch (Exception ex)
                {
                    await ReplyAsync(ex.Message);
                }
            }

            [Command("find"), Remarks("find (Name)")]
            [RequireOwner]
            public async Task FindGuild([Remainder]string Name)
            {
                IGuild Guild = null;
                try
                {
                    Guild = _Client.Guilds.Where(x => x.Name.ToLower().Contains(Name.ToLower())).First();
                }
                catch
                {
                    await ReplyAsync($"Could not find guild with name `{Name}`");
                    return;
                }

                string Owner = "NO OWNER";
                var Users = await Guild.GetUsersAsync();
                IGuildUser ThisOwner = await Guild.GetOwnerAsync().ConfigureAwait(false) ?? null;
                if (ThisOwner != null)
                {
                    Owner = $"{ThisOwner.Username}#{ThisOwner.Discriminator} - {ThisOwner.Id}";
                }
                var embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = $"{Guild.Name}",
                        IconUrl = Guild.IconUrl
                    },
                    Description = "```md" + Environment.NewLine + $"<ID {Guild.Id}>" + Environment.NewLine + $"<Owner {Owner}>" + Environment.NewLine + $"<Users {Users.Where(x => !x.IsBot).Count()}> <Bots {Users.Where(x => x.IsBot).Count()}> <BotPercentage {_Utils.BotPercentage(Users.Count(), Users.Where(x => x.IsBot).Count())}>```",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Created {Guild.CreatedAt.Day}/{Guild.CreatedAt.Month}/{Guild.CreatedAt.Year}"
                    },
                    Color = DiscordUtils.GetRoleColor(Context.Channel as ITextChannel)
                };
                await ReplyAsync("", false, embed).ConfigureAwait(false);

            }

            [Command("channels"), Remarks("channels (GID)")]
            [RequireOwner]
            public async Task Channels(ulong ID = 0)
            {
                if (ID == 0)
                {
                    ID = Context.Guild.Id;
                }
                List<string> Channels = new List<string>();
                IGuild Guild = _Utils.GetGuild(_Client, ID);

                if (Guild == null)
                {
                    await ReplyAsync($"`Could not find guild by id {ID}`");
                    return;
                }
                int Count = 1;
                foreach (var Chan in await Guild.GetTextChannelsAsync())
                {
                    Channels.Add($"<[{Count}]{Chan.Name} {Chan.Id}>");
                    Count++;
                }
                var embed = new EmbedBuilder()
                {
                    Description = "```md" + Environment.NewLine + string.Join(Environment.NewLine, Channels) + Environment.NewLine + "```",
                    Color = DiscordUtils.GetRoleColor(Context.Channel as ITextChannel)
                };
                _Bot.BotCache.TryGetValue(Guild.Id, out IGuildUser GU);
                if (!GU.GuildPermissions.EmbedLinks)
                {
                    await Context.Channel.SendMessageAsync("```md" + Environment.NewLine + string.Join(Environment.NewLine, Channels) + Environment.NewLine + "```");
                }
                else
                {

                    await Context.Channel.SendMessageAsync("", false, embed);
                }

            }
        }
    }
    #endregion
}

namespace Bot.Utils
{
    #region HttpRequest
    public class HttpRequest
    {
        public static string GetString(string Url)
        {
            string Response = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Proxy = null;
            request.Method = WebRequestMethods.Http.Get;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        Response = reader.ReadToEnd();
                    }
                }
            }
            return Response;
        }
        public static dynamic GetJsonObject(string Url, string Auth = "", string OtherHeader = "", string OtherValue = "")
        {
            dynamic Response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.Proxy = null;
                request.Method = WebRequestMethods.Http.Get;
                request.Accept = "application/json";
                if (Auth != "")
                {
                    request.Headers.Add("Authorization", Auth);
                }
                if (OtherHeader != "")
                {
                    request.Headers.Add(OtherHeader, OtherValue);
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream stream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string ResponseText = reader.ReadToEnd();
                            Response = Newtonsoft.Json.Linq.JObject.Parse(ResponseText);
                        }

                    }
                }
            }
            catch { }
            return Response;
        }
        public static dynamic GetJsonArray(string Url, string Auth = "", string OtherHeader = "", string OtherValue = "")
        {
            dynamic Response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.Proxy = null;
                request.Method = WebRequestMethods.Http.Get;
                request.Accept = "application/json";
                if (Auth != "")
                {
                    request.Headers.Add("Authorization", Auth);
                }
                if (OtherHeader != "")
                {
                    request.Headers.Add(OtherHeader, OtherValue);
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream stream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string ResponseText = reader.ReadToEnd();
                            Response = Newtonsoft.Json.Linq.JArray.Parse(ResponseText);
                        }
                    }
                }
            }
            catch { }
            return Response;
        }

    }
    #endregion

    #region DiscordUtils
    public class DiscordUtils
    {
        public static async Task<IGuildUser> StringToUser(IGuild Guild, string Username)
        {
            IGuildUser GuildUser = null;
            if (Username.StartsWith("<@"))
            {
                string RealUsername = Username;
                RealUsername = RealUsername.Replace("<@", "").Replace(">", "");
                if (RealUsername.Contains("!"))
                {
                    RealUsername = RealUsername.Replace("!", "");
                }
                GuildUser = await Guild.GetUserAsync(Convert.ToUInt64(RealUsername));
            }
            else
            {
                GuildUser = await Guild.GetUserAsync(Convert.ToUInt64(Username));
            }
            return GuildUser;
        }

        public static string StringToUserID(string User)
        {
            if (User.StartsWith("("))
            {
                User = User.Replace("(", "");
            }
            if (User.EndsWith(")"))
            {
                User = User.Replace(")", "");
            }
            if (User.StartsWith("<@"))
            {
                User = User.Replace("<@", "").Replace(">", "");
                if (User.Contains("!"))
                {
                    User = User.Replace("!", "");
                }
            }
            return User;
        }

        public static Color GetRoleColor(ITextChannel Channel)
        {
            Color RoleColor = new Discord.Color(30, 0, 200);
            IGuildUser BotUser = null;
            if (Channel != null)
            {
                _Bot.BotCache.TryGetValue(Channel.Guild.Id, out BotUser);
                if (BotUser.GetPermissions(Channel).EmbedLinks)
                {
                    if (BotUser != null)
                    {
                        if (BotUser.RoleIds.Count != 0)
                        {
                            foreach (var Role in BotUser.Guild.Roles.OrderBy(x => x.Position))
                            {
                                if (BotUser.RoleIds.Contains(Role.Id))
                                {
                                    RoleColor = Role.Color;
                                }
                            }
                        }
                    }
                }
            }
            return RoleColor;
        }
    }
    #endregion

    #region OtherUtils
    public class OtherUtils
    {
        public static DateTime UnixToDateTime(long Unix)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Unix).ToLocalTime();
            return dtDateTime;
        }
    }
    #endregion
}