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
using System.Net.Sockets;
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

class Program
{
    static void Main()
    {
        DisableConsoleQuickEdit.Go();
        string TokenPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\PixelBot\\Tokens.txt";
        Console.WriteLine(TokenPath);
        using (Stream stream = File.Open(TokenPath, FileMode.Open))
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                DiscordToken = reader.ReadLine();
                MysqlHost = reader.ReadLine();
                MysqlUser = reader.ReadLine();
                MysqlPass = reader.ReadLine();
                TwitchToken = reader.ReadLine();
                TwitchOauth = reader.ReadLine();
                SteamKey = reader.ReadLine();
                OsuKey = reader.ReadLine();
                XboxKey = reader.ReadLine();
                VaingloryKey = reader.ReadLine();
                YoutubeKey = reader.ReadLine();
                DbotsAPI = reader.ReadLine();
            }
        }
        new Program().Run().GetAwaiter().GetResult();
    }
    public static string DiscordToken;
    public static string MysqlHost;
    public static string MysqlUser;
    public static string MysqlPass;
    public static string TwitchToken;
    public static string TwitchOauth;
    public static string SteamKey;
    public static string OsuKey;
    public static string XboxKey;
    public static string VaingloryKey;
    public static string YoutubeKey;
    public static string DbotsAPI;
    private DiscordSocketClient client;
    private CommandService commands;
    private DependencyMap map;
    public async Task Run()
    {
        client = new DiscordSocketClient();
        commands = new CommandService();
        map = new DependencyMap();
        await InstallCommands();

        client.Ready += async () =>
        {
            if (true)
            {
                await client.SetGameAsync("Bot Maintenance");
                return;
            }
            await client.SetGameAsync("p/help | blaze.ml");
            Console.WriteLine("PixelBot > Onling in " + client.Guilds.Count + " Guilds");
            Console.Title = "PixelBot - Online!";
            if (Environment.UserName != "Brandan")
            {
                UpdateBotStats();
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = 60000;
                timer.Elapsed += Timer;
                timer.Start();
            }
        };
        
        client.LeftGuild += (g) =>
        {
            if (false)
            {
                if (Environment.UserName != "Brandan")
                {
                    UpdateBotStats();
                    Console.WriteLine($"Left Guild > {g.Name} - {g.Id}");
                }
            }
            return Task.CompletedTask;
        };
        client.JoinedGuild += async (g) =>
        {
            if (true)
            {
                await g.DefaultChannel.SendMessageAsync("Bot is under Maintenance please wait");
                return;
            }
            if (Environment.UserName != "Brandan")
            {
                UpdateBotStats();
                if (g.Id == 252388688766435328 || g.Id == 282731527161511936)
                {
                    Console.WriteLine($"Removed {g.Name} - {g.Id} due to blacklist");
                    await g.LeaveAsync();
                    return;
                }
                MySQLConnection myConn;
                MySQLDataReader MyReader = null;
                myConn = new MySQLConnection(new MySQLConnectionString(MysqlHost, MysqlUser, MysqlUser, MysqlPass).AsString);
                myConn.Open();
                string stm = $"SELECT guild FROM guilds WHERE guild='{g.Id}'";
                MySQLCommand cmd = new MySQLCommand(stm, myConn);
                MyReader = cmd.ExecuteReaderEx();
                if (!MyReader.HasRows)
                {
                    Console.WriteLine($"New Guild > {g.Name} - {g.Id}");
                    string Command = $"INSERT INTO guilds(guild) VALUES ('{g.Id}')";
                    MySQLCommand cmd2 = new MySQLCommand(Command, myConn);
                    cmd2.ExecuteNonQuery();
                }
                myConn.Close();
            }
            Console.WriteLine($"Joined Guild {g.Name} - {g.Id}");
        };
        client.GuildAvailable += async (g) =>
        {
            if (true)
            {
                return;
            }
            if (Environment.UserName != "Brandan")
            {
                if (g.Id == 252388688766435328 || g.Id == 282731527161511936)
                {
                    Console.WriteLine($"Removed {g.Name} - {g.Id} due to blacklist");
                    await g.LeaveAsync();
                    return;
                }
                MySQLConnection myConn;
                MySQLDataReader MyReader = null;
                myConn = new MySQLConnection(new MySQLConnectionString(MysqlHost, MysqlUser, MysqlUser, MysqlPass).AsString);

                myConn.Open();
                string stm = $"SELECT guild FROM guilds WHERE guild='{g.Id.ToString()}'";
                MySQLCommand cmd = new MySQLCommand(stm, myConn);
                MyReader = cmd.ExecuteReaderEx();
                if (!MyReader.HasRows)
                {
                Console.WriteLine($"New Guild > {g.Name} - {g.Id}");
                    string Command = $"INSERT INTO guilds(guild) VALUES ('{g.Id}')";
                    MySQLCommand cmd2 = new MySQLCommand(Command, myConn);
                    cmd2.ExecuteNonQuery();
                }
                myConn.Close();
            }
        };

        client.UserVoiceStateUpdated += async (u, v, s) =>
        {
            if (true)
            {
                return;
            }
            if (Environment.UserName != "Brandan")
            {
                if (s.VoiceChannel == null)
                {
                    foreach (var Chan in v.VoiceChannel.Guild.VoiceChannels)
                    {
                        if (Chan.Name == $"Temp-{u.Username}")
                        {
                            await Chan.DeleteAsync();
                        }
                    }
                }
            }
        };
        await client.LoginAsync(TokenType.Bot, DiscordToken);
        await client.StartAsync();
        await Task.Delay(-1);
    }
    public void UpdateBotStats()
    {
        var request = (HttpWebRequest)WebRequest.Create("https://bots.discord.pw/api/bots/277933222015401985/stats");
        request.ContentType = "application/json";
        request.Headers.Add("Authorization", DbotsAPI);
        request.Method = "POST";
        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
        {
            string json = "{\"server_count\":\"" + client.Guilds.Count.ToString() + "\"}";

            streamWriter.Write(json);
        }
        var response = (HttpWebResponse)request.GetResponse();
        using (var streamReader = new StreamReader(response.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
        }
    }
    public async Task InstallCommands()
    {
        client.UsePaginator(map);
        client.MessageReceived += HandleCommand;
        await commands.AddModulesAsync(Assembly.GetEntryAssembly());
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
        if (Environment.UserName == "Brandan")
        {
            if (!(message.HasStringPrefix("tp/", ref argPos))) return;
            if (true)
            {
                await message.Channel.SendMessageAsync("Bot is under Maintenance please wait");
                return;
            }
            var context = new CommandContext(client, message);
            var result = await commands.ExecuteAsync(context, argPos, map);
            if (!result.IsSuccess)
            {
                if (message.Content.Contains("vg "))
                {
                    if (result.ErrorReason == "This input does not match any overload.")
                    {
                        await context.Channel.SendMessageAsync("You can only use this command 2 times every half a minute");
                    }
                }
            }
        }
        else
        {
            if (!(message.HasStringPrefix("p/", ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;
            if (true)
            {
                await message.Channel.SendMessageAsync("Bot is under Maintenance please wait");
                return;
            }
            var context = new CommandContext(client, message);
            var result = await commands.ExecuteAsync(context, argPos, map);
            if (!result.IsSuccess)
            {
                if (message.Content.Contains("vg "))
                {
                    if (result.ErrorReason == "This input does not match any overload.")
                    {
                        await context.Channel.SendMessageAsync("You can only use this command 2 times every half a minute");
                    }
                }
            }
        }
    }
    public async Task YTNOTIFY()
    {
        MySQLConnection myConn;
        MySQLDataReader MyReader = null;
        myConn = new MySQLConnection(new MySQLConnectionString(MysqlHost, MysqlUser, MysqlUser, MysqlPass).AsString);
        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = YoutubeKey
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

    private async void Timer(object sender, ElapsedEventArgs e)
    {
        var Client = new TwitchAuthenticatedClient(TwitchToken, TwitchOauth);
        MySQLConnection DB;
        MySQLDataReader MyReader = null;
        DB = new MySQLConnection(new MySQLConnectionString(MysqlHost, MysqlUser, MysqlUser, MysqlPass).AsString);
        DB.Open();
        string RS = $"SELECT type, userid, guild, channel, twitch, live FROM twitch";
        MySQLCommand cmd = new MySQLCommand(RS, DB);
        MyReader = cmd.ExecuteReaderEx();
        while (MyReader.Read())
        {
            if (Client.IsLive(MyReader.GetString(4)) == true)
            {
                if (MyReader.GetString(0) == "channel")
                {
                    if (MyReader.GetString(5) == "no")
                    {
                        try
                        {
                            IGuild Guild = client.GetGuild(Convert.ToUInt64(MyReader.GetString(2)));
                            ITextChannel Channel = await Guild.GetChannelAsync(Convert.ToUInt64(MyReader.GetString(3))) as ITextChannel;
                            var TwitchChannel = Client.GetChannel(MyReader.GetString(4));
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
                            await Channel.SendMessageAsync("", false, embed);
                            string update = $"UPDATE twitch SET live='yes' WHERE type='channel' AND guild='{MyReader.GetString(2)}' AND channel='{MyReader.GetString(3)}' AND twitch='{MyReader.GetString(4)}'";
                            MySQLCommand upcmd = new MySQLCommand(update, DB);
                            upcmd.ExecuteNonQuery();
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
                        try
                        {
                            IGuild Guild = client.GetGuild(Convert.ToUInt64(MyReader.GetString(2)));
                            IUser User = await Guild.GetUserAsync(Convert.ToUInt64(MyReader.GetString(1))) as IUser;
                            var TwitchChannel = Client.GetChannel(MyReader.GetString(4));
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
                            var DM = await User.CreateDMChannelAsync();
                            await DM.SendMessageAsync("", false, embed);
                            string update = $"UPDATE twitch SET live='yes' WHERE type='user' AND userid='{MyReader.GetString(1)}' AND twitch='{MyReader.GetString(4)}'";
                            MySQLCommand upcmd = new MySQLCommand(update, DB);
                            upcmd.ExecuteNonQuery();
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
}
public class Info : ModuleBase
{
    [Command("vainglory")]
    [Alias("vg")]
    public async Task vainglory()
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

    [Command("vg user"), Ratelimit(2, 0.30, Measure.Minutes)]
    [Alias("vg u")]
    public async Task vg(string Region = null, string VGUser = null)
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
            httpWebRequest.Headers.Add("Authorization", Program.VaingloryKey);
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
    public async Task vg2(string Region, [Remainder] string VGUser)
    {
        if (Region == "na" || Region == "eu" || Region == "sa" || Region == "ea" || Region == "sg")
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.dc01.gamelockerapp.com/shards/" + Region + $"/matches?filter[createdAt-start]={Context.Message.Timestamp.Year}-{Context.Message.Timestamp.Month.ToString().PadLeft(2, '0')}-{Context.Message.Timestamp.Day.ToString().PadLeft(2, '0')}T13:25:30Z&filter[playerNames]=" + VGUser);
            httpWebRequest.Method = WebRequestMethods.Http.Get;
            httpWebRequest.Headers.Add("Authorization", Program.VaingloryKey);
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

    [Command("xbox")]
    public async Task xbox()
    {
        HttpWebRequest GetUserId = (HttpWebRequest)WebRequest.Create("http://support.xbox.com/en-US/LiveStatus/GetHeaderStatusModule");
        GetUserId.Method = WebRequestMethods.Http.Get;
        HttpWebResponse response = (HttpWebResponse)GetUserId.GetResponse();
        Stream receiveStream = response.GetResponseStream();
        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
        var Req = readStream.ReadToEnd();
        string Status = "Online";
        if (Req.Contains("Up and Running"))
        {

        }
        else
        {
            Console.WriteLine(Req);
            Status = "Issues!";
        }
        var embed = new EmbedBuilder()
        {
            Title = "Xbox Live | " + Status,
            Description = "```md" + Environment.NewLine + "[ p/xbox (GamerTag) ]( Get xbox live user info )```"
        };
        await Context.Channel.SendMessageAsync("", false, embed);
    }

    [Command("ow")]
    public async Task ow(string User = "")
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
    public async Task xboxuser(string User)
    {
        await Context.Channel.SendMessageAsync("Disabled due to not working");
        return;
        var PWM = await Context.Channel.SendMessageAsync("`Please wait`");
        HttpWebRequest GetUserId = (HttpWebRequest)WebRequest.Create("https://xboxapi.com/v2/xuid/" + User);
        GetUserId.Method = WebRequestMethods.Http.Get;
        GetUserId.Headers.Add("X-AUTH", Program.XboxKey);
        GetUserId.Accept = "application/json";
        HttpWebResponse response = (HttpWebResponse)GetUserId.GetResponse();
        if (response.StatusCode.ToString() != "OK")
        {
            await PWM.DeleteAsync();
            await Context.Channel.SendMessageAsync("Could not find user");
            return;
        }
        Stream receiveStream = response.GetResponseStream();
        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
        var Req = readStream.ReadToEnd();
        var UserID = "";
        Console.WriteLine(Req);
        foreach (var Number in Req.Where(char.IsNumber))
        {
            UserID = UserID + Number;
        }
        string UserOnline = "Offline";
        HttpWebRequest OnlineHttp = (HttpWebRequest)WebRequest.Create("https://xboxapi.com/v2/" + UserID + "/presence");
        OnlineHttp.Method = WebRequestMethods.Http.Get;
        OnlineHttp.Headers.Add("X-AUTH", Program.XboxKey);
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
        HttpUserGamercard.Headers.Add("X-AUTH", Program.XboxKey);
        HttpUserGamercard.Accept = "application/json";
        HttpWebResponse GamercardRes = (HttpWebResponse)HttpUserGamercard.GetResponse();
        Stream GamercardStream = GamercardRes.GetResponseStream();
        StreamReader GamercardRead = new StreamReader(GamercardStream, Encoding.UTF8);
        var GamercardJson = GamercardRead.ReadToEnd();
        dynamic stuff = Newtonsoft.Json.Linq.JObject.Parse(GamercardJson);
        HttpWebRequest HttpUserFrineds = (HttpWebRequest)WebRequest.Create("https://xboxapi.com/v2/" + UserID + "/friends");
        HttpUserFrineds.Method = WebRequestMethods.Http.Get;
        HttpUserFrineds.Headers.Add("X-AUTH", Program.XboxKey);
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
            ThumbnailUrl = stuff.avatarBodyImagePath
        };
        await PWM.DeleteAsync();
        await Context.Channel.SendMessageAsync("", false, embed);
    }

    [Command("guild")]
    public async Task guild(string arg = "guild")
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
        foreach (var emoji in Context.Guild.Emojis)
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
            Color = new Color(0, 0, 0),
            Description = $"Owner: {Owner.Mention}```md" + Environment.NewLine + $"[Online](Offline)" + Environment.NewLine + $"<Users> [{MembersOnline}]({Members}) <Bots> [{BotsOnline}]({Bots})" + Environment.NewLine + $"Channels <Text {TextChan}> <Voice {VoiceChan}>" + Environment.NewLine + $"<Roles {Context.Guild.Roles.Count}> <Region {Context.Guild.VoiceRegionId}>" + Environment.NewLine + "List of roles | p/guild roles```",
            Footer = new EmbedFooterBuilder()
            {
                Text = $"Created {Context.Guild.CreatedAt.Date.Day} {Context.Guild.CreatedAt.Date.DayOfWeek} {Context.Guild.CreatedAt.Year}"
            }
        };
        await Context.Channel.SendMessageAsync("", false, embed);
    }

    [Command("user")]
    public async Task user([Remainder] string User = null)
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
                                char[] arr = Member.Username.ToCharArray();
                                arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))));
                                string str = new string(arr);
                                string LowerUser = str.ToLower();
                                if (LowerUser.Contains(User.ToLower()))
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

        var embed = new EmbedBuilder()
        {
            Author = new EmbedAuthorBuilder()
            {
                Name = $"User Info (Click To Me To Get Avatar Image)",
                Url = GuildUser.GetAvatarUrl()
            },
            ThumbnailUrl = GuildUser.GetAvatarUrl(),
            Color = new Color(0, 0, 0),
            Description = $"<@{GuildUser.Id}>" + Environment.NewLine + "```md" + Environment.NewLine + $"<Discrim {GuildUser.Discriminator}> <ID {GuildUser.Id}>" + Environment.NewLine + $"<Joined_Guild {GuildUser.JoinedAt.Value.Date.ToShortDateString()}>" + Environment.NewLine + $"<Created_Account {GuildUser.CreatedAt.Date.ToShortDateString()}>```",
            Footer = new EmbedFooterBuilder()
            {
                Text = $"Discrim search broken :/"
            }
        };
        await Context.Channel.SendMessageAsync("", false, embed);
    }

    [Command("info")]
    [Alias("bot")]
    public async Task info()
    {
        List<string> Feature = new List<string>();
        MySQLConnection myConn;
        MySQLDataReader MyReader = null;
        myConn = new MySQLConnection(new MySQLConnectionString(Program.MysqlHost, Program.MysqlUser, Program.MysqlUser, Program.MysqlPass).AsString);
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
            Description = "Created by xXBuilderBXx#9113 | Visit the website for a list of commands and info"
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

    [Command("roll")]
    [Alias("dice")]
    public async Task roll()
    {
        var random = new Random((int)DateTime.Now.Ticks); var randomValue = random.Next(1, 7);
        await Context.Channel.SendMessageAsync($"{Context.User.Username} Rolled a {randomValue}");
    }

    [Command("invite")]
    public async Task invite()
    {
        var embed = new EmbedBuilder()
        {
            Title = "",
            Description = "[Invite this bot to your guild](https://discordapp.com/oauth2/authorize?&client_id=277933222015401985&scope=bot&permissions=0)"
        };
        await Context.Channel.SendMessageAsync("", false, embed);
    }

    [Command("flip")]
    [Alias("coin")]
    public async Task flip()
    { var random = new Random((int)DateTime.Now.Ticks); var randomValue = random.Next(1, 3);
        if (randomValue == 1)
        { await Context.Channel.SendMessageAsync($"{Context.User.Username} Flipped Heads"); }
        else
        { await Context.Channel.SendMessageAsync($"{Context.User.Username} Flipped Tails"); }
    }

    [Command("prune all")]
    public async Task pruneall(string arg = "")
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
            await Context.Channel.SendMessageAsync("`You do not have permission to manage messages");
            return;
        }
        int Ammount = 0;
        foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
        {
            if (arg.Contains("-p"))
            {
                Ammount++;
                await Item.DeleteAsync();
            }
            else
            {
                if (!Item.IsPinned)
                {
                    Ammount++;
                    await Item.DeleteAsync();
                }
            }
        }
        await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Ammount} messages`");
    }

    [Command("prune user")]
    public async Task pruneuser(IUser User = null)
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
        int Ammount = 0;
        foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
        {
            if (Item.Author.Id == User.Id)
            {
                Ammount++;
                await Item.DeleteAsync();
            }
        }
        await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Ammount} user messages`");
    }

    [Command("prune bots")]
    [Alias("prune bot")]
    public async Task prunebot()
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
        int Ammount = 0;
        foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
        {
            if (Item.Author.IsBot)
            {
                Ammount++;
                await Item.DeleteAsync();
            }
        }
        await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Ammount} bot messages`");
    }

    [Command("prune images")]
    [Alias("prune image")]
    public async Task pruneimage()
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
        int Ammount = 0;
        foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
        {
            if (Item.Attachments.Any())
            {
                Ammount++;
                await Item.DeleteAsync();
            }
        }
        await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Ammount} images`");
    }

    [Command("prune embeds")]
    [Alias("prune embed")]
    public async Task pruneembed()
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
        int Ammount = 0;
        foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
        {
            if (Item.Embeds.Any())
            {
                Ammount++;
                await Item.DeleteAsync();
            }
        }
        await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Ammount} embeds`");
    }

    [Command("prune links")]
    [Alias("prune command", "tidy command", "tidy commands", "purge command", "purge commands", "clean command", "clean commands")]
    public async Task prunelinks()
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
        int Ammount = 0;
        foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
        {
            if (Item.Content.Contains("http://") || Item.Content.Contains("https://"))
            {
                Ammount++;
                await Item.DeleteAsync();
            }
        }
        await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Ammount} links`");
    }

    [Command("prune commands")]
    [Alias("prune command", "tidy command", "tidy commands", "purge command", "purge commands", "clean command", "clean commands")]
    public async Task prunecommands()
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
        int Ammount = 0;
        foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
        {
            if (Item.Content.StartsWith("p/") || Item.Content.StartsWith("/") || Item.Content.StartsWith("!") || Item.Content.StartsWith(",") || Item.Content.StartsWith("=") || Item.Content.StartsWith("%") || Item.Content.StartsWith("b!"))
            {
                Ammount++;
                await Item.DeleteAsync();
            }
        }
        await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Ammount} commands`");
    }

    [Command("prune text")]
    public async Task prunetext([Remainder] string Text = null)
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
        int Ammount = 0;
        foreach (var Item in await Context.Channel.GetMessagesAsync(100).Flatten())
        {
            if (Item.Content.Contains(Text))
            {
                Ammount++;
                await Item.DeleteAsync();
            }
        }
        await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Ammount} messages`");
    }

    [Command("mc")]
    public async Task mc()
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

    [Command("mc ping")]
    public async Task mcping(string IP = "null")
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
            MineStat ms = new MineStat(IP, 25565);
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

    [Command("mc bping")]
    public async Task mcbping(string IP = "null")
    {
        await Context.Channel.SendMessageAsync("`Feature under construction`");
    }

    [Command("mc skin")]
    public async Task mcskin(string Arg = null, [Remainder] string User = null)
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

    [Command("yt")]
    public async Task yt()
    {
        await Context.Channel.SendMessageAsync("`Coming Soon`");
    }
    [Command("yt user")]
    public async Task ytuser([Remainder] string User = "null")
    {
        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = Program.YoutubeKey
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
    public async Task ytnotify()
    {
        await Context.Channel.SendMessageAsync("```fix" + Environment.NewLine + "Feature under construction```");
        return;
        await Context.Channel.SendMessageAsync("**/yt notify add (Channel ID)** - Add a  youtube streamer to your notification settings" + Environment.NewLine + "**/yt notify list** - List all of your youtube notification settings" + Environment.NewLine + "**/yt notify remove (Channel ID)** - Remove a youtube streamer from your notification settings");
    }
    [Command("yt notify add")]
    public async Task ytadd([Remainder] string User = "null")
    {
        await Context.Channel.SendMessageAsync("```fix" + Environment.NewLine + "Feature under construction```");

        MySQLConnection myConn;
        MySQLDataReader MyReader = null;
        myConn = new MySQLConnection(new MySQLConnectionString(Program.MysqlHost, Program.MysqlUser, Program.MysqlUser, Program.MysqlPass).AsString);
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
    public async Task ytlist()
    {
        List<string> YTList = new List<string>();
        await Context.Channel.SendMessageAsync("```fix" + Environment.NewLine + "Feature under construction```");

        MySQLConnection myConn;
        MySQLDataReader MyReader = null;
        myConn = new MySQLConnection(new MySQLConnectionString(Program.MysqlHost, Program.MysqlUser, Program.MysqlUser, Program.MysqlPass).AsString);
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
    public async Task ytdel([Remainder] string User = "null")
    {
        await Context.Channel.SendMessageAsync("```fix" + Environment.NewLine + "Feature under construction```");

        MySQLConnection myConn;
        MySQLDataReader MyReader = null;
        myConn = new MySQLConnection(new MySQLConnectionString(Program.MysqlHost, Program.MysqlUser, Program.MysqlUser, Program.MysqlPass).AsString);
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
    public async Task profile([Remainder] string User = null)
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
        myConn = new MySQLConnection(new MySQLConnectionString(Program.MysqlHost, Program.MysqlUser, Program.MysqlUser, Program.MysqlPass).AsString);
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

    [Command("temp create")]
    public async Task tempcreate()
    {
        IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
        if (!Bot.GuildPermissions.Connect || !Bot.GuildPermissions.ManageChannels || !Bot.GuildPermissions.ManageRoles)
        {
            string Connect = "Voice Connect :x: :white_check_mark: ";
            string Manage = "Manage Channels :x:";
            string Roles = "Manage Roles :x:";
            if (Bot.GuildPermissions.Connect)
            {
                Connect = "Voice Connect :white_check_mark:";
            }
            if (Bot.GuildPermissions.ManageChannels)
            {
                Connect = "Manage Channels :white_check_mark:";
            }
            if (Bot.GuildPermissions.ManageRoles)
            {
                Connect = "Manage Roles :white_check_mark:";
            }
            await Context.Channel.SendMessageAsync("Bot is missing permissions" + Environment.NewLine + Connect + Environment.NewLine + Manage + Environment.NewLine + Roles);
            return;
        }
        IGuildUser User = await Context.Guild.GetUserAsync(Context.User.Id);
        if (User.VoiceChannel == null)
        {
            await Context.Channel.SendMessageAsync("`You need to be in a voice channel to use this feature`");
            return;
        }
        var DenyConnect = new OverwritePermissions(0, 36700160);
        var BotPerm = new OverwritePermissions(269484048, 0);
        var AllowConnect = new OverwritePermissions(36700160, 0);
        IVoiceChannel MyChan = null;
        foreach (var Chan in await Context.Guild.GetVoiceChannelsAsync())
        {
            if (Chan.Name == $"Temp-{Context.User.Username}")
            {
                MyChan = Chan;
            }
        }
        if (MyChan == null)
        {
            IVoiceChannel Chan = await Context.Guild.CreateVoiceChannelAsync($"Temp-{Context.User.Username}");
            await Chan.AddPermissionOverwriteAsync(Bot, BotPerm);
            await Chan.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, DenyConnect);
            await Chan.AddPermissionOverwriteAsync(Context.User, AllowConnect);
            await Context.Channel.SendMessageAsync("You have created a temp voice channel | It will be deleted when you disconnect from the voice service (Switching to other voice channels is fine)");
        }
        else
        {
            await Context.Channel.SendMessageAsync("`You already have a temp channel`");
        }
    }

    [Command("temp invite")]
    public async Task tempinvite(IUser User = null)
    {
        IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
        if (!Bot.GuildPermissions.Connect || !Bot.GuildPermissions.ManageChannels || !Bot.GuildPermissions.ManageRoles)
        {
            string Connect = "Voice Connect :x: :white_check_mark: ";
            string Manage = "Manage Channels :x:";
            string Roles = "Manage Roles :x:";
            if (Bot.GuildPermissions.Connect)
            {
                Connect = "Voice Connect :white_check_mark:";
            }
            if (Bot.GuildPermissions.ManageChannels)
            {
                Connect = "Manage Channels :white_check_mark:";
            }
            if (Bot.GuildPermissions.ManageRoles)
            {
                Connect = "Manage Roles :white_check_mark:";
            }
            await Context.Channel.SendMessageAsync("Bot is missing permissions" + Environment.NewLine + Connect + Environment.NewLine + Manage + Environment.NewLine + Roles);
            return;
        }
        IGuildUser GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
        if (GuildUser.VoiceChannel == null)
        {
            await Context.Channel.SendMessageAsync("`You need to be in a voice channel to use this feature`");
            return;
        }
        var AllowConnect = new OverwritePermissions(36700160, 0);
        IVoiceChannel MyChan = null;
        foreach (var Chan in await Context.Guild.GetVoiceChannelsAsync())
        {
            if (Chan.Name == $"Temp-{Context.User.Username}")
            {
                MyChan = Chan;
            }
        }
        if (MyChan == null)
        {
            await Context.Channel.SendMessageAsync("Could not find your temp channel");
        }
        else
        {
            await MyChan.AddPermissionOverwriteAsync(User, AllowConnect);
            await Context.Channel.SendMessageAsync($"User {User.Username} is now allowed to join your temp channel");
        }
    }

    [Command("temp kick")]
    public async Task tempkick(IUser User = null)
    {
        IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
        if (!Bot.GuildPermissions.Connect || !Bot.GuildPermissions.ManageChannels || !Bot.GuildPermissions.ManageRoles)
        {
            string Connect = "Voice Connect :x: :white_check_mark: ";
            string Manage = "Manage Channels :x:";
            string Roles = "Manage Roles :x:";
            if (Bot.GuildPermissions.Connect)
            {
                Connect = "Voice Connect :white_check_mark:";
            }
            if (Bot.GuildPermissions.ManageChannels)
            {
                Connect = "Manage Channels :white_check_mark:";
            }
            if (Bot.GuildPermissions.ManageRoles)
            {
                Connect = "Manage Roles :white_check_mark:";
            }
            await Context.Channel.SendMessageAsync("Bot is missing permissions" + Environment.NewLine + Connect + Environment.NewLine + Manage + Environment.NewLine + Roles);
            return;
        }
        IGuildUser GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
        if (GuildUser.VoiceChannel == null)
        {
            await Context.Channel.SendMessageAsync("`You need to be in a voice channel to use this feature`");
            return;
        }
        IVoiceChannel MyChan = null;
        foreach (var Chan in await Context.Guild.GetVoiceChannelsAsync())
        {
            if (Chan.Name == $"Temp-{Context.User.Username}")
            {
                MyChan = Chan;
            }
        }
        if (MyChan == null)
        {
            await Context.Channel.SendMessageAsync("Could not find your temp channel");
        }
        else
        {
            await MyChan.RemovePermissionOverwriteAsync(User);
            await Context.Channel.SendMessageAsync($"User {User.Username} has been removed from your temp channel");
        }
    }

    [Command("temp toggle")]
    public async Task temptoggle()
    {
        IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
        if (!Bot.GuildPermissions.Connect || !Bot.GuildPermissions.ManageChannels || !Bot.GuildPermissions.ManageRoles)
        {
            string Connect = "Voice Connect :x: :white_check_mark: ";
            string Manage = "Manage Channels :x:";
            string Roles = "Manage Roles :x:";
            if (Bot.GuildPermissions.Connect)
            {
                Connect = "Voice Connect :white_check_mark:";
            }
            if (Bot.GuildPermissions.ManageChannels)
            {
                Connect = "Manage Channels :white_check_mark:";
            }
            if (Bot.GuildPermissions.ManageRoles)
            {
                Connect = "Manage Roles :white_check_mark:";
            }
            await Context.Channel.SendMessageAsync("Bot is missing permissions" + Environment.NewLine + Connect + Environment.NewLine + Manage + Environment.NewLine + Roles);
            return;
        }
        IGuildUser GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
        if (GuildUser.VoiceChannel == null)
        {
            await Context.Channel.SendMessageAsync("`You need to be in a voice channel to use this feature`");
            return;
        }
        IVoiceChannel MyChan = null;
        foreach (var Chan in await Context.Guild.GetVoiceChannelsAsync())
        {
            if (Chan.Name == $"Temp-{Context.User.Username}")
            {
                MyChan = Chan;
            }
        }
        if (MyChan == null)
        {
            await Context.Channel.SendMessageAsync("Could not find your temp channel");
        }
        else
        {
            var AllowConnect = new OverwritePermissions(36700160, 0);
            var DenyConnect = new OverwritePermissions(0, 36700160);
            if (MyChan.GetPermissionOverwrite(Context.Guild.EveryoneRole).Equals(AllowConnect))
            {
                await MyChan.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, DenyConnect);
                await Context.Channel.SendMessageAsync($"Everyone role disabled for temp channel");
            }
            else
            {
                await MyChan.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, AllowConnect);
                await Context.Channel.SendMessageAsync($"Everyone role enabled for temp channel");
            }

        }
    }

    [Command("steam game")]
    [Alias("steam g")]
    public async Task steamg([Remainder] string Game = null)
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

    [Command("steam")]
    public async Task steam()
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

    [Command("steam user")]
    [Alias("steam u")]
    public async Task steamu([Remainder] string User = null)
    {
        if (User == null)
        {
            await Context.Channel.SendMessageAsync("p/steam u (User)" + Environment.NewLine + "How to get a steam user? <" + "http://i.imgur.com/pM9dff5.png" + ">");
            return;
        }
        string Claim = "";
        MySQLConnection DB;
        DB = new MySQLConnection(new MySQLConnectionString(Program.MysqlHost, Program.MysqlUser, Program.MysqlUser, Program.MysqlPass).AsString);
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
        SteamWebAPI.SetGlobalKey(Program.SteamKey);
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
    
    [Command("osu")]
    public async Task osu(string User = null)
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
            HttpWebRequest GetUserId = (HttpWebRequest)WebRequest.Create("https://osu.ppy.sh/api/get_user?k=" + Program.OsuKey + "&u=" + User);
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

    [Command("tw search")]
    [Alias("tw s")]
    public async Task tsearch(string Search = null)
    {
        if (Search == null)
        {
            await Context.Channel.SendMessageAsync("`Enter a channel name to search | p/tw s (User)`");
            return;
        }
        var client = new TwitchAuthenticatedClient(Program.TwitchToken, Program.TwitchOauth);
        var Usearch = client.SearchChannels(Search).List;
        var embed = new EmbedBuilder()
        {
            Title = "Channels",
            Description = $"{Usearch[0].Name} | {Usearch[1].Name} | {Usearch[2].Name}"
        };
        await Context.Channel.SendMessageAsync("", false, embed);
    }

    [Command("tw")]
    public async Task twitchchannel(string Channel = null)
    {
        if (Channel == null)
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
                }
            };
            await Context.Channel.SendMessageAsync("", false, infoembed);
            return;
        }
        var client = new TwitchAuthenticatedClient(Program.TwitchToken, Program.TwitchOauth);
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
            Color = new Color(255,0,0),
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
    public async Task twn(string Option = null, string Channel = null)
    {
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
        var client = new TwitchAuthenticatedClient(Program.TwitchToken, Program.TwitchOauth);
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
            myConn = new MySQLConnection(new MySQLConnectionString(Program.MysqlHost, Program.MysqlUser, Program.MysqlUser, Program.MysqlPass).AsString);
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
            myConn = new MySQLConnection(new MySQLConnectionString(Program.MysqlHost, Program.MysqlUser, Program.MysqlUser, Program.MysqlPass).AsString);
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
    public async Task twl(string Option = null)
    {
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
            myConn = new MySQLConnection(new MySQLConnectionString(Program.MysqlHost, Program.MysqlUser, Program.MysqlUser, Program.MysqlPass).AsString);
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
            myConn = new MySQLConnection(new MySQLConnectionString(Program.MysqlHost, Program.MysqlUser, Program.MysqlUser, Program.MysqlPass).AsString);
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
            myConn = new MySQLConnection(new MySQLConnectionString(Program.MysqlHost, Program.MysqlUser, Program.MysqlUser, Program.MysqlPass).AsString);
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
    public async Task twr(string Option = null, string Channel = null)
    {
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
            myConn = new MySQLConnection(new MySQLConnectionString(Program.MysqlHost, Program.MysqlUser, Program.MysqlUser, Program.MysqlPass).AsString);
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
            myConn = new MySQLConnection(new MySQLConnectionString(Program.MysqlHost, Program.MysqlUser, Program.MysqlUser, Program.MysqlPass).AsString);
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

    [Command("math")]
    [Alias("calc")]
    public async Task math([Remainder] string Math)
    {
        var interpreter = new DynamicExpresso.Interpreter();
        var result = interpreter.Eval(Math);
        await Context.Channel.SendMessageAsync(result.ToString());
    }
}

public class Help : ModuleBase
{
    private readonly PaginationService paginator;
    public Help(PaginationService PagService)
    {
        paginator = PagService;
    }
    string MiscText = "[ p/bot ]( Bot information such as new features, website, invite link )" + Environment.NewLine + "[ p/guild ]( Get info about the guild such as the owner )" + Environment.NewLine + "[ p/user (User) ]( Get info about a user | Use names or mentions )" + Environment.NewLine + "[ p/roll ]( Roll a dice from 1 to 6 )" + Environment.NewLine + "[ p/flip ]( Flip a coin and land heads or tails )" + Environment.NewLine + "[ p/math (Math) ]( Solve some math )```";
    string PruneText = "[ p/prune all ]( Prune all messages )" + Environment.NewLine + "[ p/prune user (@User) ]( Prune a users messages )" + Environment.NewLine + "[ p/prune bot ]( Prune bot messages )" + Environment.NewLine + "[ p/prune image ]( Prune attachments )" + Environment.NewLine + "[ p/prune embed ]( Prune embeds )" + Environment.NewLine + "[ p/prune link ]( Prune links )" + Environment.NewLine + "[ p/prune commands ]( Prune messages that start with p/ m/ / ! = % )" + Environment.NewLine + "[ p/prune text (Text) ](Find and prune messages that contain the TEXT )```";
    string GameText = "[ p/steam ]( Steam user info and game info )" + Environment.NewLine + "[ p/ow (User#Tag) ]( Overwatch stats )" + Environment.NewLine + "[ p/mc ]( Minecraft user skins and server ping/status )" + Environment.NewLine + "[ p/vg ]( Vainglory game info, user info and matches )" + Environment.NewLine + "[ p/osu ]( Osu! game info and user info )" + Environment.NewLine + "[ p/xbox ]( Xbox live status and user info )```";
    string TempVoiceText = "[ p/temp create ]( Create a temp voice channel )" + Environment.NewLine + "[ p/temp invite (User) ]( Invite a user to your channel )" + Environment.NewLine + "[ p/temp kick (User) ]( Kick a user from your channel )" + Environment.NewLine + "[ p/temp toggle ]( Toggle so anyone can join your channel )```";
    string MediaText = "[ p/tw (Channel) ]( Get info about a channel )" + Environment.NewLine + "[ p/tw s (Channel ]( Get 3 channel names )" + Environment.NewLine + "[ p/tw n (Option) (Channel) ]( Get a notification when a streamer goes live )" + Environment.NewLine + "[ p/tw l (Option) ]( Get a list of notification settings )" + Environment.NewLine + "[ p/tw r (Option) (Channel) ]( Remove a channel from notification setting )```";

    [Command("help")]
    [Alias("commands")]
    public async Task pag(string Option = "")
    {
        var BotUser = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id) as IGuildUser;
        if (Option == "all")
        {
            await Context.Channel.SendMessageAsync($"{Context.User.Username} I have sent you you a full list of commands");
            var allemebed = new EmbedBuilder()
            {
                Title = "Commands List"
            };
            allemebed.AddField(x =>
            {
                x.Name = "Misc"; x.Value = "```md" + Environment.NewLine + MiscText;
            });
            allemebed.AddField(x =>
            {
                x.Name = "Game"; x.Value = "```md" + Environment.NewLine + GameText;
            });
            allemebed.AddField(x =>
            {
                x.Name = "Media"; x.Value = "```md" + Environment.NewLine + MediaText;
            });
            allemebed.AddField(x =>
            {
                x.Name = "Prune"; x.Value = "```md" + Environment.NewLine + PruneText;
            });
            allemebed.Color = new Color(0, 191, 255);
            var DM = await Context.User.CreateDMChannelAsync();
            await DM.SendMessageAsync("", false, allemebed);
            return;
        }
        string HelpText = "```md" + Environment.NewLine + "[ p/misc ]( Guild/User Info | Dice Roll | Coin Flip )" + Environment.NewLine + "[ p/prune ]( Prune Messages | Embeds | Links | Text Match )" + Environment.NewLine + "[ p/game ]( Steam | Osu! | Minecraft )" + Environment.NewLine + "[ p/media ]( Twitch Commands )" + Environment.NewLine + "[ p/temp ]( Create A Temp Voice Channel )```";
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
                Color = new Color(0, 191, 255)
            };
            await Context.Channel.SendMessageAsync("", false, embed);
            return;
        }
        var Guilds = await Context.Client.GetGuildsAsync();
        var pages = new List<string>
            {
                "```md" + Environment.NewLine + "< Info     | Commands ► >" + Environment.NewLine + "Language C# | Library .net 1.0" + Environment.NewLine + $"Guilds {Guilds.Count}``` For a full list of commands do **p/help all** or visit the website" + Environment.NewLine + "[Website](https://blaze.ml) | [Invite Bot](https://discordapp.com/oauth2/authorize?&client_id=277933222015401985&scope=bot&permissions=0) | [Github](https://github.com/ArchboxDev/PixelBot) | [My Guild](http://discord.gg/WJTYdNb)",
                "```md" + Environment.NewLine + "< ◄ Info |     Misc     | Games ► >" + Environment.NewLine + MiscText,
                "```md" + Environment.NewLine + "< ◄ Misc |     Games     | Media ► >" + Environment.NewLine + GameText,
                "```md" + Environment.NewLine + "< ◄ Games |     Media     | Prune ► >" + Environment.NewLine + MediaText,
            "```md" + Environment.NewLine + "<  Games |     Prune >" + Environment.NewLine + PruneText
        };
        var message = new PaginatedMessage(pages, "Commands List", new Color(0, 191, 255), Context.User);
        await paginator.SendPaginatedMessageAsync(Context.Channel, message);
    }

    [Command("misc")]
    public async Task misc()
    {
        await Context.Channel.SendMessageAsync("Misc Commands ```md" + Environment.NewLine + MiscText);
    }

    [Command("game")]
    [Alias("games")]
    public async Task game()
    {
        await Context.Channel.SendMessageAsync("Game Commands ```md" + Environment.NewLine + GameText);
    }

    [Command("temp")]
    public async Task temp()
    {
        await Context.Channel.SendMessageAsync("TempVoice Commands ```md" + Environment.NewLine + TempVoiceText);
    }

    [Command("media")]
    public async Task media()
    {
        await Context.Channel.SendMessageAsync("Media Commands ```md" + Environment.NewLine + MediaText);
    }

    [Command("prune")]
    [Alias("purge, clean, tidy")]
    public async Task prune()
    {
        var BotUser = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
        if (!BotUser.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
        {
            await Context.Channel.SendMessageAsync("`Bot does not have access to manage messages`");
            return;
        }
        await Context.Channel.SendMessageAsync("Prune Commands ```md" + Environment.NewLine + PruneText);
    }
}

public class Steam : InteractiveModuleBase
{
    [Command("steam claim", RunMode = RunMode.Async)]
    public async Task steamclaim([Remainder] string User = null)
    {
        if (User == null)
        {
            await Context.Channel.SendMessageAsync("`No user set | p/steam claim (User)");
            return;
        }
        MySQLConnection myConn;
        MySQLDataReader MyReader = null;
        myConn = new MySQLConnection(new MySQLConnectionString(Program.MysqlHost, Program.MysqlUser, Program.MysqlUser, Program.MysqlPass).AsString);
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
        SteamWebAPI.SetGlobalKey(Program.SteamKey);
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

public class MineStat
{
    const ushort dataSize = 512; // this will hopefully suffice since the MotD should be <=59 characters
    const ushort numFields = 6;  // number of values expected from server

    public string Address { get; set; }
    public ushort Port { get; set; }
    public string Motd { get; set; }
    public string Version { get; set; }
    public string CurrentPlayers { get; set; }
    public string MaximumPlayers { get; set; }
    public bool ServerUp { get; set; }

    public MineStat(string address, ushort port)
    {
        var rawServerData = new byte[dataSize];

        Address = address;
        Port = port;

        try
        {
            // ToDo: Add timeout
            var tcpclient = new TcpClient();
            tcpclient.Connect(address, port);
            var stream = tcpclient.GetStream();
            var payload = new byte[] { 0xFE, 0x01 };
            stream.Write(payload, 0, payload.Length);
            stream.Read(rawServerData, 0, dataSize);
            tcpclient.Close();
        }
        catch (Exception)
        {
            ServerUp = false;
            return;
        }

        if (rawServerData == null || rawServerData.Length == 0)
        {
            ServerUp = false;
        }
        else
        {
            var serverData = Encoding.Unicode.GetString(rawServerData).Split("\u0000\u0000\u0000".ToCharArray());
            if (serverData != null && serverData.Length >= numFields)
            {
                ServerUp = true;
                Version = serverData[2];
                Motd = serverData[3];
                CurrentPlayers = serverData[4];
                MaximumPlayers = serverData[5];
            }
            else
            {
                ServerUp = false;
            }
        }
    }

    #region Obsolete

    [Obsolete]
    public string GetAddress()
    {
        return Address;
    }

    [Obsolete]
    public void SetAddress(string address)
    {
        Address = address;
    }

    [Obsolete]
    public ushort GetPort()
    {
        return Port;
    }

    [Obsolete]
    public void SetPort(ushort port)
    {
        Port = port;
    }

    [Obsolete]
    public string GetMotd()
    {
        return Motd;
    }

    [Obsolete]
    public void SetMotd(string motd)
    {
        Motd = motd;
    }

    [Obsolete]
    public string GetVersion()
    {
        return Version;
    }

    [Obsolete]
    public void SetVersion(string version)
    {
        Version = version;
    }

    [Obsolete]
    public string GetCurrentPlayers()
    {
        return CurrentPlayers;
    }

    [Obsolete]
    public void SetCurrentPlayers(string currentPlayers)
    {
        CurrentPlayers = currentPlayers;
    }

    [Obsolete]
    public string GetMaximumPlayers()
    {
        return MaximumPlayers;
    }

    [Obsolete]
    public void SetMaximumPlayers(string maximumPlayers)
    {
        MaximumPlayers = maximumPlayers;
    }

    [Obsolete]
    public bool IsServerUp()
    {
        return ServerUp;
    }

    #endregion
}




