using Discord;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MySQLDriverCS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Timers;
using TwitchCSharp.Clients;

public class Services
{
    public static Timer _Timer_Twitch = new Timer();
    public static Timer _Timer_Stats = new Timer();
    public static Timer _Timer_Uptime = new Timer();
    public static Dictionary<ulong, int> UptimeBotsList = new Dictionary<ulong, int>();

    public static void BotGuildCount(object sender, ElapsedEventArgs e)
    {
        try
        {
            var request = (HttpWebRequest)WebRequest.Create("https://bots.discord.pw/api/bots/277933222015401985/stats");
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", Program.TokenMap.Dbots);
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = "{\"server_count\":\"" + Program._client.Guilds.Count.ToString() + "\"}";

                streamWriter.Write(json);
            }
            request.GetResponse();
        }
        catch
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
                string json = "{\"server_count\":\"" + Program._client.Guilds.Count.ToString() + "\"}";

                streamWriter.Write(json);
            }
            request.GetResponse();
        }
        catch
        {
            Console.WriteLine("Error could not update DbotsV2 Stats");
        }
    }
    public static void TwitchNotification(object sender, ElapsedEventArgs e)
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
                            IGuild Guild = Program._client.GetGuild(Convert.ToUInt64(MyReader.GetString(2)));
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
    public static async void Uptime(object sender, ElapsedEventArgs e)
    {
        try
        {
            List<IGuildUser> BotsList = new List<IGuildUser>();
            var Dbots = Program._client.GetGuild(110373943822540800);
            var DbotsV2 = Program._client.GetGuild(264445053596991498);
            await Dbots.DownloadUsersAsync();
            await DbotsV2.DownloadUsersAsync();

            BotsList.AddRange(Dbots.Users.Where(x => x.IsBot));
            BotsList.AddRange(DbotsV2.Users.Where(x => x.IsBot));

            foreach (var Bot in BotsList)
            {
                if (!File.Exists(Program.BotPath + "Uptime\\" + Bot.Id.ToString() + ".txt"))
                {
                    File.WriteAllText(Program.BotPath + "Uptime\\" + Bot.Id.ToString() + ".txt", "75");
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
                File.WriteAllText(Program.BotPath + "Uptime\\" + Bot.Id.ToString() + ".txt", UptimeCount.ToString());
                UptimeBotsList[Bot.Id] = UptimeCount;
                BotsList.RemoveAll(x => x.Id == Bot.Id);
            }
        }
        catch
        {
            Console.WriteLine("[Error] Error in bot uptime service");
        }
    }
    public static void YTNOTIFY()
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
}