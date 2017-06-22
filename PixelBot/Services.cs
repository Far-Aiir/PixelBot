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
namespace PixelBot.Services
{
    public class Twitch
    {
        public class TwitchClass
        {
            public string Type { get; set; }
            public ulong User { get; set; }
            public ulong Guild { get; set; }
            public ulong Channel { get; set; }
            public string Twitch { get; set; }
            public bool Live { get; set; }
        }
        public static Timer Timer = new Timer();
        
        public static List<TwitchClass> NotificationList = new List<TwitchClass>();

        public static void SendNotifications(object sender, ElapsedEventArgs e)
        {
            var TwitchClient = new TwitchAuthenticatedClient(PixelBot.Tokens.Twitch, PixelBot.Tokens.TwitchAuth);
            foreach (var Item in NotificationList)
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
                                IGuild Guild = PixelBot._client.GetGuild(Item.Guild);
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
                                using (StreamWriter file = File.CreateText(PixelBot.BotPath + $"Twitch\\user-{Item.User}-{Item.Twitch.ToLower()}.json"))
                                {
                                    serializer.Serialize(file, Item);
                                }
                            }
                            else
                            {
                                IGuild Guild = PixelBot._client.GetGuild(Item.Guild);
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
                                using (StreamWriter file = File.CreateText(PixelBot.BotPath + $"Twitch\\channel-{Item.Guild.ToString()}-{Item.Channel}-{Item.Twitch}.json"))
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
    }
    public class Stats
    {
        public static Timer Timer = new Timer();
        public static void PostStats(object sender, ElapsedEventArgs e)
        {
            string json = "{\"server_count\":\"" + PixelBot._client.Guilds.Count.ToString() + "\"}";
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://bots.discord.pw/api/bots/277933222015401985/stats");
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", PixelBot.Tokens.Dbots);
                request.Method = "POST";
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {


                    streamWriter.Write(json);
                }
                request.GetResponse();
            }
            catch
            {
            }
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://discordbots.org/api/bots/277933222015401985/stats");
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", PixelBot.Tokens.DbotsV2);
                request.Method = "POST";
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(json);
                }
                request.GetResponse();
            }
            catch
            {

            }
        }
    }
    public class Youtube
    {
        public static void YTNOTIFY()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = PixelBot.Tokens.Youtube
            });
        }
    }
    public class Blacklist
    {
        public static Timer Timer = new Timer();

        public static void CheckBlacklist(object sender, ElapsedEventArgs e)
        {
            foreach(var Guild in PixelBot._client.Guilds)
            {
                if (Properties.Settings.Default.Blacklist.Contains(Guild.Id.ToString()))
                {
                    Guild.LeaveAsync().GetAwaiter();
                }
            }
        }
    }
}