using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PixelBot.Services;

namespace PixelBot
{
    public class DiscordEvents
    {
        public DiscordSocketClient _Client;
        public DiscordEvents(DiscordSocketClient Client)
        {
            _Client = Client;
            _Client.Connected += () =>
            {
                if (Config.BlacklistChannel == null)
                {
                    IGuild Guild = _Client.GetGuild(275054291360940032);
                    Config.BlacklistChannel = Guild.GetTextChannelAsync(327398925889961984).GetAwaiter().GetResult();
                }
                if (Config.NewGuildChannel == null)
                {
                    IGuild Guild = _Client.GetGuild(275054291360940032);
                    Config.NewGuildChannel = Guild.GetTextChannelAsync(327398889298853898).GetAwaiter().GetResult();
                }
                return Task.CompletedTask;
            };

            _Client.Disconnected += (r) =>
            {
                Config.BlacklistChannel = null;
                Config.NewGuildChannel = null;
                return Task.CompletedTask;
            };

            _Client.Ready += () =>
            {
                if (Config.DevMode == false)
                {
                    _Client.SetGameAsync($"p/help | {_Client.Guilds.Count} Guilds | https://blaze.ml").GetAwaiter();

                    if (Config.FirstStart == false)
                    {
                        
                        Utils.DiscordUtils.UpdateUptimeGuilds();
                        
                        
                        Console.WriteLine("[PixelBot] Timer Service Online");
                    }
                }
                Config.FirstStart = true;
                return Task.CompletedTask;
            };

            _Client.LeftGuild += (g) =>
            {
                Utils.DiscordUtils.GuildBotCache.Remove(g.Id);
                Console.WriteLine($"[Left] > {g.Name} - {g.Id}");
                if (Config.DevMode == false)
                {
                    _Client.SetGameAsync($"p/help | {_Client.Guilds.Count} Guilds | https://blaze.ml ").GetAwaiter();
                    if (Config.NewGuildChannel != null)
                    {
                        var embed = new EmbedBuilder()
                        {
                            Title = $"Left {g.Name}",
                            Description = g.Id.ToString()
                        };
                        Config.NewGuildChannel.SendMessageAsync("", false, embed).GetAwaiter();
                    }
                }
                foreach (var i in Config.ALLUSERS.Where(x => x.Guild.Id == g.Id))
                {
                    Config.ALLUSERS.Remove(i);
                }
                return Task.CompletedTask;
            };

            _Client.JoinedGuild += (g) =>
            {
                #region Blacklist
                if (Config.DevMode == false)
                {
                    var GuildUsers = g.Users;
                    int Users = GuildUsers.Where(x => !x.IsBot).Count();
                    int Bots = GuildUsers.Where(x => x.IsBot).Count();
                    if (Config.DevMode == false & Properties.Settings.Default.Blacklist.Contains(g.Id.ToString()))
                    {
                        Console.WriteLine($"[Blacklist] Removed {g.Name} - {g.Id}");
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

                    if (g.Users.Where(x => x.IsBot).Count() * 100 / g.Users.Count() > 90)
                    {
                        Console.WriteLine($"[Blacklist] Added {g.Name} - {g.Id}");
                        Properties.Settings.Default.Blacklist.Add(g.Id.ToString());
                        Properties.Settings.Default.Save();
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
                }
                #endregion
                if (Config.DevMode == false)
                {
                    _Client.SetGameAsync($"p/help | {_Client.Guilds.Count} Guilds | https://blaze.ml ").GetAwaiter();
                    if (Config.NewGuildChannel != null)
                    {
                        var embed = new EmbedBuilder()
                        {
                            Title = $"Joined {g.Name}",
                            Description = g.Id.ToString()
                        };
                        Config.NewGuildChannel.SendMessageAsync("", false, embed).GetAwaiter();
                    }
                }
                IGuildUser BotUser = g.GetUser(_Client.CurrentUser.Id);
                Utils.DiscordUtils.GuildBotCache.Add(g.Id, BotUser);
                Console.WriteLine($"[Joined] {g.Name} - {g.Id}");
                foreach (var User in g.Users.Where(x => !x.IsBot))
                {
                    if (!Config.ALLUSERS.Exists(x => x.Id == User.Id))
                    {
                        Config.ALLUSERS.Add(User);
                    }
                }
                return Task.CompletedTask;
            };

            _Client.UserJoined += (u) =>
            {
                if (!Config.ALLUSERS.Exists(x => x.Id == u.Id))
                {
                    Config.ALLUSERS.Add(u);
                }
                return Task.CompletedTask;
            };

            _Client.UserLeft += (u) =>
            {
                if (!Config.ALLUSERS.Exists(x => x.Id == u.Id & x.GuildId == u.Guild.Id))
                {
                    Config.ALLUSERS.Add(u);
                }
                return Task.CompletedTask;
            };

            _Client.GuildAvailable += (g) =>
            {
                #region Blacklist
                if (Config.DevMode == false)
                {
                    var GuildUsers = g.Users;
                    int Users = GuildUsers.Where(x => !x.IsBot).Count();
                    int Bots = GuildUsers.Where(x => x.IsBot).Count();
                    if (Properties.Settings.Default.Blacklist.Contains(g.Id.ToString()))
                    {
                        Console.WriteLine($"[Blacklist] Removed {g.Name} - {g.Id}");
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

                    if (g.Users.Where(x => x.IsBot).Count() * 100 / g.Users.Count() > 90)
                    {
                        Console.WriteLine($"[Blacklist] Added {g.Name} - {g.Id}");
                        Properties.Settings.Default.Blacklist.Add(g.Id.ToString());
                        Properties.Settings.Default.Save();
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
                }
                #endregion
                IGuildUser BotUser = g.GetUser(_Client.CurrentUser.Id);
                Utils.DiscordUtils.GuildBotCache.Add(g.Id, BotUser);
                foreach (var User in g.Users)
                {
                    if (!Config.ALLUSERS.Exists(x => x.Id == User.Id))
                    {
                        Config.ALLUSERS.Add(User);
                    }
                }
                return Task.CompletedTask;
            };
        }
    }
}
