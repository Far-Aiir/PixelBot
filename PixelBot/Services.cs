using Discord;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Timers;
using TwitchCSharp.Clients;

public class MyServices
{
    public class TwitchNotifyClass
    {
        public string Type { get; set; }
        public ulong User { get; set; }
        public ulong Guild { get; set; }
        public ulong Channel { get; set; }
        public string Twitch { get; set; }
        public bool Live { get; set; }
    }
    public static Timer _Timer_Twitch = new Timer();
    public static Timer _Timer_Stats = new Timer();
    public static Timer _Timer_Uptime = new Timer();
    public static Dictionary<ulong, int> UptimeBotsList = new Dictionary<ulong, int>();
    public static List<TwitchNotifyClass> TwitchNotifications = new List<TwitchNotifyClass>();
    public static void BotGuildCount(object sender, ElapsedEventArgs e)
    {
        try
        {
            var request = (HttpWebRequest)WebRequest.Create("https://bots.discord.pw/api/bots/277933222015401985/stats");
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", PixelBot.PixelBot.Tokens.Dbots);
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = "{\"server_count\":\"" + PixelBot.PixelBot._client.Guilds.Count.ToString() + "\"}";

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
            request.Headers.Add("Authorization", PixelBot.PixelBot.Tokens.DbotsV2);
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = "{\"server_count\":\"" + PixelBot.PixelBot._client.Guilds.Count.ToString() + "\"}";

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
        var TwitchClient = new TwitchAuthenticatedClient(PixelBot.PixelBot.Tokens.Twitch, PixelBot.PixelBot.Tokens.TwitchAuth);
        foreach (var Item in TwitchNotifications)
        {
            try
            {
                if (TwitchClient.IsLive(Item.Twitch))
                {
                    if (Item.Live == false)
                    {
                        Item.Live = true;
                        if (Item.Type == "user")
                        {
                            IGuild Guild = PixelBot.PixelBot._client.GetGuild(Item.Guild);
                            IUser User = Guild.GetUserAsync(Item.User).GetAwaiter().GetResult() as IUser;
                            var TwitchChannel = TwitchClient.GetChannel(Item.Twitch);
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
                            JsonSerializer serializer = new JsonSerializer();
                            using (StreamWriter file = File.CreateText(PixelBot.PixelBot.BotPath + $"Twitch\\user-{Item.User}-{Item.Twitch.ToLower()}.json"))
                            {
                                serializer.Serialize(file, Item);
                            }
                        }
                        else
                        {
                            IGuild Guild = PixelBot.PixelBot._client.GetGuild(Item.Guild);
                            ITextChannel Channel = Guild.GetChannelAsync(Item.Channel).GetAwaiter().GetResult() as ITextChannel;
                            var TwitchChannel = TwitchClient.GetChannel(Item.Twitch);
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
                            JsonSerializer serializer = new JsonSerializer();
                            using (StreamWriter file = File.CreateText(PixelBot.PixelBot.BotPath + $"Twitch\\channel-{Item.Guild.ToString()}-{Item.Channel}-{Item.Twitch}.json"))
                            {
                                serializer.Serialize(file, Item);
                            }
                        }
                    }
                }
                else
                {
                    if (Item.Live == true)
                    {
                        Item.Live = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Twitch Service] Error for {Item.Type}-{Item.Guild}-{Item.Channel}-{Item.User}-{Item.Twitch}");
                Console.WriteLine(ex);
            }
        }
    }
    public static async void Uptime(object sender, ElapsedEventArgs e)
    {
        try
        {
            List<IGuildUser> BotsList = new List<IGuildUser>();
            var Dbots = PixelBot.PixelBot._client.GetGuild(110373943822540800);
            var DbotsV2 = PixelBot.PixelBot._client.GetGuild(264445053596991498);
            await Dbots.DownloadUsersAsync();
            await DbotsV2.DownloadUsersAsync();

            BotsList.AddRange(Dbots.Users.Where(x => x.IsBot));
            BotsList.AddRange(DbotsV2.Users.Where(x => x.IsBot));

            foreach (var Bot in BotsList)
            {
                if (!File.Exists(PixelBot.PixelBot.BotPath + "Uptime\\" + Bot.Id.ToString() + ".txt"))
                {
                    File.WriteAllText(PixelBot.PixelBot.BotPath + "Uptime\\" + Bot.Id.ToString() + ".txt", "75");
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
                File.WriteAllText(PixelBot.PixelBot.BotPath + "Uptime\\" + Bot.Id.ToString() + ".txt", UptimeCount.ToString());
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
        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = PixelBot.PixelBot.Tokens.Youtube
        });
    }
}