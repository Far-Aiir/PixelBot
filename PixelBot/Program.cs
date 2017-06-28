using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Runtime.InteropServices;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Bot.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Timers;
using Newtonsoft.Json;
using System.Net;

namespace Bot
{
    public class Program
    {
        public static void Main()
        {
            Console.Title = "[DevMode] " + Config.BotName;
            DisableConsoleQuickEdit.Go();
            Directory.CreateDirectory(Config.BotPath);
            Config.ConfigLoad();
            if (File.Exists(Config.BotPath + "LIVE.txt"))
            {
                Console.Title = Config.BotName;
                Config.DevMode = false;
            }

            if (!File.Exists(Config.BotPath + "Config.json"))
            {
                Console.WriteLine("[Error] No config file");
                while (true)
                {

                }
            }
            if (Config._Configs.Discord == "")
            {
                Console.WriteLine("[Error] No discord token");
                while (true)
                {

                }
            }
            
            Console.WriteLine($"[{Config.BotName}] Starting");
            new Bot().RunAndBlockAsync().GetAwaiter().GetResult();
        }
    }
    public class Bot
    {
        public DiscordSocketClient _Client;
        public static IServiceProvider _Services;
        public CommandService _CommandService;
        public CommandHandler _CommandHandler;
        public BlacklistService _Blacklist;
        public static Dictionary<ulong, IGuildUser> GuildBotCache = new Dictionary<ulong, IGuildUser>();
        public ITextChannel BlacklistChannel = null;
        public bool FirstStart = false;
        public async Task RunAsync()
        {
            _Client = new DiscordSocketClient(new DiscordSocketConfig { AlwaysDownloadUsers = true, ConnectionTimeout = int.MaxValue, MessageCacheSize = 10 });
            _CommandService = new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, DefaultRunMode = RunMode.Async });
            _CommandHandler = new CommandHandler(_Client, _CommandService);
            _Blacklist = new BlacklistService();
            ServicePointManager.DefaultConnectionLimit = 6;
            Directory.CreateDirectory(Config.BotPath + "Users\\");
            _Services = BotServices.AddServices(_Client, _CommandService, _CommandHandler);
            var CmdHandler = _Services.GetService<CommandHandler>();
            var CmdService = _Services.GetService<CommandService>();
            await CmdHandler.StartHandling().ConfigureAwait(false);
            await CmdService.AddModulesAsync(Assembly.GetEntryAssembly()).ConfigureAwait(false);
            await _Client.LoginAsync(TokenType.Bot, Config._Configs.Discord).ConfigureAwait(false);
            await _Client.StartAsync().ConfigureAwait(false);
            _Client.Connected += () =>
            {
                Console.WriteLine($"[{Config.BotName}] Connected");
                if (Config.BlacklistChannel == null)
                {
                    IGuild Guild = _Client.GetGuild(275054291360940032);
                    Config.BlacklistChannel = Guild.GetTextChannelAsync(327398925889961984).GetAwaiter().GetResult();
                }
                return Task.CompletedTask;
            };

            _Client.Disconnected += (ex) =>
            {
                Console.WriteLine($"[{Config.BotName}] Disconnected");
                return Task.CompletedTask;
            };

            _Client.Ready += () =>
            {
                Console.WriteLine($"[{Config.BotName}] Ready in {_Client.Guilds.Count} guilds");
                return Task.CompletedTask;
            };

            _Client.GuildAvailable += (g) =>
            {
                if (Config.DevMode == false && g.Id != 272248892161261569 && g.Owner == null)
                {
                    g.DefaultChannel.SendMessageAsync($"Removed guild > Null owner").GetAwaiter();
                    Console.WriteLine($"[Null Owner] {g.Name} - {g.Id}");
                    g.LeaveAsync().GetAwaiter();
                    return Task.CompletedTask;
                }
                int Users = g.Users.Where(x => !x.IsBot).Count();
                int Bots = g.Users.Where(x => x.IsBot).Count();
                if (Config.DevMode == false && _Blacklist.Blacklist.Exists(x => x.GuildID == g.Id))
                {
                    BlacklistService.BlacklistClass BlacklistItem = _Blacklist.Blacklist.Find(x => x.GuildID == g.Id);
                    Console.WriteLine($"[Blacklist] Removed {g.Name} - {g.Id}" + Environment.NewLine + $"    Reason: {BlacklistItem.Reason}");
                    g.DefaultChannel.SendMessageAsync($"Removed guild > {BlacklistItem.Reason}").GetAwaiter();
                    g.LeaveAsync().GetAwaiter();
                    if (Config.BlacklistChannel != null)
                    {
                        var embed = new EmbedBuilder()
                        {
                            Title = g.Name,
                            Description = $"Users {Users}/{Bots} Bots",
                            Footer = new EmbedFooterBuilder()
                            {
                                Text = g.Id.ToString()
                            }
                        };
                        Config.BlacklistChannel.SendMessageAsync("", false, embed).GetAwaiter();
                    }
                    return Task.CompletedTask;
                }

                if (Config.DevMode == false && Bots * 100 / g.Users.Count() > 90)
                {
                    Console.WriteLine($"[Blacklist] Removed {g.Name} - {g.Id}" + Environment.NewLine + $"    Reason: Bot collection guild");
                    g.DefaultChannel.SendMessageAsync($"Removed guild > Bot collection guild").GetAwaiter();
                    _Blacklist.BlacklistAdd(g.Name, g.Id, "Bot collection guild", $"{Users}/{Bots}");

                    g.LeaveAsync().GetAwaiter();
                    if (Config.BlacklistChannel != null)
                    {
                        var embed = new EmbedBuilder()
                        {
                            Title = g.Name,
                            Description = $"Users {Users}/{Bots} Bots",
                            Footer = new EmbedFooterBuilder()
                            {
                                Text = g.Id.ToString()
                            }
                        };
                        Config.BlacklistChannel.SendMessageAsync("", false, embed).GetAwaiter();
                    }
                }
                else
                {
                    if (Config.DevMode == false)
                    {
                        _Client.SetGameAsync($"{Config.Prefix}/help | {_Client.Guilds.Count} Guilds | https://blaze.ml ").GetAwaiter();
                    }
                    IGuildUser BotUser = g.GetUser(_Client.CurrentUser.Id);
                    Bot.GuildBotCache.Add(g.Id, BotUser);
                }
                return Task.CompletedTask;
            };

            _Client.JoinedGuild += (g) =>
            {
                if (Config.DevMode == false && g.Id != 272248892161261569 && g.Owner == null)
                {
                    g.DefaultChannel.SendMessageAsync($"Removed guild > Null owner").GetAwaiter();
                    Console.WriteLine($"[Null Owner] {g.Name} - {g.Id}");
                    g.LeaveAsync().GetAwaiter();
                    return Task.CompletedTask;
                }
                int Users = g.Users.Where(x => !x.IsBot).Count();
                int Bots = g.Users.Where(x => x.IsBot).Count();
                if (Config.DevMode == false && _Blacklist.Blacklist.Exists(x => x.GuildID == g.Id))
                {
                    BlacklistService.BlacklistClass BlacklistItem = _Blacklist.Blacklist.Find(x => x.GuildID == g.Id);
                    Console.WriteLine($"[Blacklist] Removed {g.Name} - {g.Id}" + Environment.NewLine + $"    Reason: {BlacklistItem.Reason}");
                    g.DefaultChannel.SendMessageAsync($"Removed guild > {BlacklistItem.Reason}").GetAwaiter();
                    g.LeaveAsync().GetAwaiter();
                    if (Config.BlacklistChannel != null)
                    {
                        var embed = new EmbedBuilder()
                        {
                            Title = g.Name,
                            Description = $"Users {Users}/{Bots} Bots",
                            Footer = new EmbedFooterBuilder()
                            {
                                Text = g.Id.ToString()
                            }
                        };
                        Config.BlacklistChannel.SendMessageAsync("", false, embed).GetAwaiter();
                    }
                    return Task.CompletedTask;
                }

                if (Config.DevMode == false && Bots * 100 / g.Users.Count() > 90)
                {
                    Console.WriteLine($"[Blacklist] Removed {g.Name} - {g.Id}" + Environment.NewLine + $"    Reason: Bot collection guild");
                    g.DefaultChannel.SendMessageAsync($"Removed guild > Bot collection guild").GetAwaiter();
                    _Blacklist.BlacklistAdd(g.Name, g.Id, "Bot collection guild", $"{Users}/{Bots}");
                    g.LeaveAsync().GetAwaiter();
                    if (Config.BlacklistChannel != null)
                    {
                        var embed = new EmbedBuilder()
                        {
                            Title = g.Name,
                            Description = $"Users {Users}/{Bots} Bots",
                            Footer = new EmbedFooterBuilder()
                            {
                                Text = g.Id.ToString()
                            }
                        };
                        Config.BlacklistChannel.SendMessageAsync("", false, embed).GetAwaiter();
                    }
                }
                else
                {
                    if (Config.DevMode == false)
                    {
                        _Client.SetGameAsync($"{Config.Prefix}/help | {_Client.Guilds.Count} Guilds | https://blaze.ml ").GetAwaiter();
                    }
                    IGuildUser BotUser = g.GetUser(_Client.CurrentUser.Id);
                    Bot.GuildBotCache.Add(g.Id, BotUser);
                    Console.WriteLine($"[Joined] {g.Name} - {g.Id}");
                }
                return Task.CompletedTask;
            };

            _Client.LeftGuild += (g) =>
            {
                GuildBotCache.Remove(g.Id);
                Console.WriteLine($"[Left] > {g.Name} - {g.Id}");
                if (Config.DevMode == false)
                {
                    _Client.SetGameAsync($"{Config.Prefix}/help | {_Client.Guilds.Count} Guilds | https://blaze.ml ").GetAwaiter();
                }
                return Task.CompletedTask;

            };
            
            _Client.Ready += () =>
            {
                if (Config.DevMode == false)
                {
                    _Client.SetGameAsync($"{Config.Prefix}/help | {_Client.Guilds.Count} Guilds | https://blaze.ml").GetAwaiter();

                    if (FirstStart == false)
                    {
                        Utils.DiscordUtils.UpdateUptimeGuilds();
                    }
                }
                FirstStart = true;
                return Task.CompletedTask;
            };

            await Task.Delay(-1);
        }

        public async Task RunAndBlockAsync()
        {
            await RunAsync().ConfigureAwait(false);
            await Task.Delay(-1).ConfigureAwait(false);
        }


    }
    #region CommandHandler
    public class CommandHandler
    {
        public readonly DiscordSocketClient _Client;
        public readonly CommandService _CommandService;
        public IServiceProvider _Services;
        public CommandHandler(DiscordSocketClient client, CommandService commandservice)
        {
            _Client = client;
            _CommandService = commandservice;
        }
        public Task StartHandling()
        {
            _Client.MessageReceived += (msg) => { var _ = Task.Run(() => HandleCommand(msg)); return Task.CompletedTask; };
            return Task.CompletedTask;
        }
        public void AddServices(IServiceProvider services)
        {
            _Services = services;
        }
        public async Task HandleCommand(SocketMessage messageParam)
        {

            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.IsBot) return;
            int argPos = 0;
            if (Config.DevMode == false)
            {
                if (!(message.HasStringPrefix(Config.Prefix, ref argPos) || message.HasMentionPrefix(_Client.CurrentUser, ref argPos))) return;
                var context = new CommandContext(_Client, message);

                var result = await _CommandService.ExecuteAsync(context, argPos, _Services);
                if (result.IsSuccess)
                {
                    if (context.Channel is IPrivateChannel)
                    {
                        Console.WriteLine($"[Command] Executed DM");
                        Console.WriteLine($"     {context.Message.Author}: {context.Message.Content}");
                    }
                    else
                    {
                        Console.WriteLine($"[Command] Executed {context.Guild.Name} #{context.Channel.Name}");
                        Console.WriteLine($"     {context.Message.Author}: {context.Message.Content}");
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
                if (!(message.HasStringPrefix(Config.DevPrefix, ref argPos))) return;
                var context = new CommandContext(_Client, message);

                var result = await _CommandService.ExecuteAsync(context, argPos, _Services);
                if (result.IsSuccess)
                {
                    Console.WriteLine($"[Command] Executed > {context.Message.Content}");
                }
                else
                {
                    Console.WriteLine($"[Command] Error > {context.Message.Content}");
                    Console.WriteLine($"     Error: {result.ErrorReason}");
                }
            }

        }
    }
    #endregion

    #region BlacklistService
    public class BlacklistService
    {
        public class BlacklistClass
        {
            public string GuildName = "";
            public ulong GuildID = 0;
            public string Reason = "";
            public string UsersToBots = "";
        }
        public List<BlacklistClass> Blacklist = new List<BlacklistClass>();
        public BlacklistService()
        {
            Timer Timer = new Timer();
            Timer.Interval = 300000;
            Timer.Elapsed += UpdateBlacklist;
            Timer.Start();
            foreach (var File in Directory.GetFiles(Config.PathBlacklist))
            {
                using (StreamReader reader = new StreamReader(File))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Blacklist.Add((BlacklistService.BlacklistClass)serializer.Deserialize(reader, typeof(BlacklistService.BlacklistClass)));
                }
            }
        }

        public void BlacklistAdd(string GuildName = "", ulong ID = 0, string Reason = "", string UsersBots = "")
        {
            BlacklistService.BlacklistClass NewBlacklist = new BlacklistService.BlacklistClass()
            {
                GuildID = ID, Reason = Reason, GuildName = GuildName, UsersToBots = UsersBots
            };
            Blacklist.Add(NewBlacklist);
            using (StreamWriter file = File.CreateText(Config.PathBlacklist + $"{ID.ToString()}.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, NewBlacklist);
            }
        }
        public void UpdateBlacklist(object sender, ElapsedEventArgs e)
        {
            string Dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Blacklist/";
            foreach (var File in Directory.GetFiles(Dir))
            {
                string FileName = File.Replace(Dir, "").Replace(".json", "");
                if (!Blacklist.Exists(x => x.GuildID.ToString() == FileName))
                {
                    using (StreamReader reader = new StreamReader(File))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        Blacklist.Add((BlacklistClass)serializer.Deserialize(reader, typeof(BlacklistClass)));
                    }
                }
            }
        }
    }
    #endregion

    #region OwnerCommands
    public class OwnerModule : ModuleBase
    {
        [Command("owner")]
        public async Task Owner()
        {
            var embed = new EmbedBuilder()
            {
                Title = "xXBuilderBXx#9113 owns this bot",
                Description = "<@190590364871032834>",
                Color = Utils.DiscordUtils.GetRoleColor(Context)
            };
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Group("o")]
        public class OwnerGroup : ModuleBase
        {
            [Command]
            [RequireOwner]
            public async Task OwnerList()
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync("`chatlog (ID) | invite (ID) | info (ID) | leavehere | leave (ID) | botcol | clear | blacklist | toggle`");
            }


            [Command("chatlog")]
            [RequireOwner]
            public async Task Chatlog(ulong ID = 0)
            {
                if (ID == 0)
                {
                    Config.ChatlogGuild = 0;
                    await Context.Channel.SendMessageAsync("`Chat log has been turned off`");
                }
                else
                {
                    if (ID == 1)
                    {
                        Config.ChatlogGuild = 1;
                        await Context.Channel.SendMessageAsync("`Chat log has been set to ALL`");
                    }
                    else
                    {
                        Config.ChatlogGuild = ID;
                        await Context.Channel.SendMessageAsync($"`Chat log has been set to {ID}`");
                    }
                }

            }

            [Command("invite")]
            [RequireOwner]
            public async Task Invite(ulong ID)
            {
                IGuild Guild = await Context.Client.GetGuildAsync(ID);
                IGuildChannel Chan = await Guild.GetDefaultChannelAsync();
                var Invite = await Chan.CreateInviteAsync();
                await Context.Channel.SendMessageAsync(Invite.Code);
            }

            [Command("info")]
            [RequireOwner]
            public async Task Oinfo(ulong ID)
            {
                try
                {
                    var Guild = await Context.Client.GetGuildAsync(ID);
                    string Owner = "NO OWNER";
                    var Users = await Guild.GetUsersAsync();
                    try
                    {
                        IGuildUser ThisOwner = await Guild.GetOwnerAsync();
                        Owner = $"{ThisOwner.Username}#{ThisOwner.Discriminator} - {ThisOwner.Id}";
                    }
                    catch
                    {

                    }
                    var embed = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = $"{Guild.Name}",
                            IconUrl = Guild.IconUrl
                        },
                        Description = $"Owner: {Owner}" + Environment.NewLine + $"Users {Users.Where(x => !x.IsBot).Count()}/{Users.Where(x => x.IsBot).Count()} Bots",
                        Color = Utils.DiscordUtils.GetRoleColor(Context),
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = $"Created {Guild.CreatedAt.Day}/{Guild.CreatedAt.Month}/{Guild.CreatedAt.Year}"
                        }
                    };
                    await Context.Channel.SendMessageAsync("", false, embed);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync($"`Cannot find guild {ID}`");
                }
            }

            [Command("leavehere")]
            [RequireOwner]
            public async Task Leavehere()
            {
                await Context.Guild.LeaveAsync();
            }

            [Command("clear")]
            [RequireOwner]
            public async Task Clear()
            {
                await Context.Message.DeleteAsync();
                Console.Clear();
                Console.WriteLine("Console cleared");
                await Context.Channel.SendMessageAsync("`Console cleared`");
            }

            [Command("botcol")]
            [RequireOwner]
            public async Task Botcol(int Number = 100)
            {
                await Context.Message.DeleteAsync();
                var Guilds = await Context.Client.GetGuildsAsync();
                List<string> GuildList = new List<string>();
                foreach (var Guild in Guilds)
                {
                    if (Guild.Id == 110373943822540800 || Guild.Id == 264445053596991498)
                    {

                    }
                    else
                    {
                        IGuildUser Owner = null;
                        try
                        {
                            Owner = await Guild.GetOwnerAsync();
                            var Users = await Guild.GetUsersAsync();
                            if (Users.Count(x => x.IsBot) >= Number || Users.Count(x => !x.IsBot) == 1)
                            {
                                GuildList.Add($"{Guild.Name} ({Guild.Id}) - Owner: {Owner.Username} ({Owner.Id}) - {Users.Where(x => !x.IsBot).Count()}/{Users.Where(x => x.IsBot).Count()}");
                            }
                        }
                        catch
                        {
                            GuildList.Add($"{Guild.Name} ({Guild.Id}) - NO OWNER!");
                        }
                    }

                }
                string AllGuilds = string.Join(Environment.NewLine, GuildList.ToArray());
                IDMChannel DM = await Context.User.CreateDMChannelAsync();
                foreach (var g in GuildList)
                {
                    await DM.SendMessageAsync(g);
                }
            }

            [Command("leave")]
            [RequireOwner]
            public async Task Leave(ulong ID)
            {
                await Context.Message.DeleteAsync();
                IGuild Guild = null;
                try
                {
                    Guild = await Context.Client.GetGuildAsync(ID);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync($"`Could not find guild by id {ID}`");
                    return;
                }
                try
                {
                    IGuildUser Owner = await Guild.GetOwnerAsync();
                    await Guild.LeaveAsync();
                    await Context.Channel.SendMessageAsync($"`Left guild {Guild.Name} - {Guild.Id} | Owned by {Owner.Username}#{Owner.Discriminator}`");
                }
                catch
                {
                    await Guild.LeaveAsync();
                    await Context.Channel.SendMessageAsync($"`Left guild {Guild.Name} - {Guild.Id}`");
                }
            }

            [Group("blacklist")]
            public class BlacklistGroup : ModuleBase
            {
                public readonly BlacklistService _BlacklistService;
                public BlacklistGroup(BlacklistService blacklistservice)
                {
                    _BlacklistService = blacklistservice;
                }
                [Command]
                public async Task Blacklist()
                {
                    await ReplyAsync("Blacklist > add | reload | remove | list | info").ConfigureAwait(false);
                }

                [Command("add")]
                public async Task BlacklistAdd(ulong ID, [Remainder] string Reason = "")
                {
                    if (_BlacklistService.Blacklist.Exists(x => x.GuildID == ID))
                    {
                        await Context.Channel.SendMessageAsync($"{ID} is already in the blacklist");
                    }
                    else
                    {
                        try
                        {
                            IGuild Guild = await Context.Client.GetGuildAsync(ID);
                            var Users = await Guild.GetUsersAsync();
                            _BlacklistService.BlacklistAdd(Guild.Name, ID, Reason, $"{Users.Where(x => !x.IsBot).Count()}/{Users.Where(x => x.IsBot).Count()}");
                            await Context.Channel.SendMessageAsync($"`Adding {Guild.Name} - {ID} to blacklist`");
                            await Guild.LeaveAsync();
                        }
                        catch
                        {
                            _BlacklistService.BlacklistAdd("", ID, Reason);
                            await Context.Channel.SendMessageAsync($"`Adding {ID} to blacklist`");
                        }
                        
                    }
                }

                [Command("reload")]
                public async Task BlacklistReload()
                {
                    List<BlacklistService.BlacklistClass> BlacklistCache = new List<BlacklistService.BlacklistClass>();
                    foreach (var File in Directory.GetFiles(Config.PathBlacklist))
                    {
                        using (StreamReader reader = new StreamReader(File))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            BlacklistCache.Add((BlacklistService.BlacklistClass)serializer.Deserialize(reader, typeof(BlacklistService.BlacklistClass)));
                        }

                    }
                    _BlacklistService.Blacklist.Clear();
                    _BlacklistService.Blacklist = BlacklistCache;
                }

                [Command("remove")]
                public async Task BlacklistRemove(ulong ID)
                {
                    BlacklistService.BlacklistClass Item = _BlacklistService.Blacklist.Find(x => x.GuildID == ID);
                    if (Item == null)
                    {
                        await ReplyAsync("`Could not find guild ID`");
                    }
                    else
                    {
                        _BlacklistService.Blacklist.Remove(Item);
                        if (File.Exists(Config.PathBlacklist + $"{Item.GuildID.ToString()}.json"))
                        {
                            File.Delete(Config.PathBlacklist + $"{Item.GuildID.ToString()}.json");
                        }
                        await ReplyAsync($"Remove {Item.GuildName} - {Item.GuildID} from blacklist");
                    }
                }

                [Command("list")]
                public async Task BlacklistList()
                {
                    if (_BlacklistService.Blacklist.Count == 0)
                    {
                        await ReplyAsync("`Blacklist is empty`");
                    }
                    else
                    {
                        List<string> BlacklistItems = new List<string>();
                        foreach (var i in _BlacklistService.Blacklist)
                        {
                            BlacklistItems.Add($"{i.GuildName} - {i.GuildID} ({i.UsersToBots})");
                        }
                        await ReplyAsync("```" + Environment.NewLine + string.Join(Environment.NewLine, BlacklistItems) + "```");
                    }
                }

                [Command("info")]
                public async Task BlacklistInfo(ulong ID)
                {
                    BlacklistService.BlacklistClass Item = _BlacklistService.Blacklist.Find(x => x.GuildID == ID);
                    if (Item == null)
                    {
                        await ReplyAsync("`Could not find guild ID`");
                    }
                    else
                    {
                        await ReplyAsync($"{Item.GuildName} - {Item.GuildID} ({Item.UsersToBots})" + Environment.NewLine + Item.Reason);
                    }
                }
            }
        }
    }
    #endregion
}


#region ConsoleFix
static class DisableConsoleQuickEdit
{
    const uint ENABLE_QUICK_EDIT = 0x0040;
    const int STD_INPUT_HANDLE = -10;
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);
    [DllImport("kernel32.dll")]
    static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
    [DllImport("kernel32.dll")]
    static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
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
