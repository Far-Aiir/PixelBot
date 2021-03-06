﻿using Discord;
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
using Discord.WebSocket;
using Bot;

namespace Bot.Services
{
    public class Twitch
    {
        readonly DiscordSocketClient _Client;
        public Twitch(DiscordSocketClient client)
        {
            Directory.CreateDirectory(_Config.BotPath + "Twitch\\");
            _Client = client;
            foreach (var File in Directory.GetFiles(_Config.BotPath + "Twitch\\"))
            {
                using (StreamReader reader = new StreamReader(File))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    NotificationList.Add((TwitchClass)serializer.Deserialize(reader, typeof(TwitchClass)));
                }
            }
            Timer.Interval = 60000;
            Timer.Elapsed += SendNotifications;
            Timer.Start();
        }
        public class TwitchClass
        {
            public string Type { get; set; }
            public ulong User { get; set; }
            public ulong Guild { get; set; }
            public ulong Channel { get; set; }
            public string Twitch { get; set; }
            public bool Live { get; set; }
        }
        public Timer Timer = new Timer();

        public List<TwitchClass> NotificationList = new List<TwitchClass>();

        public void SendNotifications(object sender, ElapsedEventArgs e)
        {
            var TwitchClient = new TwitchAuthenticatedClient(_Config.Tokens.Twitch, _Config.Tokens.TwitchAuth);
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
                                IGuild Guild = _Client.GetGuild(Item.Guild);
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
                                var DM = User.GetOrCreateDMChannelAsync().GetAwaiter().GetResult();
                                DM.SendMessageAsync("", false, embed.Build()).GetAwaiter();
                                JsonSerializer serializer = new JsonSerializer();
                                using (StreamWriter file = File.CreateText(_Config.BotPath + $"Twitch\\user-{Item.User}-{Item.Twitch.ToLower()}.json"))
                                {
                                    serializer.Serialize(file, Item);
                                }
                            }
                            else
                            {
                                IGuild Guild = _Client.GetGuild(Item.Guild);
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
                                Channel.SendMessageAsync("", false, embed.Build()).GetAwaiter();
                                JsonSerializer serializer = new JsonSerializer();
                                using (StreamWriter file = File.CreateText(_Config.BotPath + $"Twitch\\channel-{Item.Guild.ToString()}-{Item.Channel}-{Item.Twitch}.json"))
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
    public class Youtube
    {
        public void YTNOTIFY()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _Config.Tokens.Youtube
            });
        }
    }
}