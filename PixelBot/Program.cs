﻿using Discord;
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
using Bot.Services;
namespace Bot
{
    public class Program
    {
        public static void Main()
        {

            new _Bot().Start().GetAwaiter().GetResult();
        }

    }

    #region Logger
    public class _Log
    {
        /// <summary>
        /// [Bot] Test
        /// </summary>
        public static void Bot(string Message)
        {
            if (Console.ForegroundColor != ConsoleColor.White)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine($"[{_Config.BotName}] {Message}");
        }
        /// <summary>
        /// [Command] Command + Color Cyan
        /// </summary>
        public static void Command(CommandContext Context)
        {
            if (Console.ForegroundColor != ConsoleColor.Cyan)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            string Command = Context.Message.Content;
            if (Command.StartsWith("<@"))
            {
                Command = $"@{Context.Client.CurrentUser.Username} {Context.Message.Content.Replace($"<@{Context.Client.CurrentUser.Id}> ", "")}";
                if (Context.Message.Content.StartsWith("<@!"))
                {
                    Command = $"@{Context.Client.CurrentUser.Username} {Context.Message.Content.Replace($"<@!{Context.Client.CurrentUser.Id}> ", "")}";
                }
            }
            if (Context.Guild == null)
            {
                Console.WriteLine($"[Command] DM" + Environment.NewLine + $"     {Context.Message.Author}: {Command}");
            }
            else
            {
                Console.WriteLine($"[Command] Guild | {Context.Guild.Name} #{Context.Channel.Name}" + Environment.NewLine + $"     {Context.Message.Author}: {Command}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// [Command Error] ErrorMessage + Color Yellow
        /// </summary>
        public static void CommandError(string ErrorMessage, CommandContext Context)
        {
            if (Console.ForegroundColor != ConsoleColor.Yellow)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            string Command = Context.Message.Content;
            if (Command.StartsWith("<@"))
            {
                Command = $"@{Context.Client.CurrentUser.Username} {Context.Message.Content.Replace($"<@{Context.Client.CurrentUser.Id}> ", "")}";
                if (Context.Message.Content.StartsWith("<@!"))
                {
                    Command = $"@{Context.Client.CurrentUser.Username} {Context.Message.Content.Replace($"<@!{Context.Client.CurrentUser.Id}> ", "")}";
                }
            }
            if (Context.Guild == null)
            {
                Console.WriteLine($"[Command Error] DM > {Command}" + Environment.NewLine + $"       {Context.User.Username}#{Context.User.Discriminator}: {ErrorMessage}");
            }
            else
            {
                Console.WriteLine($"[Command Error] Guild | {Context.Guild.Name} #{Context.Channel.Name} > {Command} " + Environment.NewLine + $"       {Context.User.Username}#{Context.User.Discriminator}: {ErrorMessage}");
            }

            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// [Bot] Custom Text + Color Green
        /// </summary>
        public static void Ok(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{_Config.BotName}] {Message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// [Blacklist] Guild + Color Magenta
        /// </summary>
        public static void Blacklist(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[Blacklist] {Message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// [Joined] Guild + Color Green
        /// </summary>
        public static void GuildJoined(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[Joined] {Message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// [Left] Guild + Color Green
        /// </summary>
        public static void GuildLeft(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[Left] {Message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// [Bot] Warning! + Color Yellow
        /// </summary>
        public static void Warning(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{_Config.BotName}] {Message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// [Error] Error! + Color Red
        /// </summary>
        public static void Error(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Error] {Message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// CustomText + ConsoleColor
        /// </summary>
        public static void Custom(string Message, ConsoleColor Color = ConsoleColor.White)
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
        }
        /// <summary>
        /// Throw a command error exception
        /// </summary>
        public static void ThrowError(ICommandContext Context, string Message, string ConsoleMessage)
        {
            _Log.CommandError(ConsoleMessage, (CommandContext)Context);
            throw new Exception("Custom - " + Message);
        }
        public static void ThrowError(string Message)
        {
            throw new Exception("Custom - " + Message);
        }
    }
    #endregion

    internal class _GuildCount
    {
        public int server_count = 0;
    }
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
        public string Prefix = "";
        static Dictionary<ulong, IGuildUser> BotCache = new Dictionary<ulong, IGuildUser>();

        public static IGuildUser GetBot(ulong GuildID)
        {
            BotCache.TryGetValue(GuildID, out IGuildUser Bot);
            return Bot;
        }

        public void AddBot(ulong GuildID, IGuildUser Bot)
        {
            BotCache.Add(GuildID, Bot);
        }

        public DiscordSocketClient _Client;
        public bool DevMode = true;
        public string DisabledMessage = "";
        public bool Ready = false;
        public IServiceProvider _Services;
        public CommandService _Commands;
        public Stopwatch Uptime = new Stopwatch();

        public string GetStatus()
        {
            return $"{_Config.Prefix}help [{_Client.Guilds.Count}] blazeweb.ml";
        }

        public _Bot()
        {
            _Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Warning,
                ConnectionTimeout = int.MaxValue,
                MessageCacheSize = 10,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });
            _Commands = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Sync,
            });
        }

        internal async Task Start()
        {
            DisableConsoleQuickEdit.Go();
            Console.Title = _Config.BotName;
            Console.ForegroundColor = ConsoleColor.White;
            LoadConfig();
            _Services = _Config.AddServices(this, _Client, _Commands);

            if (File.Exists($"{_Config.BotPath}LIVE.txt"))
            {
                DevMode = false;
                _Blacklist.Load();
                _Whitelist.Load();
                Prefix = _Config.Prefix;
                _Log.Bot($"Loading in LIVE mode");
            }
            else
            {
                Console.Title = $"[DevMode] {_Config.BotName}";
                Prefix = $"t{_Config.Prefix}";
                _Log.Bot($"Loading in DEV mode");
            }
            var CommandHandler = _Services.GetService<CommandHandler>();
            _Commands = _Services.GetService<CommandService>();
            await _Commands.AddModulesAsync(this.GetType().Assembly);
            CommandHandler.AddServices(_Services, _Commands);
            await CommandHandler.StartHandle().ConfigureAwait(false);
            await Login().ConfigureAwait(false);
        }

        #region LoadConfig
        private void LoadConfig()
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
            if (!File.Exists($"{_Config.BotPath}Config.json"))
            {
                _Log.Error("Config file not found");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else
            {
                using (StreamReader reader = new StreamReader(_Config.BotPath + "Config.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    _Config.Tokens = (_Config.Class)serializer.Deserialize(reader, typeof(_Config.Class));
                }
                if (_Config.Tokens.Discord == "")
                {
                    _Log.Error("Discord token not set");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
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
            private static Dictionary<ulong, Item> List = new Dictionary<ulong, Item>();
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
                Item Item = null;
                List.TryGetValue(ID, out Item);
                return Item;
            }
            /// <summary>
            /// Get all whitelist items
            /// </summary>
            public static Dictionary<ulong, Item> GetAll()
            {
                return List;
            }
            /// <summary>
            /// Check if blacklist has a guild ID
            /// </summary>
            public static bool Check(ulong ID)
            {
                if (List.Keys.Contains(ID))
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
                        Item Item = (Item)serializer.Deserialize(reader, typeof(Item));
                        List.Add(Item.GuildID, Item);
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
                List.Add(ID, NewBlacklist);
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
                List.Remove(ID);
                if (File.Exists(Path + $"{ID.ToString()}.json"))
                {
                    File.Delete(Path + $"{ID.ToString()}.json");
                }
            }
            public static void UpdateBlacklist(object sender, ElapsedEventArgs e)
            {
                Dictionary<ulong, Item> BlacklistCache = new Dictionary<ulong, Item>();
                foreach (var File in Directory.GetFiles(_Blacklist.Path))
                {
                    using (StreamReader reader = new StreamReader(File.Replace(_Blacklist.Path, "").Replace(".json", "")))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        Item Item = (Item)serializer.Deserialize(reader, typeof(Item));
                        BlacklistCache.Add(Item.GuildID, Item);
                    }

                }
                List.Clear();
                List = BlacklistCache;
            }
        }
        #endregion

        public async Task Login()
        {

            _Client.JoinedGuild += (g) => { var _ = Task.Run(() => JoinedGuild(g)); return Task.CompletedTask; };

            _Client.LeftGuild += (g) => { var _ = Task.Run(() => LeftGuild(g)); return Task.CompletedTask; };

            _Client.GuildAvailable += (g) => { var _ = Task.Run(() => Client_GuildAvailable(g)); return Task.CompletedTask; };


            _Client.Ready += ClientReady;

            _Client.Connected += Connected;
            _Client.Disconnected += Disconnected;


            await _Client.LoginAsync(TokenType.Bot, _Config.Tokens.Discord).ConfigureAwait(false);
            await _Client.StartAsync().ConfigureAwait(false);
            await Task.Delay(-1);
        }

        #region ClientEvents
        private async Task JoinedGuild(SocketGuild g)
        {
            _Utils.Json_Save(new _GuildCount() { server_count = _Client.Guilds.Count }, "GuildCount");

            if (!DevMode)
            {
                if (_Bot._Blacklist.Check(g.Id))
                {
                    _Bot._Blacklist.Item BlacklistItem = _Bot._Blacklist.Get(g.Id);
                    _Log.Blacklist($"Removed {g.Name} | {g.Id}" + Environment.NewLine + $"    Reason: {BlacklistItem.Reason}");
                    try
                    {
                        await g.DefaultChannel.SendMessageAsync($"This guild has been blacklisted for {BlacklistItem.Reason}.").ConfigureAwait(false);
                    }
                    catch { }

                    await g.LeaveAsync();
                }
                else if (g.Owner == null && !_Bot._Whitelist.Check(g.Id))
                {
                    _Log.Warning($"[Joined] {g.Name} | {g.Id} - NULL OWNER ({g.Users.Where(x => !x.IsBot).Count()}/{g.Users.Where(x => x.IsBot).Count()})");
                    try
                    {
                        await g.DefaultChannel.SendMessageAsync("This guild has no owner :( i have to leave. If this is an issue contact xXBuilderBXx#9113").ConfigureAwait(false);
                    }
                    catch { }
                    await g.LeaveAsync().ConfigureAwait(false);
                }
                else
                {
                    int Users = g.Users.Where(x => !x.IsBot).Count();
                    int Bots = g.Users.Where(x => x.IsBot).Count();
                    if (Bots * 100 / g.Users.Count() > 85)
                    {

                        _Log.Blacklist($"Removed {g.Name} | {g.Id}" + Environment.NewLine + $"    Reason: Bot collection guild");
                        try
                        {
                            await g.DefaultChannel.SendMessageAsync($"This guild has been blacklisted for Bot collection guild. If this is an issue please contact xXBuilderBXx#9113");
                        }
                        catch { }
                        _Bot._Blacklist.Add(g.Name, g.Id, "Bot collection guild", $"{Users}/{Bots}");
                        await g.LeaveAsync();

                    }
                    else
                    {
                        _Log.GuildJoined($"{g.Name} | {g.Id} - {g.Owner.Username}#{g.Owner.Discriminator} ({g.Users.Where(x => !x.IsBot).Count()}/{g.Users.Where(x => x.IsBot).Count()})");
                        await _Client.SetGameAsync(GetStatus());
                    }
                }
            }

        }

        private async Task LeftGuild(SocketGuild g)
        {
            _Utils.Json_Save(new _GuildCount() { server_count = _Client.Guilds.Count }, "GuildCount");
            if (!DevMode && _Bot._Blacklist.Check(g.Id))
            {
                await _Client.SetGameAsync(GetStatus());
            }
            if (g.Owner == null)
            {
                _Log.Warning($"[Left] ({_Client.Guilds.Count}) {g.Name} - NULL OWNER - ({g.Users.Where(x => !x.IsBot).Count()}/{g.Users.Where(x => x.IsBot).Count()})");
            }
            else
            {
                _Log.GuildLeft($"{g.Name} - {g.Owner.Username}#{g.Owner.Discriminator} ({g.Users.Where(x => !x.IsBot).Count()}/{g.Users.Where(x => x.IsBot).Count()})");
            }
        }

        private Task Connected()
        {
            Uptime.Start();
            if (DevMode == true)
            {
                Console.Title = $"[DevMode] {_Config.BotName} - Online!";
            }
            else
            {
                Console.Title = $"{_Config.BotName} - Online!";
            }
            _Log.Bot("CONNECTED");
            return Task.CompletedTask;
        }

        private Task Disconnected(Exception ex)
        {
            Uptime.Stop();
            if (DevMode == true)
            {
                Console.Title = $"[DevMode] {_Config.BotName} - Offline!";
            }
            else
            {
                Console.Title = $"{_Config.BotName} - Offline!";
            }
            if (ex.Message == null)
            {
                _Log.Warning("DISCONNECTED");
            }
            else
            {
                _Log.Warning("DISCONNECTED");
                _Log.Custom($"[Exception] {ex.Message}");
            }
            return Task.CompletedTask;
        }

        private async Task ClientReady()
        {
            if (!Ready)
            {
                Ready = true;
                _Utils.Json_Save(new _GuildCount() { server_count = _Client.Guilds.Count }, "GuildCount");
                _Log.Ok($"Ready in {_Client.Guilds.Count} guilds");
                if (!DevMode)
                {
                    await _Client.SetGameAsync(GetStatus());
                }
            }
        }

        private async Task Client_GuildAvailable(SocketGuild g)
        {
            if (!DevMode)
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
        #endregion
    }

}
namespace Bot.Services
{
    #region CommandService
    public class CommandHandler
    {
        private readonly DiscordSocketClient _Client;
        private CommandService _Commands;
        private IServiceProvider _Services;
        private readonly _Bot _Bot;
        public CommandHandler(_Bot Bot, DiscordSocketClient Client)
        {
            _Bot = Bot;
            _Client = Client;
        }
        public void AddServices(IServiceProvider services, CommandService Commands)
        {
            _Commands = Commands;
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
            if (message == null || message.Author.IsBot) return;
            int argPos = 0;
            if (!_Bot.DevMode && message.Content.StartsWith("bbtest") && message.Author.Id == 190590364871032834)
            {
                if (message.Content == "bbtest" || message.Content == "bbtest ")
                    await message.Channel.SendMessageAsync("Test");
                else
                    await message.Channel.SendMessageAsync(message.Content.Replace("bbtest ", ""));
            }
            if (!(message.HasStringPrefix(_Bot.Prefix, ref argPos) || !_Bot.DevMode && message.HasMentionPrefix(_Bot._Client.CurrentUser, ref argPos))) return;
            if (message.Channel is ITextChannel Chan)
            {
                IGuildUser Bot = null;
                if (_Bot.GetBot(Chan.GuildId) == null)
                {
                    Bot = await Chan.Guild.GetCurrentUserAsync();
                    _Bot.AddBot(Chan.GuildId, Bot);
                }
                else
                {
                    Bot = _Bot.GetBot(Chan.GuildId);
                }
                if (!Bot.GuildPermissions.EmbedLinks && !Bot.GetPermissions(Chan).EmbedLinks)
                {
                    await message.Channel.SendMessageAsync("```python" + Environment.NewLine + $"Bot requires permission \" Embed Links \"```");
                    return;
                }
            }
            if (_Bot.DisabledMessage != "" && message.Author.Id != 190590364871032834)
            {
                var embed = new EmbedBuilder()
                {
                    Title = "Disabled By Owner - xXBuilderBXx#9113",
                    Description = _Bot.DisabledMessage,
                    Color = new Color(200, 0, 0)
                };
                await message.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            CommandContext Context = new CommandContext(_Client, message);
            IResult result = await _Commands.ExecuteAsync(Context, argPos, _Services);
            if (result.IsSuccess)
            {
                _Log.Command(Context);
            }
            else
            {
                if (_Bot.DevMode)
                {
                    _Log.Error(result.ErrorReason);
                }
                if (result.ErrorReason.StartsWith("Custom - "))
                {
                    _Log.CommandError(result.ErrorReason.Replace("Custom - ", ""), Context);
                    await Context.Channel.SendMessageAsync($"`{result.ErrorReason.Replace("Custom - ", "")}`");
                    return;
                }
                _Log.CommandError(result.ErrorReason, Context);
                if (result.ErrorReason.Contains("Bot requires guild permission") || result.ErrorReason.Contains("User requires guild permission") || result.ErrorReason.Contains("Bot requires channel permission") || result.ErrorReason.Contains("User requires channel permission"))
                {
                    await Context.Channel.SendMessageAsync($"`{result.ErrorReason}`");
                }
                else if (result.ErrorReason == "Invalid context for command; accepted contexts: Guild")
                {

                    await Context.Channel.SendMessageAsync("`You need to use this command in a guild channel`");
                }
                else if (result.ErrorReason == "Invalid context for command; accepted contexts: DM")
                {
                    await Context.Channel.SendMessageAsync("`You need to use this command in a DM channel`");
                }

            }
        }
    }
    #endregion
}
namespace Bot.Commands
{
    #region CoreCommands
    public class Core : ModuleBase
    {
        private DiscordSocketClient _Client;
        private _Bot _Bot;
        public Core(_Bot Bot, DiscordSocketClient Client)
        {
            _Bot = Bot;
            _Client = Client;
        }

        [Command("ping"), Alias("botping", "pong")]
        public async Task Ping()
        {
            await ReplyAsync($":ping_pong: `PONG {_Client.Latency} ms`");
        }

        [Command("prefix")]
        public async Task Prefix()
        {
            await ReplyAsync($"My prefix is `{_Config.Prefix}help` or `@{_Bot._Client.CurrentUser.Username} help` custom prefix option coming soon");
        }

        [Command("invite")]
        public async Task Invite(string Test = "")
        {
            var embed = new EmbedBuilder()
            {
                Description = $"[Add {_Client.CurrentUser.Username} to your server/guild](https://discordapp.com/oauth2/authorize?&client_id=" + Context.Client.CurrentUser.Id + "&scope=bot&permissions=0)",
                Color = _Utils.Discord.GetRoleColor(Context.Channel)
            };
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("website")]
        public async Task Website()
        {
            var embed = new EmbedBuilder()
            {
                Description = $"[Visit my website](https://blazeweb.ml)",
                Color = _Utils.Discord.GetRoleColor(Context.Channel)
            };
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("github")]
        public async Task Github()
        {
            if (_Config.Github == "")
            {
                await ReplyAsync("`This bot does not have a github`");
                return;
            }
            var embed = new EmbedBuilder()
            {
                Description = $"Please report issues/suggestions to [Github]({_Config.Github})",
                Color = _Utils.Discord.GetRoleColor(Context.Channel)
            };
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Group("o"), Alias("owner")]
        public class OwnerGroup : ModuleBase
        {
            private _Bot _Bot;
            private CommandService _Commands;
            private DiscordSocketClient _Client;
            public OwnerGroup(_Bot Bot, CommandService Commands, DiscordSocketClient Client)
            {
                _Bot = Bot;
                _Client = Client;
                _Commands = Commands;
            }

            [Command]
            public async Task Owner()
            {
                if (Context.User.Id != 190590364871032834)
                {
                    EmbedBuilder Embed = new EmbedBuilder()
                    {
                        Title = "Bot Owner",
                        Description = "Contact me if there is a problem with the bot" + Environment.NewLine + "xXBuilderBXx#9113 <@190590364871032834>"
                    };
                    await ReplyAsync("", false, Embed.Build());
                    return;
                }
                HashSet<string> OwnerCommands = new HashSet<string>();
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
                    await ReplyAsync($"`{string.Join(" | ", OwnerCommands)} | blacklist | whitelist`").ConfigureAwait(false);
                }
            }

            [Command("enable"), Remarks("enable"), RequireOwner]
            public async Task Enable()
            {
                _Bot.DisabledMessage = "";
                await ReplyAsync($"`{_Config.BotName} has been enabled`");
            }

            [Command("disable"), Remarks("disable (Text)"), RequireOwner]
            public async Task Disable([Remainder]string Message = "")
            {
                if (Message == "")
                {
                    await ReplyAsync("`You need to enter a reason`");
                    return;
                }
                _Bot.DisabledMessage = Message;
                await ReplyAsync($"`{_Config.BotName} has been disabled`");
            }

            [Command("invite"), Remarks("invite (GID)"), RequireOwner]
            public async Task Invite(ulong ID)
            {
                if (ID == 0)
                {
                    ID = Context.Guild.Id;
                }
                IGuild Guild = _Client.GetGuild(ID);
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

            [Command("info"), Remarks("info (GID)"), RequireOwner]
            public async Task Oinfo(ulong ID = 0)
            {
                if (ID == 0)
                {
                    ID = Context.Guild.Id;
                }
                IGuild Guild = _Client.GetGuild(ID);
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
                    Description = "```md" + Environment.NewLine + $"<ID {Guild.Id}>" + Environment.NewLine + $"<Owner {Owner}>" + Environment.NewLine + $"<Users {Users.Where(x => !x.IsBot).Count()}> <Bots {Users.Where(x => x.IsBot).Count()}> <BotPercentage {_Utils.Discord.BotPercentage(Users.Count(), Users.Where(x => x.IsBot).Count())}>```",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Created {Guild.CreatedAt.Day}/{Guild.CreatedAt.Month}/{Guild.CreatedAt.Year}"
                    },
                    Color = _Utils.Discord.GetRoleColor(Context.Channel as ITextChannel)
                };
                await ReplyAsync("", false, embed.Build());
            }

            [Command("leavehere"), Remarks("leavehere"), RequireOwner]
            public async Task Leavehere()
            {
                await Context.Guild.LeaveAsync();
            }

            [Command("clearcon"), Remarks("clearcon"), RequireOwner]
            public async Task Clear()
            {
                Console.Clear();
                _Log.Custom("Console cleared", ConsoleColor.Cyan);
                await Context.Channel.SendMessageAsync("`Console cleared`").ConfigureAwait(false);
            }

            [Command("botcol"), Remarks("botcol (*int)"), RequireOwner]
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

            [Command("leave"), Remarks("leave (GID)"), RequireOwner]
            public async Task Leave(ulong ID)
            {
                IGuild Guild = _Client.GetGuild(ID);
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
                private _Bot _Bot;
                public WhitelistGroup(DiscordSocketClient Client, _Bot Bot)
                {
                    _Client = Client;
                    _Bot = Bot;
                }
                [Command, RequireOwner]
                public async Task Whitelist()
                {
                    if (_Bot.DevMode)
                    {
                        await ReplyAsync("`Cannot use whitelist in devmode`");
                        return;
                    }
                    await ReplyAsync("`Whitelist > add (ID) | reload | remove (ID) | list`");
                }

                [Command("add"), RequireOwner]
                public async Task WhitelistAdd(ulong ID)
                {
                    if (_Bot.DevMode)
                    {
                        await ReplyAsync("`Cannot use whitelist in devmode`"); return;
                    }
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

                [Command("reload"), RequireOwner]
                public async Task WhitelistReload()
                {
                    if (_Bot.DevMode)
                    {
                        await ReplyAsync("`Cannot use whitelist in devmode`"); return;
                    }
                    _Bot._Whitelist.Reload();
                    await ReplyAsync("`Whitelist reloaded`").ConfigureAwait(false);
                }

                [Command("remove"), RequireOwner]
                public async Task WhitelistRemove(ulong ID)
                {
                    if (_Bot.DevMode)
                    {
                        await ReplyAsync("`Cannot use whitelist in devmode`"); return;
                    }
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

                [Command("list"), RequireOwner]
                public async Task WhitelistList()
                {
                    if (_Bot.DevMode)
                    {
                        await ReplyAsync("`Cannot use whitelist in devmode`"); return;
                    }
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
                private _Bot _Bot;
                public BlacklistGroup(DiscordSocketClient CLient, _Bot Bot)
                {
                    _Client = CLient;
                    _Bot = Bot;
                }
                [Command, RequireOwner]
                public async Task Blacklist()
                {
                    if (_Bot.DevMode)
                    {
                        await ReplyAsync("`Cannot use blacklist in devmode`"); return;
                    }
                    await ReplyAsync("`Blacklist > add (ID) | reload | remove (ID) | list | info (ID)`");
                }

                [Command("add"), RequireOwner]
                public async Task BlacklistAdd(ulong ID, [Remainder] string Reason = "")
                {
                    if (_Bot.DevMode)
                    {
                        await ReplyAsync("`Cannot use blacklist in devmode`"); return;
                    }
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

                [Command("reload"), RequireOwner]
                public async Task BlacklistReload()
                {
                    if (_Bot.DevMode)
                    {
                        await ReplyAsync("`Cannot use blacklist in devmode`"); return;
                    }
                    _Bot._Blacklist.UpdateBlacklist(null, null);
                    await ReplyAsync("`Blacklist reloaded`").ConfigureAwait(false);
                }

                [Command("remove"), RequireOwner]
                public async Task BlacklistRemove(ulong ID)
                {
                    if (_Bot.DevMode)
                    {
                        await ReplyAsync("`Cannot use blacklist in devmode`"); return;
                    }
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

                [Command("list"), RequireOwner]
                public async Task BlacklistList()
                {
                    if (_Bot.DevMode)
                    {
                        await ReplyAsync("`Cannot use blacklist in devmode`"); return;
                    }
                    if (_Bot._Blacklist.Count() == 0)
                    {
                        await ReplyAsync("`Blacklist is empty`").ConfigureAwait(false);
                    }
                    else
                    {
                        _Utils.Json_Save(_Bot._Blacklist.GetAll().Values, "Blacklist");
                        await Context.Channel.SendFileAsync(_Config.BotPath + "Blacklist.json");
                        File.Delete(_Config.BotPath + "Blacklist.json");
                    }
                }

                [Command("info"), RequireOwner]
                public async Task BlacklistInfo(ulong ID)
                {
                    if (_Bot.DevMode)
                    {
                        await ReplyAsync("`Cannot use blacklist in devmode`"); return;
                    }
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

            [Command("say"), Remarks("say (Text)"), RequireOwner]
            public async Task Say([Remainder]string Message)
            {
                IGuildUser Bot = await Context.Guild.GetCurrentUserAsync();
                if (Bot.GuildPermissions.ManageMessages || Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages) await Context.Message.DeleteAsync();

                var embed = new EmbedBuilder()
                {
                    Description = Message,
                    Color = _Utils.Discord.GetRoleColor(Context.Channel as ITextChannel)
                };
                await ReplyAsync("", false, embed.Build());
            }

            [Command("csay"), Remarks("csay (CID) (Text)"), RequireOwner]
            public async Task SayChannel(ulong CID, [Remainder]string Message)
            {
                IGuildUser Bot = await Context.Guild.GetCurrentUserAsync();
                if (Bot.GuildPermissions.ManageMessages || Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages) await Context.Message.DeleteAsync();

                try
                {
                    ITextChannel Chan = await Context.Guild.GetTextChannelAsync(CID);
                    if (Chan == null)
                    {
                        await ReplyAsync($"Could not find channel `{CID}`");
                        return;
                    }
                    var embed = new EmbedBuilder()
                    {
                        Description = Message,
                        Color = _Utils.Discord.GetRoleColor(Chan as ITextChannel)
                    };
                    await Chan.SendMessageAsync("", false, embed.Build());
                }
                catch (Exception ex)
                {
                    await ReplyAsync(ex.Message);
                }
            }

            [Command("gsay"), Remarks("gsay (GID) (CID) (Text)"), RequireOwner]
            public async Task SayGuild(ulong GID, ulong CID, [Remainder]string Message)
            {
                IGuild Guild = _Client.GetGuild(GID);

                if (Guild == null)
                {
                    await ReplyAsync($"Could not find guild `{GID}`");
                    return;
                }
                ITextChannel Chan = await Guild.GetTextChannelAsync(CID);
                if (Chan == null)
                {
                    await ReplyAsync($"Could not find channel `{CID}`");
                    return;
                }
                IGuildUser Bot = await Context.Guild.GetCurrentUserAsync();
                if (Bot.GuildPermissions.ManageMessages || Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages) await Context.Message.DeleteAsync();

                try
                {
                    var embed = new EmbedBuilder()
                    {
                        Description = Message,
                        Color = _Utils.Discord.GetRoleColor(Chan as ITextChannel)
                    };
                    await Chan.SendMessageAsync("", false, embed.Build());
                }
                catch (Exception ex)
                {
                    await ReplyAsync(ex.Message);
                }
            }

            [Command("find"), Remarks("find (Name)"), RequireOwner]
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
                    Description = "```md" + Environment.NewLine + $"<ID {Guild.Id}>" + Environment.NewLine + $"<Owner {Owner}>" + Environment.NewLine + $"<Users {Users.Where(x => !x.IsBot).Count()}> <Bots {Users.Where(x => x.IsBot).Count()}> <BotPercentage {_Utils.Discord.BotPercentage(Users.Count(), Users.Where(x => x.IsBot).Count())}>```",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Created {Guild.CreatedAt.Day}/{Guild.CreatedAt.Month}/{Guild.CreatedAt.Year}"
                    },
                    Color = _Utils.Discord.GetRoleColor(Context.Channel as ITextChannel)
                };
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);

            }

            [Command("channels"), Remarks("channels (GID)"), RequireOwner]
            public async Task Channels(ulong ID = 0)
            {
                if (ID == 0)
                {
                    ID = Context.Guild.Id;
                }
                List<string> Channels = new List<string>();
                IGuild Guild = _Client.GetGuild(ID);

                if (Guild == null)
                {
                    await ReplyAsync($"`Could not find guild by id {ID}`");
                    return;
                }
                foreach (var Chan in await Guild.GetTextChannelsAsync())
                {
                    Channels.Add($"<[{Chan.Position}]{Chan.Name} {Chan.Id}>");
                }
                var embed = new EmbedBuilder()
                {
                    Description = "```md" + Environment.NewLine + string.Join(Environment.NewLine, Channels) + Environment.NewLine + "```",
                    Color = _Utils.Discord.GetRoleColor(Context.Channel as ITextChannel)
                };
                await Context.Channel.SendMessageAsync("", false, embed.Build());

            }

        }
    }
    #endregion
}
namespace Bot
{
    #region Utils
    public class _Utils
    {
        public class Discord
        {
            private readonly _Bot _Bot;
            public Discord(_Bot Bot)
            {
                _Bot = Bot;
            }
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
            public static async Task<IGuildUser> MentionGetUser(IGuild Guild, string Username)
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

            public static string MentionToID(string User)
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

            public static Color GetRoleColor(IChannel Channel)
            {
                Color RoleColor = new Color(30, 50, 200);
                if (Channel is ITextChannel Chan)
                {
                    if (_Bot.GetBot(Chan.GuildId) == null) return RoleColor;
                    if (Chan.Guild.Roles.Count() > 1 && _Bot.GetBot(Chan.GuildId).RoleIds.Count != 0)
                    {
                        foreach (var i in _Bot.GetBot(Chan.GuildId).Guild.Roles.Where(x => x.Id != Chan.Guild.EveryoneRole.Id && _Bot.GetBot(Chan.GuildId).RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position))
                        {
                            if (i.Color.R != 0 && i.Color.G != 0 && i.Color.B != 0)
                            {
                                RoleColor = i.Color;
                                break;
                            }
                        }

                    }

                }
                return RoleColor;
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
        public static string GetBetween(string content, string startString, string endString)
        {
            int Start = 0, End = 0;
            if (content.Contains(startString) && content.Contains(endString))
            {
                Start = content.IndexOf(startString, 0) + startString.Length;
                End = content.IndexOf(endString, Start);
                return content.Substring(Start, End - Start);
            }
            else
                return "";
        }

        public static void Json_Save(Object Data, string FileName)
        {
            using (StreamWriter file = File.CreateText(_Config.BotPath + FileName + ".json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Data);
            }
        }

        public class Http
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
                    else
                    {
                        _Log.ThrowError("API Error - Http GetString");
                        return "";
                    }
                }
                return Response;
            }
            public static dynamic JsonObject(string Url, string Auth = "", string OtherHeader = "", string OtherValue = "")
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
                        else
                        {
                            _Log.ThrowError("API Error - Http JsonObject");
                            return null;
                        }
                    }
                }
                catch
                {
                    return null;
                }
                return Response;
            }
            public static dynamic JsonArray(string Url, string Auth = "", string OtherHeader = "", string OtherValue = "")
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
                        else
                        {
                            _Log.ThrowError("API Error - Http JsonArray");
                            return null;
                        }
                    }
                }
                catch
                {
                    return null;
                }
                return Response;
            }
        }

        public static DateTime UnixToDateTime(long Unix)
        {
            DateTime DateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            DateTime = DateTime.AddSeconds(Unix).ToLocalTime();
            return DateTime;
        }
    }
    #endregion
}