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
            new Bot().RunAsync().GetAwaiter().GetResult();
        }
    }
    public class Bot
    {
        public DiscordSocketClient _Client;
        public static IServiceProvider _Services;
        public CommandService _CommandService;
        public CommandHandler _CommandHandler;
        public static List<BlacklistClass> Blacklist = new List<BlacklistClass>();
        public static Dictionary<ulong, IGuildUser> GuildBotCache = new Dictionary<ulong, IGuildUser>();
        public ITextChannel BlacklistChannel = null;
        public bool FirstStart = false;
        public async Task RunAsync()
        {
            _Client = new DiscordSocketClient(new DiscordSocketConfig { AlwaysDownloadUsers = true, ConnectionTimeout = int.MaxValue, MessageCacheSize = 10 });

            BlacklistStart();
            
            _Client.Connected += () =>
            {
                Console.WriteLine($"[{Config.BotName}] Connected");
                if (BlacklistChannel == null)
                {
                    IGuild Guild = _Client.GetGuild(275054291360940032);
                    BlacklistChannel = Guild.GetTextChannelAsync(327398925889961984).ConfigureAwait(false).GetAwaiter().GetResult();
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
                    g.DefaultChannel.SendMessageAsync($"Removed guild > Null owner").ConfigureAwait(false).GetAwaiter();
                    Console.WriteLine($"[Null Owner] {g.Name} - {g.Id}");
                    g.LeaveAsync().ConfigureAwait(false).GetAwaiter();
                }
                else
                {
                    int Users = g.Users.Where(x => !x.IsBot).Count();
                        int Bots = g.Users.Where(x => x.IsBot).Count();
                        if (Config.DevMode == false && Blacklist.Exists(x => x.GuildID == g.Id))
                        {
                            BlacklistClass BlacklistItem = Blacklist.Find(x => x.GuildID == g.Id);
                            Console.WriteLine($"[Blacklist] Removed {g.Name} - {g.Id}" + Environment.NewLine + $"    Reason: {BlacklistItem.Reason}");
                            g.DefaultChannel.SendMessageAsync($"Removed guild > {BlacklistItem.Reason}").ConfigureAwait(false).GetAwaiter();
                            g.LeaveAsync().ConfigureAwait(false).GetAwaiter();
                            if (BlacklistChannel != null)
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
                                BlacklistChannel.SendMessageAsync("", false, embed).ConfigureAwait(false).GetAwaiter();
                            }
                        }
                        else
                        {
                            if (Config.DevMode == false && Bots * 100 / g.Users.Count() > 85)
                            {
                                Console.WriteLine($"[Blacklist] Removed {g.Name} - {g.Id}" + Environment.NewLine + $"    Reason: Bot collection guild");
                                g.DefaultChannel.SendMessageAsync($"Removed guild > Bot collection guild").ConfigureAwait(false).GetAwaiter();
                                BlacklistAdd(g.Name, g.Id, "Bot collection guild", $"{Users}/{Bots}");

                                g.LeaveAsync().ConfigureAwait(false).GetAwaiter();
                                if (BlacklistChannel != null)
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
                                    BlacklistChannel.SendMessageAsync("", false, embed).ConfigureAwait(false).GetAwaiter();
                                }
                            }
                            else
                            {
                                IGuildUser BotUser = g.GetUser(_Client.CurrentUser.Id);
                            GuildBotCache.Add(g.Id, BotUser);
                            }
                        }
                }
                return Task.CompletedTask;
            };

            _Client.JoinedGuild += (g) =>
            {
                if (Config.DevMode == false && g.Id != 272248892161261569 && g.Owner == null)
                {
                    g.DefaultChannel.SendMessageAsync($"Removed guild > Null owner").ConfigureAwait(false).GetAwaiter();
                    Console.WriteLine($"[Null Owner] {g.Name} - {g.Id}");
                    g.LeaveAsync().ConfigureAwait(false).GetAwaiter();
                }
                else
                {
                    int Users = g.Users.Where(x => !x.IsBot).Count();
                    int Bots = g.Users.Where(x => x.IsBot).Count();
                    if (Config.DevMode == false && Blacklist.Exists(x => x.GuildID == g.Id))
                    {
                        BlacklistClass BlacklistItem = Blacklist.Find(x => x.GuildID == g.Id);
                        Console.WriteLine($"[Blacklist] Removed {g.Name} - {g.Id}" + Environment.NewLine + $"    Reason: {BlacklistItem.Reason}");
                        g.DefaultChannel.SendMessageAsync($"Removed guild > {BlacklistItem.Reason}").ConfigureAwait(false).GetAwaiter();
                        g.LeaveAsync().ConfigureAwait(false).GetAwaiter();
                        if (BlacklistChannel != null)
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
                            BlacklistChannel.SendMessageAsync("", false, embed).ConfigureAwait(false).GetAwaiter();
                        }
                    }
                    else
                    {

                        if (Config.DevMode == false && Bots * 100 / g.Users.Count() > 85)
                        {
                            Console.WriteLine($"[Blacklist] Removed {g.Name} - {g.Id}" + Environment.NewLine + $"    Reason: Bot collection guild");
                            g.DefaultChannel.SendMessageAsync($"Removed guild > Bot collection guild").ConfigureAwait(false).GetAwaiter();
                            BlacklistAdd(g.Name, g.Id, "Bot collection guild", $"{Users}/{Bots}");
                            g.LeaveAsync().ConfigureAwait(false).GetAwaiter();
                            if (BlacklistChannel != null)
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
                                BlacklistChannel.SendMessageAsync("", false, embed).ConfigureAwait(false).GetAwaiter();
                            }
                        }
                        else
                        {
                            if (Config.DevMode == false)
                            {
                                _Client.SetGameAsync($"{Config.Prefix}help | {_Client.Guilds.Count} Guilds | https://blaze.ml ").ConfigureAwait(false).GetAwaiter();
                            }
                            IGuildUser BotUser = g.GetUser(_Client.CurrentUser.Id);
                            Bot.GuildBotCache.Add(g.Id, BotUser);
                            Console.WriteLine($"[Joined] {g.Name} - {g.Id}");
                        }
                    }
                }
                return Task.CompletedTask;
            };

            _Client.LeftGuild += (g) =>
            {
                GuildBotCache.Remove(g.Id);
                Console.WriteLine($"[Left] > {g.Name} - {g.Id}");
                if (Config.DevMode == false)
                {
                    _Client.SetGameAsync($"{Config.Prefix}help | {_Client.Guilds.Count} Guilds | https://blaze.ml ").ConfigureAwait(false).GetAwaiter();
                }
                return Task.CompletedTask;

            };
            
            _Client.Ready += async () =>
            {
                if (Config.DevMode == false)
                {
                   await _Client.SetGameAsync($"{Config.Prefix}help | {_Client.Guilds.Count} Guilds | https://blaze.ml").ConfigureAwait(false);
                    if (FirstStart == false)
                    {
                            var Dbots = _Client.GetGuild(110373943822540800) ?? null;
                            if (Dbots != null)
                            {
                                await Dbots.DownloadUsersAsync().ConfigureAwait(false);
                            }
                            var DbotsV2 = _Client.GetGuild(264445053596991498) ?? null;
                        if (DbotsV2 != null)
                        {
                            await DbotsV2.DownloadUsersAsync().ConfigureAwait(false);
                        }
                    }
                }

                FirstStart = true;
            };
            
            await _Client.LoginAsync(TokenType.Bot, Config._Configs.Discord).ConfigureAwait(false);
            await _Client.StartAsync().ConfigureAwait(false);
            _CommandService = new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, DefaultRunMode = RunMode.Async });
            _CommandHandler = new CommandHandler(_Client, _CommandService);
            _Services = BotServices.AddServices(_Client, _CommandService, _CommandHandler);

            
            ServicePointManager.DefaultConnectionLimit = 6;
            Directory.CreateDirectory(Config.BotPath + "Users\\");
            
            var CmdHandler = _Services.GetService<CommandHandler>();
            var CmdService = _Services.GetService<CommandService>();
            CmdHandler.AddServices(_Services);
            await CmdHandler.StartHandling().ConfigureAwait(false);
            await CmdService.AddModulesAsync(Assembly.GetEntryAssembly()).ConfigureAwait(false);
            await Task.Delay(-1).ConfigureAwait(false);
        }
        #region Blacklist
        public class BlacklistClass
        {
            public string GuildName = "";
            public ulong GuildID = 0;
            public string Reason = "";
            public string UsersToBots = "";
        }
        public void BlacklistStart()
        {
            foreach (var File in Directory.GetFiles(Config.PathBlacklist))
            {
                using (StreamReader reader = new StreamReader(File))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Blacklist.Add((BlacklistClass)serializer.Deserialize(reader, typeof(BlacklistClass)));
                }
            }
            Timer Timer = new Timer();
            Timer.Interval = 300000;
            Timer.Elapsed += UpdateBlacklist;
            Timer.Start();

        }

        public static void BlacklistAdd(string GuildName = "", ulong ID = 0, string Reason = "", string UsersBots = "")
        {
            BlacklistClass NewBlacklist = new BlacklistClass()
            {
                GuildID = ID,
                Reason = Reason,
                GuildName = GuildName,
                UsersToBots = UsersBots
            };
            Blacklist.Add(NewBlacklist);
            using (StreamWriter file = File.CreateText(Config.PathBlacklist + $"{ID.ToString()}.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, NewBlacklist);
            }
        }
        public static void UpdateBlacklist(object sender, ElapsedEventArgs e)
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
        #endregion

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
        public void AddServices(IServiceProvider services)
        {
            _Services = services;
        }
        public Task StartHandling()
        {
            _Client.MessageReceived += (msg) => { var _ = Task.Run(() => HandleCommand(msg)); return Task.CompletedTask; };
            return Task.CompletedTask;
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

    #region OwnerCommands
    public class OwnerModule : ModuleBase
    {
        [Command("owner")]
        public async Task Owner()
        {
            var embed = new EmbedBuilder()
            {
                Title = "Bot Owner",
                Description = "xXBuilderBXx#9113 - <@190590364871032834>",
                Color = Utils.DiscordUtils.GetRoleColor(Context.Channel as ITextChannel)
            };
            await ReplyAsync("", false, embed).ConfigureAwait(false);
        }

        [Group("o")]
        public class OwnerGroup : ModuleBase
        {
            [Command]
            [RequireOwner]
            public async Task OwnerList()
            {
                await ReplyAsync("`chatlog (ID) | invite (ID) | info (ID) | leavehere | leave (ID) | botcol | clear | blacklist | toggle`").ConfigureAwait(false);
            }
            
            [Command("chatlog")]
            [RequireOwner]
            public async Task Chatlog(ulong ID = 0)
            {
                if (ID == 0)
                {
                    Config.ChatlogGuild = 0;
                    await ReplyAsync("`Chat log has been turned off`").ConfigureAwait(false);
                }
                else
                {
                    if (ID == 1)
                    {
                        Config.ChatlogGuild = 1;
                        await ReplyAsync("`Chat log has been set to ALL`").ConfigureAwait(false);
                    }
                    else
                    {
                        Config.ChatlogGuild = ID;
                        await ReplyAsync($"`Chat log has been set to {ID}`").ConfigureAwait(false);
                    }
                }

            }

            [Command("invite")]
            [RequireOwner]
            public async Task Invite(ulong ID)
            {
                IGuild Guild = await Context.Client.GetGuildAsync(ID).ConfigureAwait(false) ?? null;
                if (Guild == null)
                {
                    await ReplyAsync($"`Cannot find guild {ID}`").ConfigureAwait(false);
                    return;
                }
                var Invites = await Guild.GetInvitesAsync().ConfigureAwait(false);
                if (Invites.Count != 0)
                {
                    await ReplyAsync(Invites.First().Code).ConfigureAwait(false);
                    return;
                }
                IGuildChannel Chan = await Guild.GetDefaultChannelAsync().ConfigureAwait(false);
                var Invite = await Chan.CreateInviteAsync().ConfigureAwait(false) ?? null;
                if (Invite != null)
                {
                    await ReplyAsync(Invite.Code).ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync($"`Could not create invite for guild {ID}`").ConfigureAwait(false);
                    return;
                }
            }

            [Command("info")]
            [RequireOwner]
            public async Task Oinfo(ulong ID)
            {
                    var Guild = await Context.Client.GetGuildAsync(ID).ConfigureAwait(false) ?? null;
                    if (Guild == null)
                    {
                        await ReplyAsync($"`Cannot find guild {ID}`").ConfigureAwait(false);
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
                            IconUrl = new Uri(Guild.IconUrl)
                        },
                        Description = $"Owner: {Owner}" + Environment.NewLine + $"Users {Users.Where(x => !x.IsBot).Count()}/{Users.Where(x => x.IsBot).Count()} Bots",
                        Color = Utils.DiscordUtils.GetRoleColor(Context.Channel as ITextChannel),
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = $"Created {Guild.CreatedAt.Day}/{Guild.CreatedAt.Month}/{Guild.CreatedAt.Year}"
                        }
                    };
                    await ReplyAsync("", false, embed).ConfigureAwait(false);
            }

            [Command("leavehere")]
            [RequireOwner]
            public async Task Leavehere()
            {
                await Context.Guild.LeaveAsync().ConfigureAwait(false);
            }

            [Command("clear")]
            [RequireOwner]
            public async Task Clear()
            {
                await Context.Message.DeleteAsync().ConfigureAwait(false);
                Console.Clear();
                Console.WriteLine("Console cleared");
                await Context.Channel.SendMessageAsync("`Console cleared`").ConfigureAwait(false);
            }

            [Command("botcol")]
            [RequireOwner]
            public async Task Botcol(int Number = 100)
            {
                var Guilds = await Context.Client.GetGuildsAsync().ConfigureAwait(false);
                List<string> GuildList = new List<string>();
                foreach (var Guild in Guilds)
                {
                    if (Guild.Id != 110373943822540800 & Guild.Id != 264445053596991498)
                    {
                        var Users = await Guild.GetUsersAsync();
                        IGuildUser Owner = await Guild.GetOwnerAsync() ?? null;
                        if (Owner != null)
                        {
                            
                            if (Users.Where(x => x.IsBot).Count() * 100 / Users.Count() > 85)
                            {
                                GuildList.Add($"{Guild.Name} ({Guild.Id}) - Owner: {Owner.Username} ({Owner.Id}) - {Users.Where(x => !x.IsBot).Count()}/{Users.Where(x => x.IsBot).Count()}");
                            }
                        }
                        else
                        {
                            GuildList.Add($"{Guild.Name} ({Guild.Id}) - NO OWNER! - {Users.Where(x => !x.IsBot).Count()}/{Users.Where(x => x.IsBot).Count()}");
                        }
                    }

                }
                string AllGuilds = string.Join(Environment.NewLine, GuildList.ToArray());
                IDMChannel DM = await Context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                    await DM.SendMessageAsync("```" + Environment.NewLine + AllGuilds + "```");
            }

            [Command("leave")]
            [RequireOwner]
            public async Task Leave(ulong ID)
            {
                await Context.Message.DeleteAsync();
                IGuild Guild = await Context.Client.GetGuildAsync(ID).ConfigureAwait(false) ?? null;
                if (Guild == null)
                {
                    await ReplyAsync($"`Could not find guild by id {ID}`").ConfigureAwait(false);
                }
                else
                {
                    try
                    {
                        IGuildUser Owner = await Guild.GetOwnerAsync().ConfigureAwait(false);
                        await Guild.LeaveAsync().ConfigureAwait(false);
                        await ReplyAsync($"`Left guild {Guild.Name} - {Guild.Id} | Owned by {Owner.Username}#{Owner.Discriminator}`").ConfigureAwait(false);
                    }
                    catch
                    {
                        await Guild.LeaveAsync().ConfigureAwait(false);
                        await ReplyAsync($"`Left guild {Guild.Name} - {Guild.Id}`").ConfigureAwait(false);
                    }
                }
            }

            [Group("blacklist")]
            public class BlacklistGroup : ModuleBase
            {
                [Command]
                [RequireOwner]
                public async Task Blacklist()
                {
                    await ReplyAsync("`Blacklist > add | reload | remove | list | info`").ConfigureAwait(false);
                }

                [Command("add")]
                [RequireOwner]
                public async Task BlacklistAdd(ulong ID, [Remainder] string Reason = "")
                {
                    if (Bot.Blacklist.Exists(x => x.GuildID == ID))
                    {
                        await Context.Channel.SendMessageAsync($"`{ID} is already in the blacklist`").ConfigureAwait(false);
                    }
                    else
                    {
                        try
                        {
                            IGuild Guild = await Context.Client.GetGuildAsync(ID).ConfigureAwait(false);
                            var Users = await Guild.GetUsersAsync().ConfigureAwait(false);
                            Bot.BlacklistAdd(Guild.Name, ID, Reason, $"{Users.Where(x => !x.IsBot).Count()}/{Users.Where(x => x.IsBot).Count()}");
                            await ReplyAsync($"`Adding {Guild.Name} - {ID} to blacklist`").ConfigureAwait(false);
                            await Guild.LeaveAsync().ConfigureAwait(false);
                        }
                        catch
                        {
                            Bot.BlacklistAdd("", ID, Reason);
                            await ReplyAsync($"`Adding {ID} to blacklist`").ConfigureAwait(false);
                        }
                        
                    }
                }

                [Command("reload")]
                [RequireOwner]
                public async Task BlacklistReload()
                {
                    List<Bot.BlacklistClass> BlacklistCache = new List<Bot.BlacklistClass>();
                    foreach (var File in Directory.GetFiles(Config.PathBlacklist))
                    {
                        using (StreamReader reader = new StreamReader(File))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            BlacklistCache.Add((Bot.BlacklistClass)serializer.Deserialize(reader, typeof(Bot.BlacklistClass)));
                        }

                    }
                    Bot.Blacklist.Clear();
                    Bot.Blacklist = BlacklistCache;
                    await ReplyAsync("`Blacklist reloaded`").ConfigureAwait(false);
                }

                [Command("remove")]
                [RequireOwner]
                public async Task BlacklistRemove(ulong ID)
                {
                    Bot.BlacklistClass Item = Bot.Blacklist.Find(x => x.GuildID == ID);
                    if (Item == null)
                    {
                        await ReplyAsync("`Could not find guild ID`").ConfigureAwait(false);
                    }
                    else
                    {
                        Bot.Blacklist.Remove(Item);
                        if (File.Exists(Config.PathBlacklist + $"{Item.GuildID.ToString()}.json"))
                        {
                            File.Delete(Config.PathBlacklist + $"{Item.GuildID.ToString()}.json");
                        }
                        await ReplyAsync($"`Remove {Item.GuildName} - {Item.GuildID} from blacklist`").ConfigureAwait(false);
                    }
                }

                [Command("list")]
                [RequireOwner]
                public async Task BlacklistList()
                {
                    if (Bot.Blacklist.Count == 0)
                    {
                        await ReplyAsync("`Blacklist is empty`").ConfigureAwait(false);
                    }
                    else
                    {
                        List<string> BlacklistItems = new List<string>();
                        foreach (var i in Bot.Blacklist)
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
                    Bot.BlacklistClass Item = Bot.Blacklist.Find(x => x.GuildID == ID);
                    if (Item == null)
                    {
                        await ReplyAsync("`Could not find guild ID`").ConfigureAwait(false);
                    }
                    else
                    {
                        await ReplyAsync($"{Item.GuildName} - {Item.GuildID} ({Item.UsersToBots})" + Environment.NewLine + Item.Reason).ConfigureAwait(false);
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
