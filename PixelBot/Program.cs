using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using MySQLDriverCS;
using System.IO;
using System.Net;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Newtonsoft.Json;
using SteamStoreQuery;
using PortableSteam;
using TwitchCSharp.Clients;
using System.Timers;
using Discord.Addons.InteractiveCommands;
using Discord.Addons.Preconditions;
using Discord.Addons.Paginator;
using OverwatchAPI;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

public class Token
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
}
public class Item
{
    public int millis;
    public string stamp;
    public DateTime datetime;
    public string light;
    public float temp;
    public float vcc;
}
class Program
{
    public static bool DevMode = true;
    public static string BotPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\PixelBot\\";
    public static bool FirstStart = false;
    public static DiscordSocketClient _client;
    public static CommandService _commands = new CommandService();
    public static ServiceCollection _map = new ServiceCollection();
    public static IServiceProvider _provider;
    public static PaginationService _pagination;
    public static Dictionary<ulong, IGuildUser> ThisBot = new Dictionary<ulong, IGuildUser>();
    public static Dictionary<ulong, int> UptimeBotsList = new Dictionary<ulong, int>();
    public static Timer _Timer_Twitch = new Timer();
    public static Timer _Timer_Stats = new Timer();
    public static Timer _Timer_Uptime = new Timer();
    public static Token TokenMap = new Token();
    static void Main()
    {
        DisableConsoleQuickEdit.Go();
        using (StreamWriter file = File.CreateText(BotPath + "TokensTemplate" + ".json"))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(file, TokenMap);
        }
        Console.Title = "PixelBot";
        string TokenPath = BotPath + "Tokens.txt";
        Directory.CreateDirectory(BotPath + "Uptime\\");
        foreach (var File in Directory.GetFiles(BotPath + "Uptime\\"))
        {
                using (StreamReader reader = new StreamReader(File))
                {
                UptimeBotsList.Add(Convert.ToUInt64(File.Replace(BotPath + "Uptime\\", "").Replace(".txt", "")), Convert.ToInt32(reader.ReadLine()));
                }
        }
        if (PixelBot.Properties.Settings.Default.Blacklist == null)
        {
            PixelBot.Properties.Settings.Default.Blacklist = new System.Collections.Specialized.StringCollection();
        }
        if (File.Exists(BotPath + "LIVE.txt"))
        {
            Program.DevMode = false;
        }

        //Token test = JsonConvert.DeserializeObject<Token>(File.ReadAllText(BotPath + "Tokens.json"));
        
        using (StreamReader file = File.OpenText(BotPath + "Tokens.json"))
        {
            JsonSerializer serializer = new JsonSerializer();
            Token Items = (Token)serializer.Deserialize(file, typeof(Token));
            TokenMap = Items;
        }
        new Program().RunBot().GetAwaiter().GetResult();
    }
    public async Task RunBot()
    {
        _client = new DiscordSocketClient();
        _pagination = new PaginationService(_client);
        var services = ConfigureServices();
        await InstallCommands(_provider);

        _client.Connected += async () =>
        {
            Console.Title = "PixelBot - Online!";
            Console.WriteLine("[PixelBot] Connected");
            if (DevMode == false & FirstStart == false)
            {
                await _client.SetStatusAsync(UserStatus.Idle);
                await _client.SetGameAsync("Loading");
            }
        };

        _client.UserJoined += (User) =>
        {
            if (DevMode = false & User.Guild.Id == 81384788765712384 || User.Guild.Id == 264445053596991498 & User.IsBot & !File.Exists(Program.BotPath + "Uptime\\" + User.Id.ToString() + ".txt"))
            {
                  System.IO.File.WriteAllText(BotPath + "Uptime\\" + User.Id.ToString() + ".txt", "75");
            }
            return Task.CompletedTask;
        };

        _client.Disconnected += (r) =>
        {
            Console.Title = "PixelBot - Offline!";
            Console.WriteLine("[PixelBot] Disconnected");
            return Task.CompletedTask;
        };

        _client.Ready += async () =>
        {
            Console.WriteLine($"[PixelBot] Online in {_client.Guilds.Count} Guilds");
            await _client.SetStatusAsync(UserStatus.Online);
            await _client.SetGameAsync($"p/help | {_client.Guilds.Count} Guilds | https://blaze.ml");
            if (DevMode == false & FirstStart == false)
            {
                    UpdateUsers();
                    UpdateBotService(null, null);
                    _Timer_Twitch.Interval = 60000;
                    _Timer_Twitch.Elapsed += TwitchNotificationService;
                    _Timer_Twitch.Start();
                    _Timer_Stats.Interval = 300000;
                    _Timer_Stats.Elapsed += UpdateBotService;
                    _Timer_Stats.Start();
                    _Timer_Uptime.Interval = 600000;
                    _Timer_Uptime.Elapsed += BotUptimeService;
                    _Timer_Uptime.Start();
                    Console.WriteLine("[PixelBot] Timer Service Online");
            }
            FirstStart = true;
        };

        _client.LeftGuild += async (g) =>
        {
            ThisBot.Remove(g.Id);
            if (DevMode == false)
            {
                await _client.SetGameAsync($"p/help | {_client.Guilds.Count} Guilds | https://blaze.ml");
                
                Console.WriteLine($"Left Guild > {g.Name} - {g.Id}");
            }
        };

        _client.JoinedGuild += async (g) =>
        {
            if (DevMode == false)
            {
                if (PixelBot.Properties.Settings.Default.Blacklist.Contains(g.Id.ToString()))
                {
                    Console.WriteLine($"Removed {g.Name} - {g.Id} due to blacklist");
                    await g.DefaultChannel.SendMessageAsync($"This guild has been blacklist by the owner ({g.Id}) contact `xXBuilderBXx#9113` for more info");
                    await g.LeaveAsync();
                    return;
                }
                await _client.SetGameAsync($"p/help | {_client.Guilds.Count} Guilds | https://blaze.ml ");
            }
            IGuildUser BotUser = g.GetUser(_client.CurrentUser.Id);
            ThisBot.Add(g.Id, BotUser);
            Console.WriteLine($"Joined Guild {g.Name} - {g.Id}");
        };

        _client.GuildAvailable += async (g) =>
        {
            IGuildUser BotUser = g.GetUser(_client.CurrentUser.Id);
            ThisBot.Add(g.Id, BotUser);
            if (DevMode == false)
            {
                if (PixelBot.Properties.Settings.Default.Blacklist.Contains(g.Id.ToString()))
                {
                    Console.WriteLine($"Removed {g.Name} - {g.Id} due to blacklist");
                    await g.LeaveAsync();
                    return;
                }
            }
        };
        
        try
        {
            await _client.LoginAsync(TokenType.Bot, TokenMap.Discord);
            await _client.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        await Task.Delay(-1);
    }

    public static void UpdateBotService(object sender, ElapsedEventArgs e)
    {
        try
        {
            var request = (HttpWebRequest)WebRequest.Create("https://bots.discord.pw/api/bots/277933222015401985/stats");
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", Program.TokenMap.Dbots);
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = "{\"server_count\":\"" + _client.Guilds.Count.ToString() + "\"}";

                streamWriter.Write(json);
            }
            request.GetResponse();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error could not update Dbots Stats");
        }
        try
        {
            var request = (HttpWebRequest)WebRequest.Create("https://discordbots.org/api/bots/277933222015401985/stats");
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", Program.TokenMap.DbotsV2);
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = "{\"server_count\":\"" + _client.Guilds.Count.ToString() + "\"}";

                streamWriter.Write(json);
            }
            request.GetResponse();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error could not update DbotsV2 Stats");
        }
    }

    public IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection()
        .AddSingleton(_client)
            .AddSingleton(_pagination);
        return services.BuildServiceProvider();

    }
    public async Task InstallCommands(IServiceProvider provider)
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        _provider = provider;
        _client.MessageReceived += HandleCommand;
    }
    public async Task HandleCommand(SocketMessage messageParam)
    {
        var message = messageParam as SocketUserMessage;
        if (message == null)
        { return; }
        int argPos = 0;
        if (message.Author.IsBot)
        {
            return;
        }
        if (DevMode == true)
        {
            if (!(message.HasStringPrefix("tp/", ref argPos))) return;
            
            var context = new CommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, argPos, _provider);
            if (result.IsSuccess)
            {
                if (message.Channel is IPrivateChannel)
                {
                    Console.WriteLine($"[Test Command] > (DM) {message.Author.Username} executed {message.Content}");
                }
                else
                {
                    var GuildUser = message.Author as IGuildUser;
                    Console.WriteLine($"[Test Command] > ({GuildUser.Guild.Name}) {message.Author.Username} executed {message.Content}");
                }
            }
            else
            {
                Console.WriteLine(result.ErrorReason);
                if (message.Content.Contains("vg "))
                {
                    if (result.ErrorReason == "This input does not match any overload.")
                    {
                        //await context.Channel.SendMessageAsync("You can only use this command 2 times every half a minute");
                    }
                }
            }
        }
        else
        {
            if (!(message.HasStringPrefix("p/", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
            var context = new CommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, argPos, _provider);
            if (result.IsSuccess)
            {
                if (PixelBot.Properties.Settings.Default.CommandOutput == true)
                {
                    if (message.Channel is IPrivateChannel)
                    {
                        Console.WriteLine($"[Command] > (DM) {message.Author.Username} executed {message.Content}");
                    }
                    else
                    {
                        var GuildUser = message.Author as IGuildUser;
                        Console.WriteLine($"[Command] > ({GuildUser.Guild.Name}) {message.Author.Username} executed {message.Content}");
                    }
                }
            }
            else
            {
                if (message.Content.Contains("vg "))
                {
                    if (result.ErrorReason == "This input does not match any overload.")
                    {
                        //await context.Channel.SendMessageAsync("You can only use this command 2 times every half a minute");
                    }
                }
            }
        }
    }

    public void YTNOTIFY()
    {
        MySQLConnection myConn;
        MySQLDataReader MyReader = null;
        myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = Program.TokenMap.Youtube
        });
        myConn.Open();
        string stm = "SELECT source FROM notifysource";
        MySQLCommand cmd = new MySQLCommand(stm, myConn);
        List<string> SourceList = new List<string>();
        MyReader = cmd.ExecuteReaderEx();
        while (MyReader.Read())
        {
            SourceList.Add(MyReader.GetString(0));
        }
        var searchListRequest = youtubeService.Search.List("snippet");
        searchListRequest.MaxResults = 5;
        SourceList.ForEach(async delegate (string name)
        {

            searchListRequest.ChannelId = name;
            var searchListResponse = await searchListRequest.ExecuteAsync();
            var Live = false;
            foreach (var searchResult in searchListResponse.Items)
            {
                if (searchResult.Snippet.LiveBroadcastContent == "live")
                {
                    Live = true;
                }
                else
                {
                    Live = false;

                }
            }
            if (Live == true)
            {
                myConn.Open();
                string SS = $"SELECT stat FROM notifysource WHERE src='{name}'";
                MySQLCommand cmd2 = new MySQLCommand(SS, myConn);
                MyReader = cmd2.ExecuteReaderEx();
                while (MyReader.Read())
                {
                    Console.WriteLine(MyReader.GetString(0));
                    if (MyReader.GetString(0) == "false")
                    {

                        Console.WriteLine(name);
                    }
                }
                myConn.Close();

            }
            else
            {
                Console.WriteLine(name);
                Console.WriteLine("Not Online");
            }
        });
    }

    public static Color GetRoleColor(ICommandContext Command)
    {
        Color RoleColor = new Discord.Color(30,0,200);
            IGuildUser BotUser = null;
        if (Command.Guild != null)
        {
            ThisBot.TryGetValue(Command.Guild.Id, out BotUser);
            if (BotUser.GetPermissions(Command.Channel as ITextChannel).EmbedLinks)
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

    public void TwitchNotificationService(object sender, ElapsedEventArgs e)
    {
        var TwitchClient = new TwitchAuthenticatedClient(Program.TokenMap.Twitch, Program.TokenMap.TwitchAuth);
        MySQLConnection DB;
        MySQLDataReader MyReader = null;
        DB = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
        DB.Open();
        string RS = $"SELECT type, userid, guild, channel, twitch, live FROM twitch";
        MySQLCommand cmd = new MySQLCommand(RS, DB);
        MyReader = cmd.ExecuteReaderEx();
        while (MyReader.Read())
        {
            if (TwitchClient.IsLive(MyReader.GetString(4)) == true)
            {
                if (MyReader.GetString(0) == "channel")
                {
                    if (MyReader.GetString(5) == "no")
                    {
                        string update = $"UPDATE twitch SET live='yes' WHERE type='channel' AND guild='{MyReader.GetString(2)}' AND channel='{MyReader.GetString(3)}' AND twitch='{MyReader.GetString(4)}'";
                        MySQLCommand upcmd = new MySQLCommand(update, DB);
                        upcmd.ExecuteNonQuery();
                        try
                        {
                            IGuild Guild = _client.GetGuild(Convert.ToUInt64(MyReader.GetString(2)));
                            ITextChannel Channel = Guild.GetChannelAsync(Convert.ToUInt64(MyReader.GetString(3))).GetAwaiter().GetResult() as ITextChannel;
                            var TwitchChannel = TwitchClient.GetChannel(MyReader.GetString(4));
                            var embed = new EmbedBuilder()
                            {
                                Title = $"TWITCH - {TwitchChannel.DisplayName} is live playing {TwitchChannel.Game}",
                                Url = "https://www.twitch.tv/" + TwitchChannel.Name,
                                Description = TwitchChannel.Status,
                                Footer = new EmbedFooterBuilder()
                                {
                                    Text = $"To remove this notification do p/tw remove here {TwitchChannel.Name}"
                                },
                                ThumbnailUrl = TwitchChannel.Logo
                            };
                            Channel.SendMessageAsync("", false, embed).GetAwaiter();
                            
                        }
                        catch
                        {
                            Console.WriteLine($"Twitch Error Channel > G: {MyReader.GetString(2)} U: {MyReader.GetString(1)} T: {MyReader.GetString(4)}");
                        }

                    }
                }
                if (MyReader.GetString(0) == "user")
                {
                    if (MyReader.GetString(5) == "no")
                    {
                        string update = $"UPDATE twitch SET live='yes' WHERE type='user' AND userid='{MyReader.GetString(1)}' AND twitch='{MyReader.GetString(4)}'";
                        MySQLCommand upcmd = new MySQLCommand(update, DB);
                        upcmd.ExecuteNonQuery();
                        try
                        {
                            IGuild Guild = Program._client.GetGuild(Convert.ToUInt64(MyReader.GetString(2)));
                            IUser User = Guild.GetUserAsync(Convert.ToUInt64(MyReader.GetString(1))) as IUser;
                            var TwitchChannel = TwitchClient.GetChannel(MyReader.GetString(4));
                            var embed = new EmbedBuilder()
                            {
                                Title = $"TWITCH - {TwitchChannel.DisplayName} is live playing {TwitchChannel.Game}",
                                Url = "https://www.twitch.tv/" + TwitchChannel.Name,
                                Description = TwitchChannel.Status,
                                Footer = new EmbedFooterBuilder()
                                {
                                    Text = $"To remove this notification do p/tw remove me {TwitchChannel.Name} IN A GUILD!"
                                },
                                ThumbnailUrl = TwitchChannel.Logo
                            };
                            var DM = User.CreateDMChannelAsync().GetAwaiter().GetResult();
                            DM.SendMessageAsync("", false, embed).GetAwaiter();
                            
                        }
                        catch
                        {
                            Console.WriteLine($"Twitch Error User > G: {MyReader.GetString(2)} U: {MyReader.GetString(1)} T: {MyReader.GetString(4)}");
                        }
                    }
                }

            }
            else
            {
                if (MyReader.GetString(0) == "channel")
                {
                    string update = $"UPDATE twitch SET live='no' WHERE type='channel' AND guild='{MyReader.GetString(2)}' AND channel='{MyReader.GetString(3)}' AND twitch='{MyReader.GetString(4)}'";
                    MySQLCommand upcmd = new MySQLCommand(update, DB);
                    upcmd.ExecuteNonQuery();
                }
                if (MyReader.GetString(0) == "user")
                {
                    string update = $"UPDATE twitch SET live='no' WHERE type='user' AND  userid='{MyReader.GetString(1)}' AND twitch='{MyReader.GetString(4)}'";
                    MySQLCommand upcmd = new MySQLCommand(update, DB);
                    upcmd.ExecuteNonQuery();
                }
            }
        }
        DB.Close();
    }

    public static async void UpdateUsers()
    {
        try
        {
            var Dbots = _client.GetGuild(110373943822540800);
            await Dbots.DownloadUsersAsync();
            var DbotsV2 = _client.GetGuild(264445053596991498);
            await DbotsV2.DownloadUsersAsync();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static async void BotUptimeService(object sender, ElapsedEventArgs e)
    {
        try
        {
            List<IGuildUser> BotsList = new List<IGuildUser>();
            var Dbots = _client.GetGuild(110373943822540800);
            var DbotsV2 = _client.GetGuild(264445053596991498);
            await Dbots.DownloadUsersAsync();
            await DbotsV2.DownloadUsersAsync();

            BotsList.AddRange(Dbots.Users.Where(x => x.IsBot));
            BotsList.AddRange(DbotsV2.Users.Where(x => x.IsBot));

            foreach (var Bot in BotsList)
            {
                if (!File.Exists(BotPath + "Uptime\\" + Bot.Id.ToString() + ".txt"))
                {
                    File.WriteAllText(BotPath + "Uptime\\" + Bot.Id.ToString() + ".txt", "75");
                }
                int UptimeCount = 100;
                if (!UptimeBotsList.Keys.Contains(Bot.Id))
                {
                    UptimeBotsList.Add(Bot.Id, 75);
                }
                UptimeBotsList.TryGetValue(Bot.Id, out UptimeCount);
                if (Bot.Status == UserStatus.Offline)
                {
                    if (UptimeCount != 0)
                    {
                        UptimeCount--;
                    }
                }
                else
                {
                    if (UptimeCount != 100)
                    {
                        UptimeCount++;
                    }
                }
                File.WriteAllText(BotPath + "Uptime\\" + Bot.Id.ToString() + ".txt", UptimeCount.ToString());
                UptimeBotsList[Bot.Id] = UptimeCount;
                BotsList.RemoveAll(x => x.Id == Bot.Id);
            }
        }
        catch
        {
            Console.WriteLine("[Error] Error in bot uptime service");
        }
    }
}

public class Main : ModuleBase
{
    [Command("uptime")]
    public async Task Resetuptime(string User = "")
    {
        
        IGuildUser GuildUser = null;
        if (User == "")
        {
            await Context.Channel.SendMessageAsync("`You need to mention a bot or insert the ID`");
        }
        else
        {
            if (User.StartsWith("<@"))
            {
                string RealUser = User;
                RealUser = RealUser.Replace("<@", "").Replace(">", "");
                if (RealUser.Contains("!"))
                {
                    RealUser = RealUser.Replace("!", "");
                }
                GuildUser = await Context.Guild.GetUserAsync(Convert.ToUInt64(RealUser));
            }
            else
            {
                GuildUser = await Context.Guild.GetUserAsync(Convert.ToUInt64(User));
            }
            if (!GuildUser.IsBot)
            {
                await Context.Channel.SendMessageAsync("`This is not a bot`");
            }
            else
            {
                if (Program.UptimeBotsList.Keys.Contains(GuildUser.Id))
                {
                    int Uptime = 100;
                    Program.UptimeBotsList.TryGetValue(GuildUser.Id, out Uptime);
                    await Context.Channel.SendMessageAsync($"`{GuildUser.Username} has an uptime of {Uptime}%`");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("`This bot does not have any stats only bots in | Discord Bots | Discord Bot List | are supported for now`");
                }
            }
        }
    }

    [Command("test")]
    public async Task Test(string User = "Builderb")
    {
        
        //SteamIdentity SteamUser = null;
        //SteamWebAPI.SetGlobalKey(Program.SteamKey);
        //SteamUser = SteamWebAPI.General().ISteamUser().ResolveVanityURL(User).GetResponse().Data.Identity;
        //var RC = new RiotApi.Net.RestClient.RiotClient(Program.TokenMap.Riot);
    }

    [Command("yt")]
    public async Task Yt()
    {
        await Context.Channel.SendMessageAsync("`Coming Soon`");
    }

    [Command("yt user")]
    public async Task Ytuser([Remainder] string User = "null")
    {
        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = Program.TokenMap.Youtube
        });
        var searchListRequest = youtubeService.Search.List("snippet");
        searchListRequest.MaxResults = 1;
        searchListRequest.ChannelId = User;
        var searchListResponse = await searchListRequest.ExecuteAsync();
        foreach (var searchResult in searchListResponse.Items)
        {
            await Context.Channel.SendMessageAsync($"{searchResult.Snippet.ChannelTitle} - {searchResult.Snippet.ChannelId} - {searchResult.Kind}");
        }
    }

    [Command("yt notify")]
    public async Task Ytnotify()
    {
        await Context.Channel.SendMessageAsync("```fix" + Environment.NewLine + "Feature under construction```");
        return;
        await Context.Channel.SendMessageAsync("**/yt notify add (Channel ID)** - Add a  youtube streamer to your notification settings" + Environment.NewLine + "**/yt notify list** - List all of your youtube notification settings" + Environment.NewLine + "**/yt notify remove (Channel ID)** - Remove a youtube streamer from your notification settings");
    }

    [Command("yt notify add")]
    public async Task Ytadd([Remainder] string User = "null")
    {
        await Context.Channel.SendMessageAsync("```fix" + Environment.NewLine + "Feature under construction```");

        MySQLConnection myConn;
        MySQLDataReader MyReader = null;
        myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
        myConn.Open();
        string stm = $"SELECT channel FROM ytnotify WHERE id='{Context.User.Id}' AND channel='{User}'";
        MySQLCommand cmd = new MySQLCommand(stm, myConn);
        MyReader = cmd.ExecuteReaderEx();
        if (MyReader.HasRows)
        {
            await Context.Channel.SendMessageAsync("```fix" + Environment.NewLine + "This user already exists in your settings | If you would like to remove it do" + Environment.NewLine + $"/yt remove {User}");
        }
        else
        {
            string Command = $"INSERT INTO ytnotify (channel, id) VALUES (''.'')";
            MySQLCommand cmd2 = new MySQLCommand(Command, myConn);
            cmd2.ExecuteNonQuery();
        }
        MyReader.Close();
        myConn.Close();
    }

    [Command("yt notify list")]
    public async Task Ytlist()
    {
        List<string> YTList = new List<string>();
        await Context.Channel.SendMessageAsync("```fix" + Environment.NewLine + "Feature under construction```");

        MySQLConnection myConn;
        MySQLDataReader MyReader = null;
        myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
        myConn.Open();
        string stm = $"SELECT channel FROM ytnotify WHERE id='{Context.User.Id}'";
        MySQLCommand cmd = new MySQLCommand(stm, myConn);
        MyReader = cmd.ExecuteReaderEx();
        while (MyReader.Read())
        {
            YTList.Add(MyReader.GetString(0));
        }
        MyReader.Close();
        myConn.Close();
        string line = string.Join(Environment.NewLine, YTList.ToArray());
        await Context.Channel.SendMessageAsync(line);
    }

    [Command("yt notify remove")]
    public async Task Ytdel([Remainder] string User = "null")
    {
        await Context.Channel.SendMessageAsync("```fix" + Environment.NewLine + "Feature under construction```");

        MySQLConnection myConn;
        MySQLDataReader MyReader = null;
        myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
        myConn.Open();
        string stm = $"SELECT user FROM ytnotify WHERE id='{Context.User.Id}' AND channel='{User}'";
        MySQLCommand cmd = new MySQLCommand(stm, myConn);
        MyReader = cmd.ExecuteReaderEx();
        if (MyReader.HasRows)
        {

        }
        else
        {
            await Context.Channel.SendMessageAsync("```fix" + Environment.NewLine + "This user does not exists | If you would like to add it do" + Environment.NewLine + $"/yt add {User}");

        }
        myConn.Close();
    }

    [Command("profile")]
    public async Task Profile([Remainder] string User = null)
    {
        IGuildUser GuildUser = null;
        if (User == null)
        {
            GuildUser = Context.User as IGuildUser;
        }
        else
        {
            if (User.StartsWith("<@"))
            {
                string RealUser = User;
                RealUser = RealUser.Replace("<@", "").Replace(">", "");
                RealUser = RealUser.Replace("!", "");
                GuildUser = await Context.Guild.GetUserAsync(Convert.ToUInt64(RealUser));
            }
            bool DiscrimSearch = false;
            if (User.Length == 4)
            {
                DiscrimSearch = true;
            }
            else
            {
                var Members = await Context.Guild.GetUsersAsync();
                bool HasMember = false;
                foreach (var Member in Members)
                {
                    if (HasMember == false)
                    {
                        if (!Member.IsBot)
                        {
                            if (DiscrimSearch == false)
                            {
                                if (Member.Username == User)
                                {
                                    GuildUser = Member;
                                    HasMember = true;
                                }
                            }
                            else
                            {
                                if (Member.Discriminator == User)
                                {
                                    GuildUser = Member;
                                    HasMember = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        if (GuildUser == null)
        {
            await Context.Channel.SendMessageAsync("`Could not find user`");
            return;
        }
        if (GuildUser.IsBot)
        {
            await Context.Channel.SendMessageAsync("`Cannot use a bot account`");
            return;
        }
        Console.WriteLine(GuildUser.Username);
        return;
        MySQLConnection myConn;
        MySQLDataReader MyReader = null;
        myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
        myConn.Open();
        string RS = $"SELECT name WHERE id='{User}'";
        MySQLCommand cmd = new MySQLCommand(RS, myConn);
        MyReader = cmd.ExecuteReaderEx();
        myConn.Close();
        List<string> UList = new List<string>();
        if (!MyReader.HasRows)
        {
            char[] arr = User.ToCharArray();
            arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))));
            string str = new string(arr);
            Console.WriteLine(str);
            myConn.Open();
            string SS = $"INSERT INTO profiles(name, discrim, id) VALUES('{str}', '{User}', '{User}')";
            MySQLCommand cmd2 = new MySQLCommand(SS, myConn);
            cmd2.ExecuteNonQuery();
            myConn.Close();
            await Context.Channel.SendMessageAsync("Your profile has been created");
            return;
        }
        while (MyReader.Read())
        {
            UList.Add(MyReader.GetString(0));
        }
        if (UList.Count != 1)
        {

        }
    }

    
}
public class Misc : ModuleBase
{
    [Command("math")]
    [Remarks("math (1 + 1 * 9 / 5 - 3)")]
    [Summary("Do some maths calculations")]
    [Alias("calc")]
    public async Task Math([Remainder] string Math)
    {
        var interpreter = new DynamicExpresso.Interpreter();
        var result = interpreter.Eval(Math);
        await Context.Channel.SendMessageAsync($"`{Math} = {result.ToString()}`");
    }

    [Command("discrim")]
    [Remarks("discrim (0000)")]
    [Summary("list of guild and global user with a discrim")]
    public async Task Discrim(int Discrim = 0, string Option = "")
    {
        if (Discrim == 0)
        {
            await Context.Channel.SendMessageAsync($"`You can search for a discrim using p/discrim {Context.User.Discriminator} guild/global | The guild or global is optional`");
            return;
        }
        List<IGuildUser> GuildUsers = new List<IGuildUser>();
        var Guilds = await Context.Client.GetGuildsAsync();
        foreach (var Guild in Guilds)
        {
            var Users = await Guild.GetUsersAsync();
            foreach (var User in Users)
            {
                GuildUsers.Add(User);
            }
        }

        List<string> UserList = new List<string>();
        if (Context.Channel is IPrivateChannel)
        {
            foreach (var User in GuildUsers.Where(x => x.DiscriminatorValue == Discrim))
            {
                if (!UserList.Contains($"{User.Username}#{User.Discriminator}") && User.Id != Context.User.Id)
                {
                    UserList.Add($"{User.Username}#{User.Discriminator}");
                }
            }
        }
        else
        {
            if (Option.ToLower() != "global")
            {
                foreach (var GuildUser in GuildUsers.Where(x => x.GuildId == Context.Guild.Id))
                {
                    if (!UserList.Contains($"{GuildUser.Username}#{GuildUser.Discriminator} (Guild)") && GuildUser.DiscriminatorValue == Discrim && GuildUser.Id != Context.User.Id)
                    {
                        UserList.Add($"{GuildUser.Username}#{GuildUser.Discriminator} (Guild)");
                    }
                }
            }
            if (Option.ToLower() != "guild")
            {
                foreach (var GuildUser in GuildUsers.Where(x => x.GuildId != Context.Guild.Id))
                {
                    if (!UserList.Contains($"{GuildUser.Username}#{GuildUser.Discriminator} (Global)") & !UserList.Contains($"{GuildUser.Username}#{GuildUser.Discriminator} (Guild)") && GuildUser.DiscriminatorValue == Discrim && GuildUser.Id != Context.User.Id)
                    {
                        UserList.Add($"{GuildUser.Username}#{GuildUser.Discriminator} (Global)");
                    }
                }
            }
        }

        if (UserList.Count == 0)
        {
            await Context.Channel.SendMessageAsync($"`Could not find any users with the discrim {Discrim}`");
        }
        else
        {
            string Users = string.Join(Environment.NewLine, UserList.ToArray());
            await Context.Channel.SendMessageAsync($"**Found {UserList.Count} users with the discrim {Discrim}**" + Environment.NewLine + "```" + Users + "```");
        }
    }

    [Command("guild")]
    [Remarks("guild")]
    [Summary("Info about the guild | Owner/Roles")]
    public async Task Guild(string arg = "guild")
    {
        if (arg.ToLower() == "role" || arg.ToLower() == "roles")
        {
            List<string> RoleList = new List<string>();
            foreach (var Role in Context.Guild.Roles)
            {
                RoleList.Add($"[ {Role.Name} ]( {Role.Id} )");
            }
            var line = string.Join(Environment.NewLine, RoleList.ToArray());

            var User = Context.User as IUser;
            var UserDM = await User.CreateDMChannelAsync();
            await Context.Channel.SendMessageAsync($"`{Context.User.Username} I sent you a list of guild roles`");
            await UserDM.SendMessageAsync($"Guild roles for {Context.Guild.Name}```md" + Environment.NewLine + line + "```");
            return;
        }
        int Members = 0;
        int Bots = 0;
        int MembersOnline = 0;
        int BotsOnline = 0;
        IGuildUser Owner = await Context.Guild.GetOwnerAsync();
        foreach (var User in await Context.Guild.GetUsersAsync())
        {
            if (User.IsBot)
            {
                if (User.Status == UserStatus.Online || User.Status == UserStatus.Idle || User.Status == UserStatus.AFK || User.Status == UserStatus.DoNotDisturb)
                {
                    BotsOnline++;
                }
                else
                {
                    Bots++;
                }
            }
            else
            {
                if (User.Status == UserStatus.Online || User.Status == UserStatus.Idle || User.Status == UserStatus.AFK || User.Status == UserStatus.DoNotDisturb)
                {
                    MembersOnline++;
                }
                else
                {
                    Members++;
                }
            }
        }
        int TextChan = 0;
        int VoiceChan = 0;
        foreach (var emoji in Context.Guild.Emotes)
        {
            Console.WriteLine(emoji.Name);
        }
        foreach (var Channel in await Context.Guild.GetChannelsAsync())
        {
            if (Channel is ITextChannel)
            {
                TextChan++;
            }
            else
            {
                VoiceChan++;
            }
        }
        var embed = new EmbedBuilder()
        {
            Author = new EmbedAuthorBuilder()
            {
                Name = $"{Context.Guild.Name}"
            },
            ThumbnailUrl = Context.Guild.IconUrl,
            Color = Program.GetRoleColor(Context),
            Description = $"Owner: {Owner.Mention}```md" + Environment.NewLine + $"[Online](Offline)" + Environment.NewLine + $"<Users> [{MembersOnline}]({Members}) <Bots> [{BotsOnline}]({Bots})" + Environment.NewLine + $"Channels <Text {TextChan}> <Voice {VoiceChan}>" + Environment.NewLine + $"<Roles {Context.Guild.Roles.Count}> <Region {Context.Guild.VoiceRegionId}>" + Environment.NewLine + "List of roles | p/guild roles```",
            Footer = new EmbedFooterBuilder()
            {
                Text = $"Created {Context.Guild.CreatedAt.Date.Day} {Context.Guild.CreatedAt.Date.DayOfWeek} {Context.Guild.CreatedAt.Year}"
            }
        };
        await Context.Channel.SendMessageAsync("", false, embed);
    }

    [Command("user")]
    [Remarks("user (@Mention/User ID)")]
    [Summary("Info about a user")]
    public async Task User(string User = "")
    {
        IGuildUser GuildUser = null;
        if (User == "")
        {
            GuildUser = Context.User as IGuildUser;
        }
        else
        {
            if (User.StartsWith("<@"))
            {
                string RealUser = User;
                RealUser = RealUser.Replace("<@", "").Replace(">", "");
                if (RealUser.Contains("!"))
                {
                    RealUser = RealUser.Replace("!", "");
                }
                GuildUser = await Context.Guild.GetUserAsync(Convert.ToUInt64(RealUser));
            }
            else
            {
                GuildUser = await Context.Guild.GetUserAsync(Convert.ToUInt64(User));
            }
        }
        if (GuildUser == null)
        {
            await Context.Channel.SendMessageAsync("`Could not find user`");
            return;
        }

        var embed = new EmbedBuilder()
        {
            Author = new EmbedAuthorBuilder()
            {
                Name = $"User Info (Click For Avatar Url)",
                Url = GuildUser.GetAvatarUrl()
            },
            ThumbnailUrl = GuildUser.GetAvatarUrl(),
            Color = Program.GetRoleColor(Context),
            Description = $"<@{GuildUser.Id}>" + Environment.NewLine + "```md" + Environment.NewLine + $"<Discrim {GuildUser.Discriminator}> <ID {GuildUser.Id}>" + Environment.NewLine + $"<Joined_Guild {GuildUser.JoinedAt.Value.Date.ToShortDateString()}>" + Environment.NewLine + $"<Created_Account {GuildUser.CreatedAt.Date.ToShortDateString()}>```",
            Footer = new EmbedFooterBuilder()
            { Text = "To lookup a discrim use | p/discrim 0000" }
        };
        await Context.Channel.SendMessageAsync("", false, embed);
    }

    [Command("bot")]
    [Remarks("bot")]
    [Summary("Info about this bot | Owner/Websites/Stats")]
    public async Task Info()
    {
        try
        {
            List<string> Feature = new List<string>();
            MySQLConnection myConn;
            MySQLDataReader MyReader = null;
            myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
            myConn.Open();
            string RS = $"SELECT text FROM features";
            MySQLCommand cmd = new MySQLCommand(RS, myConn);
            MyReader = cmd.ExecuteReaderEx();
            myConn.Close();
            while (MyReader.Read())
            {
                Feature.Add("- " + MyReader.GetString(0));
            }
            string line = string.Join(Environment.NewLine, Feature.ToArray());
            int GuildCount = 0;
            foreach (var Guild in await Context.Client.GetGuildsAsync())
            {
                GuildCount = GuildCount + 1;
            }
            var embed = new EmbedBuilder()
            {
                Title = "Website",
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Bot Invite p/invite | Want a custom command or feature? then please contact me"
                },
                Url = "https://blaze.ml",
                Description = "Created by xXBuilderBXx#9113 | Visit the website for a list of commands and info",
                Color = Program.GetRoleColor(Context)
            };
            embed.AddField(x =>
            {
                x.Name = "Info"; x.Value = "Language C#" + Environment.NewLine + "Library .net 1.0" + Environment.NewLine + $"Guilds {GuildCount}" + Environment.NewLine + "[My Guild](http://discord.gg/WJTYdNb)" + Environment.NewLine + "[Website](https://blaze.ml)" + Environment.NewLine + "[Invite Bot](https://discordapp.com/oauth2/authorize?&client_id=277933222015401985&scope=bot&permissions=0)" + Environment.NewLine + "[Github](https://github.com/ArchboxDev/PixelBot)"; x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "Coming Soon"; x.Value = "```fix" + Environment.NewLine + $"{line}```"; x.IsInline = true;
            });
            await Context.Channel.SendMessageAsync("", false, embed);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    [Command("roll")]
    [Alias("dice")]
    [Remarks("roll")]
    [Summary("Roll the dice!")]
    public async Task Roll()
    {
        var random = new Random((int)DateTime.Now.Ticks); var randomValue = random.Next(1, 7);
        await Context.Channel.SendMessageAsync($"{Context.User.Username} Rolled a {randomValue}");
    }

    [Command("invite")]
    [Remarks("invite")]
    [Summary("Invite this bot to your guild")]
    public async Task Invite()
    {
        if (Context.Channel is IPrivateChannel)
        {
            await Context.Channel.SendMessageAsync("**Invite this bot to your guild**" + Environment.NewLine + "https://discordapp.com/oauth2/authorize?&client_id=277933222015401985&scope=bot&permissions=0");
        }
        else
        {
            IGuildUser BotUser = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
            if (BotUser.GetPermissions(Context.Channel as ITextChannel).EmbedLinks)
            {
                var embed = new EmbedBuilder()
                {
                    Title = "",
                    Description = "[Invite this bot to your guild](https://discordapp.com/oauth2/authorize?&client_id=277933222015401985&scope=bot&permissions=0)",
                    Color = Program.GetRoleColor(Context)
                };
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                await Context.Channel.SendMessageAsync("**Invite this bot to your guild**" + Environment.NewLine + "https://discordapp.com/oauth2/authorize?&client_id=277933222015401985&scope=bot&permissions=0");
            }
        }
    }

    [Command("flip")]
    [Alias("coin")]
    [Remarks("flip")]
    [Summary("Flip a coin!")]
    public async Task Flip()
    {
        var random = new Random((int)DateTime.Now.Ticks); var randomValue = random.Next(1, 3);
        if (randomValue == 1)
        { await Context.Channel.SendMessageAsync($"{Context.User.Username} Flipped Heads"); }
        else
        { await Context.Channel.SendMessageAsync($"{Context.User.Username} Flipped Tails"); }
    }
}
public class Game : ModuleBase
{
    [Command("vainglory")]
    [Alias("vg")]
    [Remarks("vg")]
    [Summary("Vainglory game info and commands | Mobile MOBA")]
    public async Task VG()
    {
        var embed = new EmbedBuilder()
        {
            Title = "Vainglory",
            Url = "http://www.vainglorygame.com/",
            Description = "Vainglory is a MOBA similar to league of legends that is a 3 vs 3 match with other online players available for android and ios```md" + Environment.NewLine + "[ p/vg u (Region) (User) ]( Get user stats )" + Environment.NewLine + "[ p/vg match (Region) (ID) ]( Coming Soon Get a match by ID )" + Environment.NewLine + "[ p/vg matches (Region) (User) ]( Coming Soon Get the last 3 matches of a player )" + Environment.NewLine + "Full stats for all heroes and items coming soon```",
            Footer = new EmbedFooterBuilder()
            {
                Text = "Regions > na (North America) | eu (Europe) | sa (South Asia) | ea (East Asia) | sg (Sea)"
            }
        };
        await Context.Channel.SendMessageAsync("", false, embed);
    }

    [Group("vainglory")]
    [Alias("vg")]
    public class Vainglory : ModuleBase
    {
        [Command("vg user"), Ratelimit(2, 0.30, Measure.Minutes)]
        [Alias("vg u")]
        public async Task Vg(string Region = null, string VGUser = null)
        {
            if (Region == null || VGUser == null)
            {
                await Context.Channel.SendMessageAsync("`No region or user > p/vg (Region) (User) | na | eu | sa | ea | sg`");
                return;
            }
            if (Region == "na" || Region == "eu" || Region == "sa" || Region == "ea" || Region == "sg")
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.dc01.gamelockerapp.com/shards/" + Region + "/players?filter[playerName]=" + VGUser);
                httpWebRequest.Method = WebRequestMethods.Http.Get;
                httpWebRequest.Headers.Add("Authorization", Program.TokenMap.Vainglory);
                httpWebRequest.Headers.Add("X-TITLE-ID", "semc-vainglory");
                httpWebRequest.Accept = "application/json";
                try
                {
                    HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    var Req = readStream.ReadToEnd();
                    dynamic JA = Newtonsoft.Json.Linq.JObject.Parse(Req);
                    dynamic JO = Newtonsoft.Json.Linq.JArray.Parse(JA.data);
                    dynamic stuff = Newtonsoft.Json.Linq.JObject.Parse(JO);
                    Console.WriteLine(stuff);
                    //dynamic stuff = Newtonsoft.Json.Linq.JObject.Parse(JO.ToString());
                    //var embed = new EmbedBuilder()
                    //{
                    //Title = $"Vainglory | {VGUser}",
                    //Description = "```md" + Environment.NewLine + $"<Level {stuff.attributes.stats.level}> <XP {stuff.attributes.stats.xp}> <LifetimeGold {stuff.attributes.stats.lifetimeGold}>" + Environment.NewLine + $"<Wins {stuff.attributes.stats.wins}> <Played {stuff.attributes.stats.played}> <PlayedRank {stuff.attributes.stats.played_ranked}>```"
                    //};
                    //await Context.Channel.SendMessageAsync("", false, embed);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("`Request error or unknown user`");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("`Unknown region p/vg (Region) (User) | na | eu | sa | ea | sg`");
            }
        }

        [Command("vg match"), Ratelimit(2, 1, Measure.Minutes)]
        [Alias("vg m")]
        public async Task Vg2(string Region, [Remainder] string VGUser)
        {
            if (Region == "na" || Region == "eu" || Region == "sa" || Region == "ea" || Region == "sg")
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.dc01.gamelockerapp.com/shards/" + Region + $"/matches?filter[createdAt-start]={Context.Message.Timestamp.Year}-{Context.Message.Timestamp.Month.ToString().PadLeft(2, '0')}-{Context.Message.Timestamp.Day.ToString().PadLeft(2, '0')}T13:25:30Z&filter[playerNames]=" + VGUser);
                httpWebRequest.Method = WebRequestMethods.Http.Get;
                httpWebRequest.Headers.Add("Authorization", Program.TokenMap.Vainglory);
                httpWebRequest.Headers.Add("X-TITLE-ID", "semc-vainglory");
                httpWebRequest.Accept = "application/json";
                try
                {
                    HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    var Req = readStream.ReadToEnd();
                    Console.WriteLine(Req);
                    return;
                    dynamic stuff = Newtonsoft.Json.Linq.JObject.Parse(Req);

                    var embed = new EmbedBuilder()
                    {
                        Title = $"Vainglory | {VGUser}",
                        Description = "```md" + Environment.NewLine + $"<Level {stuff.data.attributes.stats.level}> <XP {stuff.data.attributes.stats.xp}> <LifetimeGold {stuff.data.attributes.stats.lifetimeGold}>" + Environment.NewLine + $"<Wins {stuff.data.attributes.stats.wins}> <Played {stuff.data.attributes.stats.played}> <PlayedRank {stuff.data.attributes.stats.played_ranked}>```",
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = $"Created {stuff.data.attributes.createdAt}"
                        }
                    };
                    await Context.Channel.SendMessageAsync("", false, embed);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("`Request error or unknown user`");
                }

            }
            else
            {
                await Context.Channel.SendMessageAsync("`Unknown region p/vg user (Region) (User) | na | eu | sa | ea | sg`");
            }
        }
    }

    [Command("ow")]
    [Remarks("ow (User#Tag)")]
    [Summary("Overwatch game user stats")]
    public async Task Ow(string User = "")
    {
        if (!User.Contains("#") | OverwatchAPIHelpers.IsValidBattletag(User) == false)
        {
            await Context.Channel.SendMessageAsync("`Overwatch user/tag not found | Example SirDoombox#2603`");
            return;
        }
        OverwatchPlayer player = new OverwatchPlayer(User);
        await player.DetectPlatform();
        await player.DetectRegionPC();
        await player.UpdateStats();
        var CasualStats = player.CasualStats.GetHero("AllHeroes");
        var RankedStats = player.CompetitiveStats.GetHero("AllHeroes");
        int Achievements = 0;
        foreach (var A in player.Achievements)
        {
            foreach (var B in A)
            {
                if (B.IsUnlocked)
                {
                    Achievements++;
                }
            }
        }
        var embed = new EmbedBuilder()
        {
            Author = new EmbedAuthorBuilder()
            {
                Name = $"{User} | {player.Region} | (Level: {player.PlayerLevel}) | (Rank: {player.CompetitiveRank})",
                IconUrl = "https://cdn2.iconfinder.com/data/icons/overwatch-players-icons/512/Overwatch-512.png",
                Url = player.ProfileURL
            },
            ThumbnailUrl = player.ProfileURL,
            Color = new Color(250, 160, 46),
            Timestamp = player.ProfileLastDownloaded.Date,
            Description = "```md" + Environment.NewLine + $"<Achievements {Achievements}>" + Environment.NewLine + $"<Casual Games won {CasualStats.GetCategory("Game").GetStat("Games Won").Value} | Time {CasualStats.GetCategory("Game").GetStat("Time Played").Value} Seconds>" + Environment.NewLine + $"<Ranked Games played {RankedStats.GetCategory("Game").GetStat("Games Played").Value} | Time {RankedStats.GetCategory("Game").GetStat("Time Played").Value} Seconds>```More stats coming soon"
        };
        await Context.Channel.SendMessageAsync("", false, embed);
    }

    [Command("xbox")]
    [Remarks("xbox (User)")]
    [Summary("Xbox live user stats")]
    public async Task Xboxuser(string User)
    {
        var PWM = await Context.Channel.SendMessageAsync("`Please wait`");
        HttpWebRequest LiveStatus = (HttpWebRequest)WebRequest.Create("http://support.xbox.com/en-US/LiveStatus/GetHeaderStatusModule");
        LiveStatus.Method = WebRequestMethods.Http.Get;
        HttpWebResponse LiveRes = (HttpWebResponse)LiveStatus.GetResponse();
        Stream LiveStream = LiveRes.GetResponseStream();
        StreamReader LiveRead = new StreamReader(LiveStream, Encoding.UTF8);
        var LiveR = LiveRead.ReadToEnd();
        string Status = "Xbox Live Is Online";
        if (LiveR.Contains("Up and Running"))
        {

        }
        else
        {
            Status = "Xbox Live Is Having Issues!";
        }
        try
        {
            HttpWebRequest GetUserId = (HttpWebRequest)WebRequest.Create("https://xboxapi.com/v2/xuid/" + User);
            GetUserId.Method = WebRequestMethods.Http.Get;
            GetUserId.Headers.Add("X-AUTH", Program.TokenMap.Xbox);
            GetUserId.Accept = "application/json";
            HttpWebResponse response = (HttpWebResponse)GetUserId.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            var Req = readStream.ReadToEnd();
            var UserID = "";
            foreach (var Number in Req.Where(char.IsNumber))
            {
                UserID = UserID + Number;
            }
            string UserOnline = "Offline";
            HttpWebRequest OnlineHttp = (HttpWebRequest)WebRequest.Create("https://xboxapi.com/v2/" + UserID + "/presence");
            OnlineHttp.Method = WebRequestMethods.Http.Get;
            OnlineHttp.Headers.Add("X-AUTH", Program.TokenMap.Xbox);
            OnlineHttp.Accept = "application/json";
            HttpWebResponse OnlineRes = (HttpWebResponse)OnlineHttp.GetResponse();
            Stream OnlineStream = OnlineRes.GetResponseStream();
            StreamReader OnlineRead = new StreamReader(OnlineStream, Encoding.UTF8);
            var OnlineJson = OnlineRead.ReadToEnd();
            if (OnlineJson.Contains("Online"))
            {
                UserOnline = "Online";
            }
            HttpWebRequest HttpUserGamercard = (HttpWebRequest)WebRequest.Create("https://xboxapi.com/v2/" + UserID + "/gamercard");
            HttpUserGamercard.Method = WebRequestMethods.Http.Get;
            HttpUserGamercard.Headers.Add("X-AUTH", Program.TokenMap.Xbox);
            HttpUserGamercard.Accept = "application/json";
            HttpWebResponse GamercardRes = (HttpWebResponse)HttpUserGamercard.GetResponse();
            Stream GamercardStream = GamercardRes.GetResponseStream();
            StreamReader GamercardRead = new StreamReader(GamercardStream, Encoding.UTF8);
            var GamercardJson = GamercardRead.ReadToEnd();
            dynamic stuff = Newtonsoft.Json.Linq.JObject.Parse(GamercardJson);
            HttpWebRequest HttpUserFrineds = (HttpWebRequest)WebRequest.Create("https://xboxapi.com/v2/" + UserID + "/friends");
            HttpUserFrineds.Method = WebRequestMethods.Http.Get;
            HttpUserFrineds.Headers.Add("X-AUTH", Program.TokenMap.Xbox);
            HttpUserFrineds.Accept = "application/json";
            HttpWebResponse FriendsRes = (HttpWebResponse)HttpUserFrineds.GetResponse();
            Stream FriendsStream = FriendsRes.GetResponseStream();
            StreamReader FriendsRead = new StreamReader(FriendsStream, Encoding.UTF8);
            var FriendsJson = FriendsRead.ReadToEnd();
            dynamic Friends = JsonConvert.DeserializeObject(FriendsJson);
            int UserFriends = 0;
            foreach (var Item in Friends)
            {
                UserFriends++;
            }
            string Bio = "None";
            if (stuff.bio != "")
            {
                Bio = stuff.bio;
            }
            var embed = new EmbedBuilder()
            {
                Title = $"{User} | {UserOnline}",
                Description = "```md" + Environment.NewLine + $"<Tier {stuff.tier}> <Score {stuff.gamerscore}>" + Environment.NewLine + $"<Friends {UserFriends}> <Bio {Bio}>```",
                ThumbnailUrl = stuff.avatarBodyImagePath,
                Footer = new EmbedFooterBuilder()
                {
                    Text = Status
                }
            };
            await PWM.DeleteAsync();
            await Context.Channel.SendMessageAsync("", false, embed);
        }
        catch
        {
            await PWM.DeleteAsync();
            await Context.Channel.SendMessageAsync($"`Could not find user | {Status}`");
            return;
        }
    }

    [Command("mc")]
    [Remarks("mc")]
    [Summary("Minecraft game info and commands")]
    public async Task Mc()
    {
        var embed = new EmbedBuilder()
        {
            Description = "```md" + Environment.NewLine + "[ p/mc ping (IP) ]( Ping a minecraft server for status and player count | Does not work well on bungeecord )" + Environment.NewLine + "[ p/mc skin (Arg) (User) ]( Get a users skin with args | full | head | cube | steal )```",
            Author = new EmbedAuthorBuilder()
            {
                Name = "Minecraft",
                Url = "https://minecraft.net",
                IconUrl = "http://www.rw-designer.com/icon-view/5547.png"
            }
        };
        embed.AddField(x =>
        {
            x.Name = "Links"; x.Value = "`Ftb Legacy: `http://ftb.cursecdn.com/FTB2/launcher/FTB_Launcher.exe";
        });
        await Context.Channel.SendMessageAsync("", false, embed);
    }

    [Group("mc")]
    public class MC : ModuleBase
    {
        [Command("ping")]
        [Remarks("mc ping (IP)")]
        [Summary("Ping a minecraft server for online/player info")]
        public async Task Mcping(string IP = "null")
        {
            if (IP == "null")
            {
                await Context.Channel.SendMessageAsync("`IP cannot be null`");
                return;
            }
            string Players = "";
            string MaxPlayers = "";
            string Version = "";
            try
            {
                PixelBot.MineStat ms = new PixelBot.MineStat(IP, 25565);
                if (ms.ServerUp)
                {
                    Players = ms.CurrentPlayers;
                    MaxPlayers = ms.MaximumPlayers;
                    Version = ms.Version;
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Server is offline or IP invalid");
                    return;
                }
            }
            catch
            {
                await Context.Channel.SendMessageAsync("Server is offline or IP invalid");
                return;
            }
            if (Version.Contains("BungeeCord"))
            {
                await Context.Channel.SendMessageAsync($"`Server is Online! {Players}/{MaxPlayers}`" + Environment.NewLine + "Bungeecord servers will not get the correct player count");
            }
            else
            {
                await Context.Channel.SendMessageAsync($"`Server is Online! {Players}/{MaxPlayers}`");
            }
        }

        [Command("bping")]
        [Remarks("mc ping (IP)")]
        [Summary("Ping a mincraft bungeecord server for online/player info")]
        public async Task Mcbping(string IP = "null")
        {
            await Context.Channel.SendMessageAsync("`Feature under construction`");
        }

        [Command("skin")]
        [Remarks("skin (Arg) (User)")]
        [Summary("Minecraft user skin | args > head cube full steal")]
        public async Task Mcskin(string Arg = null, [Remainder] string User = null)
        {
            if (Arg == null)
            {
                await Context.Channel.SendMessageAsync("`Enter an option | p/mc skin (Arg) (User) | head | cube | full | steal");
                return;
            }
            if (User == null)
            {
                await Context.Channel.SendMessageAsync("`Enter an user | p/mc skin (Arg) (User) | head | cube | full | steal");
                return;
            }
            string Temp = Path.GetTempPath();
            WebClient Web = new WebClient();
            switch (Arg.ToLower())
            {
                case "head":
                    Web.DownloadFile("https://visage.surgeplay.com/face/100/" + User, Temp + "MC.png");
                    await Context.Channel.SendFileAsync(Temp + "MC.png");
                    File.Delete(Temp + "MC.png");
                    break;
                case "cube":
                    Web.DownloadFile("https://visage.surgeplay.com/head/100/" + User, Temp + "MC.png");
                    await Context.Channel.SendFileAsync(Temp + "MC.png");
                    File.Delete(Temp + "MC.png");
                    break;
                case "full":
                    Web.DownloadFile("https://visage.surgeplay.com/full/200/" + User, Temp + "MC.png");
                    await Context.Channel.SendFileAsync(Temp + "MC.png");
                    File.Delete(Temp + "MC.png");
                    break;
                case "steal":
                    await Context.Channel.SendMessageAsync("Click here to download > https://minotar.net/download/" + User);
                    break;
                default:
                    await Context.Channel.SendMessageAsync("`Unknown option`");
                    break;
            }
        }
    }

[Command("steam")]
    [Remarks("steam")]
    [Summary("Steam info and commands")]
    public async Task Steam()
    {
        var infoembed = new EmbedBuilder()
        {
            Author = new EmbedAuthorBuilder()
            {
                Name = "Steam",
                IconUrl = "",
                Url = "http://store.steampowered.com/"
            },
            Description = "Steam user and game lookup | Advanced game stats coming soon" + Environment.NewLine + "```md" + Environment.NewLine + "[ p/steam u (User) ]( Get info for a user )" + Environment.NewLine + "[ p/steam g (Game) ]( Get info about a game )```",
            Color = new Color(255, 105, 180)
        };
        await Context.Channel.SendMessageAsync("", false, infoembed);
    }

    public class SteamComs : ModuleBase
    {
        [Command("steam game")]
        [Alias("steam g")]
        [Remarks("steam g (Game)")]
        [Summary("Steam game info")]
        public async Task Steamg([Remainder] string Game = null)
        {
            if (Game == null)
            {
                await Context.Channel.SendMessageAsync("`Enter a game name | p/steam g (Game)`");
                return;
            }
            try
            {
                string searchQuery = Game;
                List<Listing> results = Query.Search(searchQuery);
                string cost = "";
                if (results[0].SaleType.ToString() == "FreeToPlay")
                {
                    cost = "Free!";
                }
                else
                {
                    cost = $"${results[0].PriceUSD}";
                }
                await Context.Channel.SendMessageAsync($"Steam Game {cost}" + Environment.NewLine + $"{results[0].StoreLink}");
                Console.WriteLine(results[0].AppId);
            }
            catch
            {
                await Context.Channel.SendMessageAsync("`Could not find game`");
            }

        }

        [Command("steam user")]
        [Alias("steam u")]
        [Remarks("steam u (User)")]
        [Summary("Steam user info")]
        public async Task Steamu([Remainder] string User = null)
        {
            if (User == null)
            {
                await Context.Channel.SendMessageAsync("p/steam u (User)" + Environment.NewLine + "How to get a steam user? <" + "http://i.imgur.com/pM9dff5.png" + ">");
                return;
            }
            string Claim = "";
            MySQLConnection DB;
            DB = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
            DB.Open();
            string comclaim = $"SELECT user FROM profiles WHERE steam='{User}'";
            MySQLCommand cmdclaim = new MySQLCommand(comclaim, DB);
            MySQLDataReader readclaim = cmdclaim.ExecuteReaderEx();
            DB.Close();
            if (readclaim.HasRows)
            {
                while (readclaim.Read())
                {
                    Claim = readclaim.GetString(0);
                }

            }
            SteamIdentity SteamUser = null;
            SteamWebAPI.SetGlobalKey(Program.TokenMap.Steam);
            SteamUser = SteamWebAPI.General().ISteamUser().ResolveVanityURL(User).GetResponse().Data.Identity;
            if (SteamUser == null)
            {
                await Context.Channel.SendMessageAsync("`Could not find steam user`");
            }
            var Games = SteamWebAPI.General().IPlayerService().GetOwnedGames(SteamUser).GetResponse();
            var Badges = SteamWebAPI.General().IPlayerService().GetBadges(SteamUser).GetResponse();
            var LastPlayed = SteamWebAPI.General().IPlayerService().GetRecentlyPlayedGames(SteamUser).GetResponse();
            var Friends = SteamWebAPI.General().ISteamUser().GetFriendList(SteamUser, RelationshipType.Friend).GetResponse();
            var embed = new EmbedBuilder();
            {
                embed.Title = $"Steam - {User} | Level {Badges.Data.PlayerLevel} | Xp {Badges.Data.PlayerXP}";
                embed.Url = "http://steamcommunity.com/id/" + User;
            }
            if (Claim != "")
            {
                embed.Description = $"Claimed by <@{Claim}>";
            }
            string Game1 = "-";
            string Game2 = "-";
            string Game3 = "-";
            string Game4 = "-";
            string Game5 = "-";
            string GamePlay1 = "-";
            string GamePlay2 = "-";
            string GamePlay3 = "-";
            string GamePlay4 = "-";
            string GamePlay5 = "-";
            if (LastPlayed.Data.TotalCount > 0)
            {
                Game1 = LastPlayed.Data.Games[0].Name;
                GamePlay1 = "H " + LastPlayed.Data.Games[0].PlayTimeTotal.TotalHours.ToString().Split('.')[0];
            }
            if (LastPlayed.Data.TotalCount > 1)
            {
                Game2 = LastPlayed.Data.Games[1].Name;
                GamePlay2 = "H " + LastPlayed.Data.Games[1].PlayTimeTotal.Hours.ToString();
            }
            if (LastPlayed.Data.TotalCount > 2)
            {
                Game3 = LastPlayed.Data.Games[2].Name;
                GamePlay3 = "H " + LastPlayed.Data.Games[2].PlayTimeTotal.Hours.ToString();
            }
            if (LastPlayed.Data.TotalCount > 3)
            {
                Game4 = LastPlayed.Data.Games[3].Name;
                GamePlay4 = "H " + LastPlayed.Data.Games[3].PlayTimeTotal.Hours.ToString();
            }
            if (LastPlayed.Data.TotalCount > 4)
            {
                Game5 = LastPlayed.Data.Games[4].Name;
                GamePlay5 = "H " + LastPlayed.Data.Games[4].PlayTimeTotal.Hours.ToString();
            }
            embed.AddField(x =>
            {
                x.Name = "Info"; x.Value = "```md" + Environment.NewLine + $"<Friends {Friends.Data.Friends.Count}>" + Environment.NewLine + $"<Games {Games.Data.GameCount}>" + Environment.NewLine + $"<Badges {Badges.Data.Badges.Count}>```"; x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "Game"; x.Value = Game1 + Environment.NewLine + Game2 + Environment.NewLine + Game3 + Environment.NewLine + Game4 + Environment.NewLine + Game5; x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "Playtime"; x.Value = GamePlay1 + Environment.NewLine + GamePlay2 + Environment.NewLine + GamePlay3 + Environment.NewLine + GamePlay4 + Environment.NewLine + GamePlay5; x.IsInline = true;
            });
            await Context.Channel.SendMessageAsync("", false, embed);
        }
    }

    [Command("osu")]
    [Remarks("osu (User)")]
    [Summary("osu! game user info")]
    public async Task Osu(string User = null)
    {
        if (User == null)
        {
            var infoembed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = "Osu!",
                    IconUrl = "http://orig09.deviantart.net/0c0c/f/2014/223/1/5/osu_icon_by_gentheminer-d7unrx3.png",
                    Url = "https://osu.ppy.sh/"
                },
                Description = "Osu is a free rhythm game for windows/ios and mobile platforms with custom skins and beatmaps for songs" + Environment.NewLine + "```md" + Environment.NewLine + "[ p/osu (User) ]( Get info for a user )```",
                Color = new Color(255, 105, 180)
            };
            await Context.Channel.SendMessageAsync("", false, infoembed);
            return;
        }
        try
        {
            HttpWebRequest GetUserId = (HttpWebRequest)WebRequest.Create("https://osu.ppy.sh/api/get_user?k=" + Program.TokenMap.Osu + "&u=" + User);
            GetUserId.Method = WebRequestMethods.Http.Get;
            GetUserId.Accept = "application/json";
            HttpWebResponse response = (HttpWebResponse)GetUserId.GetResponse();
            Console.WriteLine(response.StatusCode);
            Stream FriendsStream = response.GetResponseStream();
            StreamReader FriendsRead = new StreamReader(FriendsStream, Encoding.UTF8);
            var Item = Newtonsoft.Json.Linq.JArray.Parse(FriendsRead.ReadToEnd())[0];
            int Count = 0;
            string OsuUser = "";
            string OsuPlaycount = "";
            string OsuRanked = "";
            string OsuTotal = "";
            string OsuLevel = "";
            string OsuAccuracy = "";
            string OsuRankSS = "";
            string OsuRankS = "";
            string OsuRankA = "";
            foreach (var Items in Item)
            {
                if (Count == 1)
                {
                    OsuUser = Items.First.ToString();
                }
                if (Count == 5)
                {
                    OsuPlaycount = Items.First.ToString();
                }
                if (Count == 6)
                {
                    OsuRanked = Items.First.ToString();
                }
                if (Count == 7)
                {
                    OsuTotal = Items.First.ToString();
                }
                if (Count == 9)
                {
                    OsuLevel = Items.First.ToString();
                }
                if (Count == 11)
                {
                    OsuAccuracy = Items.First.ToString();
                }
                if (Count == 12)
                {
                    OsuRankSS = Items.First.ToString();
                }
                if (Count == 13)
                {
                    OsuRankS = Items.First.ToString();
                }
                if (Count == 14)
                {
                    OsuRankA = Items.First.ToString();
                }
                Count++;
            }
            Embed embed = new EmbedBuilder()
            {
                Title = $"{OsuUser} | (Level: {OsuLevel.Split('.').First()})",
                Description = "```md" + Environment.NewLine + $"<Played {OsuPlaycount}> <Accuracy {OsuAccuracy.Split('.').First()}>" + Environment.NewLine + $"[ A: {OsuRankA} S: {OsuRankS} SS: {OsuRankSS} ](Rank)" + Environment.NewLine + $"[ Total: {OsuTotal} Ranked: {OsuRanked} ](Score)```",
                ThumbnailUrl = "http://orig09.deviantart.net/0c0c/f/2014/223/1/5/osu_icon_by_gentheminer-d7unrx3.png",
                Url = "https://osu.ppy.sh/u/" + OsuUser,
                Color = new Color(255, 105, 180)
            };
            await Context.Channel.SendMessageAsync("", false, embed);
        }
        catch
        {
            await Context.Channel.SendMessageAsync("`Cannot find osu user`");
        }
    }
}

public class Media : ModuleBase
{
    [Command("tw search")]
    [Alias("tw s")]
    [Remarks("tw s (User)")]
    [Summary("Search for a twitch channel")]
    public async Task TwitchSearch(string Search = null)
    {
        if (Search == null)
        {
            await Context.Channel.SendMessageAsync("`Enter a channel name to search | p/tw s (User)`");
            return;
        }
        var client = new TwitchAuthenticatedClient(Program.TokenMap.Twitch, Program.TokenMap.TwitchAuth);
        var Usearch = client.SearchChannels(Search).List;
        var embed = new EmbedBuilder()
        {
            Title = "Twitch Channels",
            Description = $"{Usearch[0].Name} | {Usearch[1].Name} | {Usearch[2].Name}",
            Color = Program.GetRoleColor(Context)
        };
        if (Context.Channel is IPrivateChannel)
        {
            await Context.Channel.SendMessageAsync("", false, embed);
            return;
        }

        IGuildUser BotUser = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);

        if (BotUser.GetPermissions(Context.Channel as ITextChannel).EmbedLinks)
        {
            await Context.Channel.SendMessageAsync("", false, embed);
        }
        else
        {
            await Context.Channel.SendMessageAsync("**Twitch Channels**" + Environment.NewLine + $"{Usearch[0].Name} | {Usearch[1].Name} | {Usearch[2].Name}");
        }
    }

    [Command("tw")]
    [Remarks("tw")]
    [Summary("Twitch commands")]
    public async Task Twitch()
    {
        var infoembed = new EmbedBuilder()
        {
            Author = new EmbedAuthorBuilder()
            {
                Name = "Twitch",
                IconUrl = "http://vignette3.wikia.nocookie.net/logopedia/images/8/83/Twitch_icon.svg/revision/latest/scale-to-width-down/421?cb=20140727180700",
                Url = "https://www.twitch.tv/"
            },
            Description = "Twitch channel lookup/search and livestream notifications in channel or user DMs" + Environment.NewLine + "```md" + Environment.NewLine + "[ p/tw (Channel) ]( Get info about a channel )" + Environment.NewLine + "[ p/tw s (Channel ]( Get 3 channel names )" + Environment.NewLine + "[ p/tw n (Option) (Channel) ]( Get a notification when a streamer goes live )" + Environment.NewLine + "[ p/tw l (Option) ]( Get a list of notification settings )" + Environment.NewLine + "[ p/tw r (Option) (Channel) ]( Remove a channel from notification setting )```",
            Footer = new EmbedFooterBuilder()
            {
                Text = "Options > ME (User DM) | HERE (Guild Channel)"
            },
            Color = Program.GetRoleColor(Context)
        };
        if (Context.Channel is IPrivateChannel)
        {
            await Context.Channel.SendMessageAsync("", false, infoembed);
        }
        else
        {
            IGuildUser BotUser = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
            if (BotUser.GetPermissions(Context.Channel as ITextChannel).EmbedLinks)
            {
                await Context.Channel.SendMessageAsync("", false, infoembed);
            }
            else
            {
                await Context.Channel.SendMessageAsync("Twitch channel lookup/search and livestream notifications in channel or user DMs" + Environment.NewLine + "```md" + Environment.NewLine + "[ p/tw (Channel) ]( Get info about a channel )" + Environment.NewLine + "[ p/tw s (Channel ]( Get 3 channel names )" + Environment.NewLine + "[ p/tw n (Option) (Channel) ]( Get a notification when a streamer goes live )" + Environment.NewLine + "[ p/tw l (Option) ]( Get a list of notification settings )" + Environment.NewLine + "[ p/tw r (Option) (Channel) ]( Remove a channel from notification setting )" + Environment.NewLine + "<Options > ME (User DM) | HERE (Guild Channel)```");
            }
        }
    }

    [Command("tw c")]
    [Remarks("tw c (Channel)")]
    [Summary("Twitch channel info")]
    public async Task TwitchChannel(string Channel = null)
    {
        if (Channel == null)
        {
            await Context.Channel.SendMessageAsync("`Enter a channel name | p/tw c MyChannel`");
            return;
        }
        var client = new TwitchAuthenticatedClient(Program.TokenMap.Twitch, Program.TokenMap.TwitchAuth);
        var t = client.GetChannel(Channel);
        if (t.CreatedAt.Year == 0001)
        {
            await Context.Channel.SendMessageAsync("`Cannot find this channel`");
            return;
        }
        string EmbedTitle = $"{t.DisplayName} - (Offline)";
        string EmbedText = "```md" + Environment.NewLine + $"<Created {t.CreatedAt.ToShortDateString()}> <Updated {t.UpdatedAt.ToShortDateString()}>" + Environment.NewLine + $"<Followers {t.Followers}> <Views {t.Views}>```";
        if (client.IsLive(Channel) == true)
        {
            EmbedTitle = $"{t.DisplayName} - Playing {t.Game}";
            EmbedText = t.Status + Environment.NewLine + EmbedText;
        }
        var embed = new EmbedBuilder()
        {
            Title = EmbedTitle,
            Url = t.Url,
            ThumbnailUrl = t.Logo,
            Description = EmbedText,
            Color = new Color(255, 0, 0),
            Footer = new EmbedFooterBuilder()
            {
                Text = $"Subscribe to streamer online alerts with | p/tw notify me {Channel}"
            }
        };
        if (!EmbedTitle.Contains("(Offline)"))
        {
            embed.Color = new Color(0, 255, 0);
        }
        Console.WriteLine(t.Game);
        var a = client.GetMyStream();
        await Context.Channel.SendMessageAsync("", false, embed);
    }

    [Command("tw notify")]
    [Alias("tw n")]
    [Remarks("tw n (Option) (Channel)")]
    [Summary("Recieve notifications from this twitch channel")]
    public async Task TwitchNotify(string Option = null, string Channel = null)
    {
        if (Context.Channel is IPrivateChannel)
        {
            await Context.Channel.SendMessageAsync("`Guild channel only!`");
            return;
        }
        IGuildUser BotUser = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
        if (!BotUser.GetPermissions(Context.Channel as ITextChannel).EmbedLinks)
        {
            await Context.Channel.SendMessageAsync("`Bot required permission Embed Links`");
            return;
        }
        if (Option == null)
        {
            await Context.Channel.SendMessageAsync("`Enter an option | p/tw notify me (Channel) - Sends a message in DMS | p/tw notify here (Channel) - Sends a message in this channel (Server Owner Only!)`");
            return;
        }
        if (Channel == null)
        {
            await Context.Channel.SendMessageAsync("`Enter a channel name | p/tw notify me (Channel) - Sends a message in DMS | p/tw notify here (Channel) - Sends a message in this channel (Server Owner Only!)`");
            return;
        }
        var client = new TwitchAuthenticatedClient(Program.TokenMap.Twitch, Program.TokenMap.TwitchAuth);
        var t = client.GetChannel(Channel);
        if (t.CreatedAt.Year == 0001)
        {
            await Context.Channel.SendMessageAsync("`Cannot find this channel`");
            return;
        }
        if (Option == "me")
        {
            MySQLConnection myConn;
            MySQLDataReader MyReader = null;
            myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
            myConn.Open();
            string RS = $"SELECT twitch FROM twitch WHERE type='user' AND userid='{Context.User.Id}' AND twitch='{Channel.ToLower()}'";
            MySQLCommand cmd = new MySQLCommand(RS, myConn);
            MyReader = cmd.ExecuteReaderEx();
            myConn.Close();
            if (MyReader.HasRows)
            {
                await Context.Channel.SendMessageAsync($"`You are already getting notification from {Channel}`");
            }
            else
            {
                myConn.Open();
                string stm = $"INSERT INTO twitch(type, userid, guild, channel, twitch, live) VALUES('user', '{Context.User.Id}', '{Context.Guild.Id}', '{Context.Channel.Id}', '{Channel.ToLower()}', 'no')";
                MySQLCommand cmd2 = new MySQLCommand(stm, myConn);
                cmd2.ExecuteNonQuery();
                myConn.Close();
                await Context.Channel.SendMessageAsync($"`User notification added for {Channel} you will get a notification in DMS when he/she goes live`");
            }
        }
        if (Option == "here")
        {
            if (Context.User.Id != Context.Guild.OwnerId)
            {
                await Context.Channel.SendMessageAsync("`You are not the guild owner`");
                return;
            }
            MySQLConnection myConn;
            MySQLDataReader MyReader = null;
            myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
            myConn.Open();
            string RS = $"SELECT userid FROM twitch WHERE type='channel' AND guild='{Context.Guild.Id}' AND channel='{Context.Channel.Id}' AND twitch='{Channel.ToLower()}'";
            MySQLCommand cmd = new MySQLCommand(RS, myConn);
            MyReader = cmd.ExecuteReaderEx();
            myConn.Close();
            if (MyReader.HasRows)
            {
                IUser GuildUser = null;
                while (MyReader.Read())
                {
                    try
                    {
                        GuildUser = await Context.Guild.GetUserAsync(Convert.ToUInt64(MyReader.GetString(0)));
                    }
                    catch
                    {

                    }
                }
                if (GuildUser == null)
                {
                    await Context.Channel.SendMessageAsync($"`This guild channel is already getting notification from {Channel} | Unknown User`");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"`This guild channel is already getting notification from {Channel} | Created by {GuildUser.Username}`");
                }
            }
            else
            {
                myConn.Open();
                string stm = $"INSERT INTO twitch(type, userid, guild, channel, twitch, live) VALUES('channel', '{Context.User.Id}', '{Context.Guild.Id}', '{Context.Channel.Id}', '{Channel.ToLower()}', 'no')";
                MySQLCommand cmd2 = new MySQLCommand(stm, myConn);
                cmd2.ExecuteNonQuery();
                myConn.Close();
                await Context.Channel.SendMessageAsync($"`Guild channel notification added for {Channel} you will get a notification here when he/she goes live`");
            }
        }
    }

    [Command("tw list")]
    [Alias("tw l")]
    [Remarks("tw list (Option)")]
    [Summary("List your twitch notification settings")]
    public async Task TwitchList(string Option = null)
    {
        if (Context.Channel is IPrivateChannel)
        {
            await Context.Channel.SendMessageAsync("`Guild channel only!`");
            return;
        }
        if (Option == null)
        {
            await Context.Channel.SendMessageAsync("`Enter an option | p/tw list me | p/tw list guild | p/tw list here`");
            return;
        }
        if (Option == "me")
        {
            List<string> TWList = new List<string>();
            MySQLConnection myConn;
            MySQLDataReader MyReader = null;
            myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
            myConn.Open();
            string RS = $"SELECT twitch FROM twitch WHERE type='user' AND userid='{Context.User.Id}'";
            MySQLCommand cmd = new MySQLCommand(RS, myConn);
            MyReader = cmd.ExecuteReaderEx();
            myConn.Close();
            if (!MyReader.HasRows)
            {
                await Context.Channel.SendMessageAsync($"`You do not have an twitch notifications`");
                return;
            }
            while (MyReader.Read())
            {
                TWList.Add(MyReader.GetString(0));
            }
            var line = string.Join(", ", TWList.ToArray());
            await Context.Channel.SendMessageAsync("You are getting notification from" + Environment.NewLine + line);
        }
        if (Option == "guild")
        {
            List<string> TWList = new List<string>();
            MySQLConnection myConn;
            MySQLDataReader MyReader = null;
            myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
            myConn.Open();
            string RS = $"SELECT twitch, channel FROM twitch WHERE type='channel' AND guild='{Context.Guild.Id}'";
            MySQLCommand cmd = new MySQLCommand(RS, myConn);
            MyReader = cmd.ExecuteReaderEx();
            myConn.Close();
            if (!MyReader.HasRows)
            {
                await Context.Channel.SendMessageAsync($"`This guild does not have any twitch notifications`");
                return;
            }
            while (MyReader.Read())
            {
                TWList.Add($"{MyReader.GetString(0)}");
            }
            var line = string.Join(", ", TWList.ToArray());
            await Context.Channel.SendMessageAsync("This guild is getting notification for" + Environment.NewLine + line);
        }
        if (Option == "here")
        {
            List<string> TWList = new List<string>();
            MySQLConnection myConn;
            MySQLDataReader MyReader = null;
            myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
            myConn.Open();
            string RS = $"SELECT twitch FROM twitch WHERE type='channel' AND guild='{Context.Guild.Id}' AND channel='{Context.Channel.Id}'";
            MySQLCommand cmd = new MySQLCommand(RS, myConn);
            MyReader = cmd.ExecuteReaderEx();
            myConn.Close();
            if (!MyReader.HasRows)
            {
                await Context.Channel.SendMessageAsync($"`This channel does not have any twitch notifications`");
                return;
            }
            while (MyReader.Read())
            {
                TWList.Add(MyReader.GetString(0));
            }
            var line = string.Join(", ", TWList.ToArray());
            await Context.Channel.SendMessageAsync("This channel is getting notification from" + Environment.NewLine + line);
        }
    }

    [Command("tw remove")]
    [Alias("tw r")]
    [Remarks("tw r (Option) (Channel)")]
    [Summary("Remove notifications from a twitch channel")]
    public async Task TwitchRemove(string Option = null, string Channel = null)
    {
        if (Context.Channel is IPrivateChannel)
        {
            await Context.Channel.SendMessageAsync("`Guild channel only!`");
            return;
        }
        if (Option == null)
        {
            await Context.Channel.SendMessageAsync("`Enter an option | p/tw remove me (Channel) | p/tw remove here (Channel)");
            return;
        }
        if (Channel == null)
        {
            await Context.Channel.SendMessageAsync("`Enter a channel name | p/tw remove me (Channel) | p/tw remove here (Channel)");
            return;
        }
        if (Option == "me")
        {
            MySQLConnection myConn;
            MySQLDataReader MyReader = null;
            myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
            myConn.Open();
            string RS = $"SELECT twitch FROM twitch WHERE type='user' AND userid='{Context.User.Id}' AND twitch='{Channel.ToLower()}'";
            MySQLCommand cmd = new MySQLCommand(RS, myConn);
            MyReader = cmd.ExecuteReaderEx();
            myConn.Close();
            if (!MyReader.HasRows)
            {
                await Context.Channel.SendMessageAsync($"`You are not getting notifications from this twitch channel`");
                return;
            }
            myConn.Open();
            string stm = $"DELETE FROM twitch WHERE type='user' AND userid='{Context.User.Id}' AND twitch='{Channel.ToLower()}'";
            MySQLCommand cmd2 = new MySQLCommand(stm, myConn);
            cmd2.ExecuteNonQuery();
            myConn.Close();
            await Context.Channel.SendMessageAsync($"You have removed {Channel} from your notifications");
        }
        if (Option == "here")
        {
            if (Context.User.Id != Context.Guild.OwnerId)
            {
                await Context.Channel.SendMessageAsync("`You are not the guild owner`");
                return;
            }
            MySQLConnection myConn;
            MySQLDataReader MyReader = null;
            myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
            myConn.Open();
            string RS = $"SELECT twitch FROM twitch WHERE type='channel' AND guild='{Context.Guild.Id}' AND channel='{Context.Channel.Id}' AND twitch='{Channel.ToLower()}'";
            MySQLCommand cmd = new MySQLCommand(RS, myConn);
            MyReader = cmd.ExecuteReaderEx();
            myConn.Close();
            if (!MyReader.HasRows)
            {
                await Context.Channel.SendMessageAsync($"`This channel does not have a notification set for {Channel}`");
                return;
            }
            myConn.Open();
            string stm = $"DELETE FROM twitch WHERE type='channel' AND guild='{Context.Guild.Id}' AND channel='{Context.Channel.Id}' AND twitch='{Channel.ToLower()}'";
            MySQLCommand cmd2 = new MySQLCommand(stm, myConn);
            cmd2.ExecuteNonQuery();
            myConn.Close();
            await Context.Channel.SendMessageAsync($"You have remove {Channel} notifications from this channel");
        }
    }
}

public class Prune : ModuleBase
{
    [Group("prune")][Alias("purge", "tidy", "clean")]
    public class PruneModule : ModuleBase
    {
        [Command("all")]
        [Remarks("prune all (Ammount)")]
        [Summary("Prune all messages | Not pinned")]
        public async Task Pruneall(int Ammount = 30)
        {
            IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
            if (!Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`Bot does not have permission to manage messages");
                return;
            }
            await Context.Message.DeleteAsync();
            var GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
            if (!GuildUser.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`You do not have permission to manage messages`");
                return;
            }
            if (Ammount < 0)
            {
                await Context.Channel.SendMessageAsync("`Ammount cannot be less than 0`");
                return;
            }
            if (Ammount > 30)
            {
                await Context.Channel.SendMessageAsync("`Ammount cannot be more than 30`");
                return;
            }
            int Count = 0;
            foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
            {
                if (Count != Ammount)
                {
                    Count++;
                    await Item.DeleteAsync();
                }
            }
            await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Count} messages`");
        }

        [Command("user")]
        [Remarks("prune user (@Mention/User ID) (Ammount)")]
        [Summary("Prune messages made by thi user")]
        public async Task Pruneuser(IUser User = null, int Ammount = 30)
        {
            if (User == null)
            {
                await Context.Channel.SendMessageAsync("`You need to select a user p/prune user @User`");
                return;
            }
            IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
            if (!Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`Bot does not have permission to manage messages`");
                return;
            }
            await Context.Message.DeleteAsync();
            var GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
            if (!GuildUser.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`You do not have permission to manage messages`");
                return;
            }
            if (Ammount < 0)
            {
                await Context.Channel.SendMessageAsync("`Ammount cannot be less than 0`");
                return;
            }
            if (Ammount > 30)
            {
                await Context.Channel.SendMessageAsync("`Ammount cannot be more than 30`");
                return;
            }
            int Count = 0;
            foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
            {
                if (Count != Ammount & Item.Author.Id == User.Id)
                {
                    Count++;
                    await Item.DeleteAsync();
                }
            }
            await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Count} user messages`");
        }

        [Command("bot")]
        [Alias("bots")]
        [Remarks("prune bot (Ammount)")]
        [Summary("Prune messages made by bots")]
        public async Task Prunebot(int Ammount = 30)
        {
            IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
            if (!Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`Bot does not have permission to manage messages`");
                return;
            }
            await Context.Message.DeleteAsync();
            var GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
            if (!GuildUser.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`You do not have permission to manage messages`");
                return;
            }
            if (Ammount < 0)
            {
                await Context.Channel.SendMessageAsync("`Ammount cannot be less than 0`");
                return;
            }
            if (Ammount > 30)
            {
                await Context.Channel.SendMessageAsync("`Ammount cannot be more than 30`");
                return;
            }
            int Count = 0;
            foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
            {
                if (Count != Ammount & Item.Author.IsBot)
                {
                    Count++;
                    await Item.DeleteAsync();
                }
            }
            await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Count} bot messages`");
        }

        [Command("image")]
        [Alias("images")]
        [Remarks("prune image (Ammount)")]
        [Summary("Prune messages that have an attachment")]
        public async Task Pruneimage(int Ammount = 30)
        {
            IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
            if (!Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`Bot does not have permission to manage messages`");
                return;
            }
            await Context.Message.DeleteAsync();
            var GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
            if (!GuildUser.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`You do not have permission to manage messages`");
                return;
            }
            if (Ammount < 0)
            {
                await Context.Channel.SendMessageAsync("`Ammount cannot be less than 0`");
                return;
            }
            if (Ammount > 30)
            {
                await Context.Channel.SendMessageAsync("`Ammount cannot be more than 30`");
                return;
            }
            int Count = 0;
            foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
            {
                if (Count != Ammount & Item.Attachments.Any())
                {
                    Count++;
                    await Item.DeleteAsync();
                }
            }
            await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Count} images`");
        }

        [Command("embed")]
        [Alias("embeds")]
        [Remarks("prune embed (Ammount)")]
        [Summary("Prune messages that have an embed")]
        public async Task Pruneembed(int Ammount = 30)
        {
            IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
            if (!Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`Bot does not have permission to manage messages`");
                return;
            }
            await Context.Message.DeleteAsync();
            var GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
            if (!GuildUser.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`You do not have permission to manage messages`");
                return;
            }
            if (Ammount < 0)
            {
                await Context.Channel.SendMessageAsync("`Ammount cannot be less than 0`");
                return;
            }
            if (Ammount > 30)
            {
                await Context.Channel.SendMessageAsync("`Ammount cannot be more than 30`");
                return;
            }
            int Count = 0;
            foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
            {
                if (Count != Ammount & Item.Embeds.Any())
                {
                    Count++;
                    await Item.DeleteAsync();
                }
            }
            await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Count} embeds`");
        }

        [Command("link")]
        [Alias("links")]
        [Remarks("prune link (Ammount)")]
        [Summary("Prune messages that have a link")]
        public async Task Prunelinks(int Ammount = 30)
        {
            IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
            if (!Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`Bot does not have permission to manage messages`");
                return;
            }
            await Context.Message.DeleteAsync();
            var GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
            if (!GuildUser.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`You do not have permission to manage messages`");
                return;
            }
            if (Ammount < 0)
            {
                await Context.Channel.SendMessageAsync("`Ammount cannot be less than 0`");
                return;
            }
            if (Ammount > 30)
            {
                await Context.Channel.SendMessageAsync("`Ammount cannot be more than 30`");
                return;
            }
            int Count = 0;
            foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
            {
                if (Count != Ammount & Item.Content.Contains("http://") || Item.Content.Contains("https://"))
                {
                    Count++;
                    await Item.DeleteAsync();
                }
            }
            await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Count} links`");
        }

        [Command("command")]
        [Alias("commands")]
        [Remarks("prune command")]
        [Summary("Prune messages that are a bot prefix like /help p/help $help *help")]
        public async Task Prunecommands(int Ammount = 30)
        {
            IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
            if (!Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`Bot does not have permission to manage messages`");
                return;
            }
            await Context.Message.DeleteAsync();
            var GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
            if (!GuildUser.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`You do not have permission to manage messages`");
                return;
            }
            if (Ammount < 0)
            {
                await Context.Channel.SendMessageAsync("`Ammount cannot be less than 0`");
                return;
            }
            if (Ammount > 30)
            {
                await Context.Channel.SendMessageAsync("`Ammount cannot be more than 30`");
                return;
            }
            int Count = 0;
            foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
            {
                if (Count != Ammount & Item.Content.StartsWith("p/") || Item.Content.StartsWith("/") || Item.Content.StartsWith("!") || Item.Content.StartsWith(",") || Item.Content.StartsWith("=") || Item.Content.StartsWith("%") || Item.Content.StartsWith("b!"))
                {
                    Count++;
                    await Item.DeleteAsync();
                }
            }
            await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Count} commands`");
        }

        [Command("text")]
        [Remarks("prune (Text Here)")]
        [Summary("Prune messages that contain this text")]
        public async Task Prunetext([Remainder] string Text = null)
        {
            IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
            if (!Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`Bot does not have permission to manage messages`");
                return;
            }
            await Context.Message.DeleteAsync();
            var GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
            if (!GuildUser.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`You do not have permission to manage messages`");
                return;
            }
            if (Text == null)
            {
                await Context.Channel.SendMessageAsync("`You need to specify text | p/prune text (Text) | Replace (Text) with anything`");
            }
            int Count = 0;
            foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
            {
                if (Item.Content.Contains(Text))
                {
                    Count++;
                    await Item.DeleteAsync();
                }
            }
            await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Count} messages`");
        }
    }
}

public class Help : ModuleBase
{
    private readonly PaginationService paginator = Program._pagination;
    [Command("help")]
    [Alias("commands")]
    public async Task Pag(string Option = "")
    {
        List<string> MiscList = new List<string>();
        List<string> GameList = new List<string>();
        List<string> MediaList = new List<string>();
        List<string> PruneList = new List<string>();
        foreach (var CMD in Program._commands.Commands.Where(x => x.Module.Name == "Misc"))
        {
            MiscList.Add($"[ p/{CMD.Remarks} ][ {CMD.Summary} ]");
        }
        foreach (var CMD in Program._commands.Commands.Where(x => x.Module.Name == "Game"))
        {
            GameList.Add($"[ p/{CMD.Remarks} ][ {CMD.Summary} ]");
        }
        foreach (var CMD in Program._commands.Commands.Where(x => x.Module.Name == "Media"))
        {
            MediaList.Add($"[ p/{CMD.Remarks} ][ {CMD.Summary} ]");
        }
        foreach (var CMD in Program._commands.Commands.Where(x => x.Module.Name == "prune"))
        {
            PruneList.Add($"[ p/{CMD.Remarks} ][ {CMD.Summary} ]");
        }
        string MiscText = string.Join(Environment.NewLine, MiscList);
        string GameText = string.Join(Environment.NewLine, GameList);
        string MediaText = string.Join(Environment.NewLine, MediaList);
        string PruneText = string.Join(Environment.NewLine, PruneList);
        IGuildUser BotUser = null;
        if (Context.Channel is IPrivateChannel || Option == "all")
        {
            if (Option == "all")
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Username} I have sent you a full list of commands");
            }
            var allemebed = new EmbedBuilder()
            {
                Title = "Commands List",
                Color = Program.GetRoleColor(Context)
            };
            allemebed.AddField(x =>
            {
                x.Name = "Misc"; x.Value = "```md" + Environment.NewLine + MiscText + "```";
            });
            allemebed.AddField(x =>
            {
                x.Name = "Game"; x.Value = "```md" + Environment.NewLine + GameText + "```";
            });
            allemebed.AddField(x =>
            {
                x.Name = "Media"; x.Value = "```md" + Environment.NewLine + MediaText + "```";
            });
            allemebed.AddField(x =>
            {
                x.Name = "Prune"; x.Value = "```md" + Environment.NewLine + PruneText + "```";
            });
            allemebed.Color = new Color(0, 191, 255);
            var DM = await Context.User.CreateDMChannelAsync();
            await DM.SendMessageAsync("", false, allemebed);
            return;
        }
        else
        {
            BotUser = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id) as IGuildUser;
        }
        string HelpText = "**Help Commands**```md" + Environment.NewLine + "[ p/misc ]( Guild/User Info | Dice Roll | Coin Flip )" + Environment.NewLine + "[ p/game ]( Steam | Osu! | Minecraft | Xbox )" + Environment.NewLine + "[ p/media ]( Twitch Commands )" + Environment.NewLine + "[ p/prune ]( Prune Messages | Embeds | Links )```For a list of all commands do p/help all";
        if (!BotUser.GetPermissions(Context.Channel as ITextChannel).EmbedLinks)
        {
            await Context.Channel.SendMessageAsync(HelpText);
            return;
        }
        if (!BotUser.GetPermissions(Context.Channel as ITextChannel).AddReactions || !BotUser.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
        {
            string PermReact = "Add Reactions: :x:";
            string PermManage = "Manage Messages :x:";
            if (BotUser.GetPermissions(Context.Channel as ITextChannel).AddReactions)
            {
                PermReact = "Add Reactions :white_check_mark: ";
            }
            if (BotUser.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                PermManage = "Manage Messages :white_check_mark: ";
            }
            var embed = new EmbedBuilder()
            {
                Title = "Help Commands",
                Description = HelpText + "For an interactive help menu add these permissions" + Environment.NewLine + $"{PermReact} | {PermManage}",
                Footer = new EmbedFooterBuilder()
                {
                    Text = "To get a full list of commands do | p/help all | Or visit the website http://blaze.ml"
                },
                Color = Program.GetRoleColor(Context)
            };
            await Context.Channel.SendMessageAsync("", false, embed);
            return;
        }
        var Guilds = await Context.Client.GetGuildsAsync();
        var pages = new List<string>
            {
                "```md" + Environment.NewLine + "< | Info     | Commands ► >" + Environment.NewLine + "<Language C#> <Library .net 1.0>" + Environment.NewLine + $"<Guilds {Guilds.Count}>``` For a full list of commands do **p/help all** or visit the website" + Environment.NewLine + "[Website](https://blaze.ml) | [Invite Bot](https://discordapp.com/oauth2/authorize?&client_id=277933222015401985&scope=bot&permissions=0) | [Github](https://github.com/ArchboxDev/PixelBot) | [My Guild](http://discord.gg/WJTYdNb)",
                "```md" + Environment.NewLine + "< ◄ Info |     Misc     | Games ► >" + Environment.NewLine + MiscText + "```",
                "```md" + Environment.NewLine + "< ◄ Misc |     Games     | Media ► >" + Environment.NewLine + GameText + "```",
                "```md" + Environment.NewLine + "< ◄ Games |     Media     | Prune ► >" + Environment.NewLine + MediaText + "```",
            "```md" + Environment.NewLine + "< ◄ Games |     Prune | >" + Environment.NewLine + PruneText + "```"
        };
        var message = new PaginatedMessage(pages, "Commands List", Program.GetRoleColor(Context), Context.User);
        await paginator.SendPaginatedMessageAsync(Context.Channel, message);
    }

    [Command("misc")]
    [Alias("game", "media", "prune", "purge", "clean", "tidy")]
    public async Task Misc()
    {
        if (Context.Message.Content.ToLower().EndsWith("/misc"))
        {
            List<string> CommandList = new List<string>();
            foreach (var CMD in Program._commands.Commands.Where(x => x.Module.Name == "Misc"))
            {
                CommandList.Add($"[ p/{CMD.Remarks} ][ {CMD.Summary} ]");
            }
            string Commands = string.Join(Environment.NewLine, CommandList);
            await Context.Channel.SendMessageAsync("Misc Commands```md" + Environment.NewLine + Commands + "```");
        }

        if (Context.Message.Content.ToLower().EndsWith("/game") || Context.Message.Content.ToLower().EndsWith("/games"))
        {
            List<string> CommandList = new List<string>();
            foreach (var CMD in Program._commands.Commands.Where(x => x.Module.Name == "Game"))
            {
                CommandList.Add($"[ p/{CMD.Remarks} ][ {CMD.Summary} ]");
            }
            string Commands = string.Join(Environment.NewLine, CommandList);
            await Context.Channel.SendMessageAsync("Game Commands```md" + Environment.NewLine + Commands + "```");
        }

        if (Context.Message.Content.ToLower().EndsWith("/media"))
        {
            List<string> CommandList = new List<string>();
            foreach (var CMD in Program._commands.Commands.Where(x => x.Module.Name == "Media"))
            {
                CommandList.Add($"[ p/{CMD.Remarks} ][ {CMD.Summary} ]");
            }
            string Commands = string.Join(Environment.NewLine, CommandList);
            await Context.Channel.SendMessageAsync("Media Commands```md" + Environment.NewLine + Commands + "```");
        }

        if (Context.Message.Content.ToLower().EndsWith("/prune") || Context.Message.Content.ToLower().EndsWith("/purge") || Context.Message.Content.ToLower().EndsWith("/clean") || Context.Message.Content.ToLower().EndsWith("/tidy"))
        {
            List<string> CommandList = new List<string>();
            foreach (var CMD in Program._commands.Commands.Where(x => x.Module.Name == "prune"))
            {
                CommandList.Add($"[ p/{CMD.Remarks} ][ {CMD.Summary} ]");
            }
            string Commands = string.Join(Environment.NewLine, CommandList);
            await Context.Channel.SendMessageAsync("Prune Commands```md" + Environment.NewLine + Commands + "```");
        }
    }

    [Command("prefix")]
    public async Task Prefix()
    {
        await Context.Channel.SendMessageAsync("Prefix is `p/` e.g `p/help`");
    }
}

public class Steam : InteractiveModuleBase
{
    [Command("steam claim", RunMode = RunMode.Async)]
    public async Task Steamclaim([Remainder] string User = null)
    {
        if (User == null)
        {
            await Context.Channel.SendMessageAsync("`No user set | p/steam claim (User)");
            return;
        }
        MySQLConnection myConn;
        MySQLDataReader MyReader = null;
        myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
        myConn.Open();
        string RS2 = $"SELECT user FROM profiles WHERE steam='{User}'";
        MySQLCommand cmd3 = new MySQLCommand(RS2, myConn);
        MyReader = cmd3.ExecuteReaderEx();
        myConn.Close();
        if (MyReader.HasRows)
        {
            while (MyReader.Read())
            {
                var claimed = new EmbedBuilder()
                {
                    Title = "Account Already Claimed By",
                    Description = $"<@{MyReader.GetString(0)}>"
                };
                await Context.Channel.SendMessageAsync("", false, claimed);
                return;
            }
        }
        SteamWebAPI.SetGlobalKey(Program.TokenMap.Steam);
        SteamIdentity SteamUser = null;
        try
        {
            SteamUser = SteamWebAPI.General().ISteamUser().ResolveVanityURL(User).GetResponse().Data.Identity;
        }
        catch
        {
            await Context.Channel.SendMessageAsync("`Could not find steam user`");
            return;
        }
        var Games = SteamWebAPI.General().IPlayerService().GetOwnedGames(SteamUser).GetResponse();
        var Badges = SteamWebAPI.General().IPlayerService().GetBadges(SteamUser).GetResponse();
        var embed = new EmbedBuilder()
        {
            Title = $"{User} - Level {Badges.Data.PlayerLevel} - Games {Games.Data.GameCount}",
            Description = "Would you like to claim this account? | yes / no",
            Url = "http://steamcommunity.com/id/" + User
        };
        var claimtext = await Context.Channel.SendMessageAsync("", false, embed);
        var response = await WaitForMessage(Context.Message.Author, Context.Channel, TimeSpan.FromMinutes(1));
        await claimtext.DeleteAsync();
        if (response.Content.ToLower() == "yes")
        {
            myConn.Open();
            string update = $"UPDATE profiles SET steam='{User}' WHERE user='{Context.User.Id}'";
            MySQLCommand upcmd = new MySQLCommand(update, myConn);
            upcmd.ExecuteNonQuery();
            myConn.Close();
            await Context.Channel.SendMessageAsync("`Account claimed`");
        }
        else
        {
            await Context.Channel.SendMessageAsync("`Account not claimed`");
        }
    }
}
