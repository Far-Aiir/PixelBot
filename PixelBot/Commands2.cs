using Bot.Game;
using Bot.Services;
using Bot.Utils;
using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MojangSharp.Endpoints;
using MojangSharp.Responses;
using Newtonsoft.Json;
using OverwatchAPI;
using PortableSteam;
using SteamStoreQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwitchCSharp.Clients;

namespace Bot.Commands
{
    public class Main2 : ModuleBase
    {
        private DiscordSocketClient _Client;
        public Main2(DiscordSocketClient client)
        {
            _Client = client;
        }
        [Command("test")]
        public async Task Test(string Region = "")
        {
            if (Region == "")
            {
                await ReplyAsync("`Please choose a region > p/lol region`");
            }
            else
            {
                string Check = _Riot.CheckGetApi(Region, out _Riot.UserRegion UserRegion, out _Utils_Http.Request Request);
                if (Check != "")
                {
                    await ReplyAsync($"`{Check}`");
                    return;
                }
                await ReplyAsync($"Region {UserRegion.Tag}");
            }

        }



        [Command("test2")]
        [RequireOwner]
        public async Task Test(string Region = "", [Remainder] string User = "")
        {
            if (Region == "" | User == "")
            {
                var embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        IconUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRidv7J3fXl5wUJIOTb-8-Pd3JM5IYD52JVBsCSk0lMFnz4tsPXpPvoLA",
                        Name = "League Of Legends",
                        Url = "http://leagueoflegends.com"
                    },

                    Color = Utils._Utils_Discord.GetRoleColor(Context.Channel as ITextChannel),
                    Description = "To get player stats do **p/lol (Region) (Summoner Name)** | Use the correct region!",
                };

                embed.AddField(x =>
                {
                    x.Name = "Regions"; x.Value = "```md" + Environment.NewLine + "<NA North America>" + Environment.NewLine + "<EUW EU West>" + Environment.NewLine + $"<EUNE EU Nordic/East>" + Environment.NewLine + "<LAN Latin Americ North>" + Environment.NewLine + "<LAS Latin America South>" + Environment.NewLine + "<BR Brazil>" + Environment.NewLine + "<JP Japan>" + Environment.NewLine + "<RU Russia>" + Environment.NewLine + "<TR Turkey>" + Environment.NewLine + "<OC Oceania>" + Environment.NewLine + "<KR Korea>```"; x.IsInline = true;
                });
                embed.AddField(x =>
                {
                    x.Name = "Stats"; x.Value = "```md" + Environment.NewLine + "<Test Test>```"; x.IsInline = true;
                });
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {

                try
                {
                    //bool HasGame = false;
                    var embed = new EmbedBuilder()
                    {
                        Title = $"[{Region}] {User}",
                        Description = "```md" + Environment.NewLine + $"```",
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = $"Last Played {(Utils._Utils_Other.UlongToDateTime(1111))}"
                        }
                    };
                    embed.AddField("Info", "```md" + Environment.NewLine + $"<Level {1}>" + Environment.NewLine + $"<ID {1}>```", true);
                    await Context.Channel.SendMessageAsync("", false, embed);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

       

        

        

        

        

        [Command("getbot"), Remarks("getbot (@Mention/User ID)"), Summary("Get info about any bot")]
        public async Task GetBotInfo(string User = "", string Api = "")
        {
            await _BotApi.GetInfo(Context.Channel as ITextChannel, User, Api);
        }

        [Command("getinvite"), Remarks("getinvite (@Mention/User ID)"), Summary("Get invite of a bot")]
        public async Task GetBotInvite(string User = "", string Api = "")
        {
            _BotApi.GetInvite(Context.Channel as ITextChannel, _Utils_Discord.MentionToID(User), Api);
        }

        [Command("getowner"), Remarks("getowner (@Mention/User ID)"), Summary("Get the owner of a bot"), Alias("getowners")]
        public async Task GetBotOwner(string User = "", string Api = "")
        {
            _BotApi.GetOwner(Context.Channel as ITextChannel, User, Api);
        }

        [Command("getbots"), Remarks("getbots (@Mention/User ID)"), Summary("Get a list of a users bots")]
        public async Task GetBots(string ID = "")
        {
            if (ID == "")
            {
                ID = Context.Message.Author.Id.ToString();
            }
            _BotApi.GetBots(Context.Channel as ITextChannel, ID);
        }

        [Command("upvotes")]
        public void Upvotes(string ID = "")
        {
            if (Context.User.Id == 190590364871032834)
            {
                _BotApi.GetUpvotes(Context.Channel as ITextChannel, ID);
            }

        }
    }
    
   

    public class Game2 : ModuleBase
    {
        private PaginationFull _PagFull;
        public Game2(CommandService commandservice, PaginationFull pagfull)
        {
            _PagFull = pagfull;
        }

        

        [Group("lol"), Summary("League Of Legends")]
        public class LolGroup : ModuleBase
        {
            [Command]
            public async Task LolHelp()
            {
                await ReplyAsync("Test");
            }

            [Command("test")]
            public async Task LolTest()
            {
                string Check = _Riot.CheckGetApi("na", out _Riot.UserRegion UserRegion, out _Utils_Http.Request Request);
                if (Check != "")
                {
                    await ReplyAsync($"`{Check}`");
                    return;
                }
                await ReplyAsync("`API is working`");
            }

            [Command("status")]
            public async Task LolStatus(string Option = "")
            {
                string RegionTag = "";
                if (Option == "" || Option == "all")
                {
                    RegionTag = "na";
                }
                else
                {
                    RegionTag = Option;
                }
                string Check = _Riot.CheckGetApi(RegionTag, out _Riot.UserRegion RegionNA, out Utils._Utils_Http.Request ReqNA);
                if (Check != "")
                {
                    await ReplyAsync($"`{Check}`");
                    return;
                }
                if (ReqNA.Success == false)
                {
                    await ReplyAsync($"`{ReqNA.Error}`");
                }
                if (Option == "")
                {
                    var embed = new EmbedBuilder()
                    {
                        Title = "<:LeagueOfLegends:358409851270856714> North America API Status",
                        Description = "API is online and working!",
                        Color = new Color(0, 200, 0),
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = "To get a list of regions use p/lol regions"
                        }
                    };
                    if (ReqNA.Json.services[0].status != "online")
                    {
                        embed.Description = "API Issues! with Game";
                        embed.Color = new Color(200, 0, 0);
                    }
                    if (ReqNA.Json.services[1].status != "online")
                    {
                        embed.Description = "API Issues! with Store";
                        embed.Color = new Color(200, 0, 0);
                    }
                    if (ReqNA.Json.services[2].status != "online")
                    {
                        embed.Description = "API Issues! with Website";
                        embed.Color = new Color(200, 0, 0);
                    }
                    if (ReqNA.Json.services[3].status != "online")
                    {
                        embed.Description = "API Issues! with Client";
                        embed.Color = new Color(200, 0, 0);
                    }
                    embed.Description = embed.Description + Environment.NewLine + Environment.NewLine + "Other Options" + Environment.NewLine + "p/lol status (RegionTag)" + Environment.NewLine + "p/lol status all";
                    await ReplyAsync("", false, embed.Build());
                }
                else if (Option == "all")
                {
                    IUserMessage Wait = await ReplyAsync("`Please wait...`");
                    _Riot.CheckGetApi("eune", out _Riot.UserRegion RegionEUNE, out _Utils_Http.Request ReqEUNE);
                    _Riot.CheckGetApi("euw", out _Riot.UserRegion RegionEUW, out _Utils_Http.Request ReqEUW);
                    _Riot.CheckGetApi("jp", out _Riot.UserRegion RegionJP, out _Utils_Http.Request ReqJP);
                    _Riot.CheckGetApi("kr", out _Riot.UserRegion RegionKR, out _Utils_Http.Request ReqKR);
                    _Riot.CheckGetApi("lan", out _Riot.UserRegion RegionLAN, out _Utils_Http.Request ReqLAN);
                    _Riot.CheckGetApi("las", out _Riot.UserRegion RegionLAS, out _Utils_Http.Request ReqLAS);
                    _Riot.CheckGetApi("br", out _Riot.UserRegion RegionBR, out _Utils_Http.Request ReqBR);
                    _Riot.CheckGetApi("oce", out _Riot.UserRegion RegionOCE, out _Utils_Http.Request ReqOCE);
                    _Riot.CheckGetApi("tr", out _Riot.UserRegion RegionTR, out _Utils_Http.Request ReqTR);
                    _Riot.CheckGetApi("ru", out _Riot.UserRegion RegionRU, out _Utils_Http.Request ReqRU);
                    string BR = "<Brazil Issue = BR>";
                    string EUNE = "<EU-Nordic-East Issue = EUNE>";
                    string EUW = "<EU-West Issue = EUW>";
                    string JP = "<Japan Issue = JP>";
                    string KR = "<Korea Issue = KR>";
                    string LAN = "<Latin-North-America Issue = LAN>";
                    string LAS = "<Latin-South-America Issue = LAS>";
                    string NA = "<North-America Issue = NA>";
                    string OCE = "<Oceania Issue = OCE>";
                    string TR = "<Turkey Issue = TR>";
                    string RU = "<Russia Issue = RU>";
                    if (ReqNA.Success)
                    {
                        NA = "<North-America Online = NA>";
                    }
                    if (ReqBR.Success)
                    {
                        BR = "<Brazil Online = BR>";
                    }
                    if (ReqEUNE.Success)
                    {
                        EUNE = "<EU-Nordic-East Online = EUNE>";
                    }
                    if (ReqEUW.Success)
                    {
                        EUW = "<EU-West Online = EUW>";
                    }
                    if (ReqJP.Success)
                    {
                        JP = "<Japan Online = JP>";
                    }
                    if (ReqKR.Success)
                    {
                        KR = "<Korea Online = KR>";
                    }
                    if (ReqLAN.Success)
                    {
                        LAN = "<Latin-North-America Online = LAN>";
                    }
                    if (ReqLAS.Success)
                    {
                        LAS = "<Latin-South-America Online = LAS>";
                    }
                    if (ReqOCE.Success)
                    {
                        OCE = "<Oceania Online = OCE>";
                    }
                    if (ReqTR.Success)
                    {
                        TR = "<Turkey Online = TR>";
                    }
                    if (ReqRU.Success)
                    {
                        RU = "<Russia Online = RU>";
                    }
                    var embed = new EmbedBuilder()
                    {
                        Title = "<:LeagueOfLegends:358409851270856714> All API Stats",
                        Description = "```md" + Environment.NewLine + string.Join(Environment.NewLine, new string[] { BR, EUNE, EUW, JP, KR, LAN, LAS, NA, OCE, TR, RU }) + "```",
                        Color = _Utils_Discord.GetRoleColor(Context.Channel),
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = "For more info use p/lol status (RegionTag) | p/lol status euw"
                        }
                    };
                    await ReplyAsync("", false, embed.Build());
                    await Wait.DeleteAsync();
                }
                else
                {
                    string GameStat = "Online";
                    string StoreStat = "Online";
                    string WebsiteStat = "Online";
                    string ClientStat = "Online";
                    Color Color = new Color(0, 200, 0);

                    if (ReqNA.Json.services[0].status != "online")
                    {
                        GameStat = "Issue";
                        Color = new Color(200, 0, 0);
                    }
                    if (ReqNA.Json.services[1].status != "online")
                    {
                        StoreStat = "Issue";
                        Color = new Color(200, 0, 0);
                    }
                    if (ReqNA.Json.services[2].status != "online")
                    {
                        WebsiteStat = "Issue";
                        Color = new Color(200, 0, 0);
                    }
                    if (ReqNA.Json.services[3].status != "online")
                    {
                        ClientStat = "Issue";
                        Color = new Color(200, 0, 0);
                    }
                    var embed = new EmbedBuilder()
                    {
                        Title = $"<:LeagueOfLegends:358409851270856714> {ReqNA.Json.name} API Status",
                        Description = "API is online and working!" + Environment.NewLine + "```md" + Environment.NewLine + $"<Game {GameStat}> <Store {StoreStat}> <Website {WebsiteStat}> <Client {ClientStat}>```",
                        Color = Color,
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = "To get a list of regions use p/lol regions"
                        }
                    };
                    await ReplyAsync("", false, embed.Build());
                }
            }

            [Command("regions"), Alias("region")]
            public async Task LolRegions()
            {
                await ReplyAsync("");
            }
        }

        

        

        

        [Command("wot"), Remarks("wot"), Summary("World Of Tanks user info")]
        public async Task Wot(string Region = "", [Remainder] string User = "")
        {
            if (Region == "" || User == "")
            {
                var embed = new EmbedBuilder()
                {
                    Description = "**p/wot (Region) (User)**",
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = "World Of Tanks"
                    },
                    Color = _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel)
                };
                embed.AddField("Regions", "```md" + Environment.NewLine + "<RU Russia>" + Environment.NewLine + "<EU Europe>" + Environment.NewLine + "<NA America>" + Environment.NewLine + "<AS Asia>```", true);
                embed.AddField("Stats", "Player data available" + Environment.NewLine + "New features coming soon", true);
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                _WOT.Player Player = null;
                Player = _WOT.GetUserStats(Region, User);
                if (Player.Status == _WOT.RequestStatus.UnknownRegion)
                {
                    await ReplyAsync("`Unknown Region`");
                    return;
                }
                if (Player.Status == _WOT.RequestStatus.UnknownPlayer)
                {
                    await ReplyAsync("`Could not find player`");
                    return;
                }
                if (Player.Status == _WOT.RequestStatus.Other)
                {
                    await ReplyAsync("`Request error`");
                    return;
                }
                var embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = $"[{Region}] {User} | Raiting {Player.Raiting}",
                        IconUrl = "http://orig10.deviantart.net/4482/f/2015/301/2/c/world_of_tanks_icon___tiger1_by_adyshor37-d9eng1i.png"
                    },
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Created {Player.CreatedAt.ToShortDateString()} | Last Battle {Player.LastBattle.ToShortDateString()}"
                    },
                    Color = _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel),
                    Description = "```md" + Environment.NewLine + $"<Battles {Player.Battles}> <Win {Player.Win}> <Loss {Player.Loss}> <Draw {Player.Draws}>" + Environment.NewLine + $"<Shots {Player.Shots}> <Hits {Player.Hits}> <Miss {Convert.ToUInt32(Player.Shots) - Convert.ToUInt32(Player.Hits)}>```More stats coming soon this is a test"
                };
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        

        [Group("vg22"), Alias("vainglory"), Summary("Vainglory commands")]
        public class VaingloryGroup : ModuleBase
        {
            [Command]
            public async Task VaingloryHelp()
            {
                var embed = new EmbedBuilder()
                {
                    Title = "Vainglory",
                    Url = "http://www.vainglorygame.com/",
                    Description = "Vainglory is a MOBA similar to league of legends that is a 3 vs 3 match with other online players available for android and ios```md" + Environment.NewLine + "[ p/vguser (Region) (User) ]( Get user stats )" + Environment.NewLine + "[ p/vgmatch (Region) (ID) ]( Coming Soon Get a match by ID )" + Environment.NewLine + "[ p/vgmatches (Region) (User) ]( Coming Soon Get the last 3 matches of a player )" + Environment.NewLine + "Full stats for all heroes and items coming soon```",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "Regions > na (North America) | eu (Europe) | sa (South Asia) | ea (East Asia) | sg (Sea)"
                    }
                };
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }

            [Command("user"), Remarks(""), Summary("User info")]
            public async Task VaingloryUser(string Region = "", [Remainder] string User = "")
            {
                if (Region == "" || User == "")
                {
                    await Context.Channel.SendMessageAsync("`No region or user > p/vg (Region) (User) | na | eu | sa | ea | sg`").ConfigureAwait(false);
                    return;
                }
                if (Region == "na" || Region == "eu" || Region == "sa" || Region == "ea" || Region == "sg")
                {
                    _Vainglory.Player Player = _Vainglory.GetPlayerStats(Region, User);
                    //dynamic JB = Apis.Vainglory.GetPlayerMatch(Region, VGUser);
                    //Console.WriteLine(JA);
                    var embed = new EmbedBuilder()
                    {
                        Title = $"Vainglory | {User}",
                        Description = "```md" + Environment.NewLine + $"<Level {Player.Level}> <XP {Player.XP}> <LifetimeGold {Player.LifetimeGold}>" + Environment.NewLine + $"<Wins {Player.Wins}> <Played {Player.Played}> <PlayedRank {Player.PlayedRanked}>" + Environment.NewLine + $"<KarmaLevel {Player.KarmaLevel}> <skillTier {Player.SkillTier}>```"
                    };
                    await Context.Channel.SendMessageAsync("", false, embed).ConfigureAwait(false);

                }
                else
                {
                    await Context.Channel.SendMessageAsync("`Unknown region p/vg (Region) (User) | na | eu | sa | ea | sg`").ConfigureAwait(false);
                }
            }

            [Command("match")]
            public async Task VaingloryMatch(string Region = "", [Remainder] string User = "")
            {
                await ReplyAsync("`Under development`");
            }
        }
    }

    public class Media2 : ModuleBase
    {
        [Group("yt")]
        public class YoutubeGroup : ModuleBase
        {
            [Command]
            public async Task YoutubeHelp()
            {
                await ReplyAsync("Test");
            }

            [Command("user")]
            public async Task YoutubeUser(string User)
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = _Config.Tokens.Youtube
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
        }

        [Group("tw")]
        public class TwitchGroup : ModuleBase
        {
            private readonly Twitch _Twitch;
            public TwitchGroup(Twitch twitch)
            {
                _Twitch = twitch;
            }

            [Command]
            public async Task TwitchHelp()
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
                    Color = _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel)
                };
                if (Context.Channel is IPrivateChannel)
                {
                    await Context.Channel.SendMessageAsync("", false, infoembed.Build());
                }
                else
                {
                    IGuildUser BotUser = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
                    if (BotUser.GetPermissions(Context.Channel as ITextChannel).EmbedLinks)
                    {
                        await Context.Channel.SendMessageAsync("", false, infoembed.Build());
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("Twitch channel lookup/search and livestream notifications in channel or user DMs" + Environment.NewLine + "```md" + Environment.NewLine + "[ p/tw (Channel) ]( Get info about a channel )" + Environment.NewLine + "[ p/tw s (Channel ]( Get 3 channel names )" + Environment.NewLine + "[ p/tw n (Option) (Channel) ]( Get a notification when a streamer goes live )" + Environment.NewLine + "[ p/tw l (Option) ]( Get a list of notification settings )" + Environment.NewLine + "[ p/tw r (Option) (Channel) ]( Remove a channel from notification setting )" + Environment.NewLine + "<Options > ME (User DM) | HERE (Guild Channel)```");
                    }
                }
            }

            [Command("tw user"), Alias("channel"), Remarks("tw user (User/Channel)"), Summary("Twitch user/channel info")]
            public async Task TwitchChannel(string User)
            {
                if (User == "")
                {
                    await Context.Channel.SendMessageAsync("`Enter a channel name | p/tw c MyChannel`");
                    return;
                }
                var client = new TwitchAuthenticatedClient(_Config.Tokens.Twitch, _Config.Tokens.TwitchAuth);
                var t = client.GetChannel(User);
                if (t.CreatedAt.Year == 0001)
                {
                    var Usearch = client.SearchChannels(User).List;
                    var embedsearch = new EmbedBuilder()
                    {
                        Title = "Twitch Channels",
                        Description = $"{Usearch[0].Name} | {Usearch[1].Name} | {Usearch[2].Name}",
                        Color = _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel)
                    };
                    if (Context.Channel is IPrivateChannel)
                    {
                        await Context.Channel.SendMessageAsync("", false, embedsearch.Build());
                        return;
                    }

                    IGuildUser BotUser = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);

                    if (BotUser.GetPermissions(Context.Channel as ITextChannel).EmbedLinks)
                    {
                        await Context.Channel.SendMessageAsync("", false, embedsearch.Build());
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("**Twitch Channels**" + Environment.NewLine + $"{Usearch[0].Name} | {Usearch[1].Name} | {Usearch[2].Name}");
                    }
                    return;
                }
                string EmbedTitle = $"{t.DisplayName} - (Offline)";
                string EmbedText = "```md" + Environment.NewLine + $"<Created {t.CreatedAt.ToShortDateString()}> <Updated {t.UpdatedAt.ToShortDateString()}>" + Environment.NewLine + $"<Followers {t.Followers}> <Views {t.Views}>```";
                if (client.IsLive(User) == true)
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
                        Text = $"Subscribe to streamer online alerts with | p/tw notify me {User}"
                    }
                };
                if (!EmbedTitle.Contains("(Offline)"))
                {
                    embed.Color = new Color(0, 255, 0);
                }
                Console.WriteLine(t.Game);
                var a = client.GetMyStream();
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }

            [Command("tw notify"),Alias("n"),Remarks("tw n (Option) (Channel)"),Summary("Recieve notifications from a twitch channel")]
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
                if (Option == null || Channel == null)
                {
                    await Context.Channel.SendMessageAsync("`Enter an option | p/tw notify me (Channel) - Sends a message in DMS | p/tw notify here (Channel) - Sends a message in this channel (Server Owner Only!)`");
                    return;
                }
                var client = new TwitchAuthenticatedClient(_Config.Tokens.Twitch, _Config.Tokens.TwitchAuth);
                var t = client.GetChannel(Channel);
                if (t.CreatedAt.Year == 0001)
                {
                    await Context.Channel.SendMessageAsync("`Cannot find this channel`");
                    return;
                }
                if (Option == "me")
                {
                    if (_Twitch.NotificationList.Exists(x => x.Twitch == Channel.ToLower() & x.User == Context.User.Id & x.Type == "user"))
                    {
                        await Context.Channel.SendMessageAsync($"`You already have {Channel} listed`");
                        return;
                    }
                    Twitch.TwitchClass NewNotif = new Twitch.TwitchClass()
                    {
                        Type = "user",
                        Guild = Context.Guild.Id,
                        Channel = Context.Channel.Id,
                        User = Context.User.Id,
                        Twitch = Channel.ToLower(),
                        Live = false
                    };
                    JsonSerializer serializer = new JsonSerializer();
                    using (StreamWriter file = File.CreateText(_Config.BotPath + $"Twitch\\user-{Context.User.Id}-{Channel.ToLower()}.json"))
                    {
                        serializer.Serialize(file, NewNotif);
                    }
                    _Twitch.NotificationList.Add(NewNotif);
                    await Context.Channel.SendMessageAsync($"`You added notifications for {Channel} to yourself`");

                }
                else if (Option == "here" || Option == "channel")
                {
                    if (Context.User.Id != Context.Guild.OwnerId)
                    {
                        await Context.Channel.SendMessageAsync("`You are not the guild owner`");
                        return;
                    }
                    if (_Twitch.NotificationList.Exists(x => x.Twitch == Channel.ToLower() & x.Guild == Context.Guild.Id & x.Type == "channel" & x.Channel == Context.Channel.Id))
                    {
                        await Context.Channel.SendMessageAsync($"`This channel already has {Channel} listed`");
                        return;
                    }
                    Twitch.TwitchClass NewNotif = new Twitch.TwitchClass()
                    {
                        Type = "channel",
                        Guild = Context.Guild.Id,
                        Channel = Context.Channel.Id,
                        User = Context.User.Id,
                        Twitch = Channel.ToLower(),
                        Live = false
                    };
                    JsonSerializer serializer = new JsonSerializer();
                    using (StreamWriter file = File.CreateText(_Config.BotPath + $"Twitch\\channel-{Context.Guild.Id.ToString()}-{Context.Channel.Id}-{Channel.ToLower()}.json"))
                    {
                        serializer.Serialize(file, NewNotif);
                    }
                    _Twitch.NotificationList.Add(NewNotif);
                    await Context.Channel.SendMessageAsync($"`You added notifications for {Channel} to this channel`");
                }
            }

            [Command("tw list"),Alias("l"),Remarks("tw list (Option)"),Summary("List your twitch notification settings")]
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
                    List<string> TWList = _Twitch.NotificationList.Where(x => x.User == Context.User.Id & x.Type == "user").Select(x => x.Twitch).ToList();
                    var embed = new EmbedBuilder()
                    { Title = "Twitch Notifications For You", Description = string.Join(", ", TWList), Color = _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel) };
                    await Context.Channel.SendMessageAsync("", false, embed);
                }
                if (Option == "guild")
                {
                    List<string> TWList = _Twitch.NotificationList.Where(x => x.Guild == Context.Guild.Id & x.Type == "channel").Select(x => x.Twitch).ToList();
                    var embed = new EmbedBuilder()
                    { Title = "Twitch Notifications For This Guild", Description = string.Join(", ", TWList), Color = _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel) };
                    await Context.Channel.SendMessageAsync("", false, embed);
                }
                if (Option == "here")
                {
                    List<string> TWList = _Twitch.NotificationList.Where(x => x.Channel == Context.Channel.Id & x.Type == "channel").Select(x => x.Twitch).ToList();
                    var embed = new EmbedBuilder()
                    {
                        Title = $"Twitch Notifications For #{Context.Channel.Name}",
                        Description = string.Join(", ", TWList),
                        Color = _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel)
                    };
                    await Context.Channel.SendMessageAsync("", false, embed);
                }
            }

            [Command("tw remove"),Alias("r"),Remarks("tw r (Option) (Channel)"),Summary("Remove notifications from a twitch channel")]
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
                    if (_Twitch.NotificationList.Exists(x => x.Type == "user" & x.Twitch == Channel.ToLower() & x.User == Context.User.Id))
                    {
                        _Twitch.NotificationList.RemoveAll(x => x.Type == "user" & x.Twitch == Channel.ToLower() & x.User == Context.User.Id);
                        File.Delete($"Twitch\\user-{Context.User.Id}-{Channel.ToLower()}.json");
                        await Context.Channel.SendMessageAsync($"`You removed notifications for {Channel} from yourself`");
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("`This twitch channel does not exist in your notifications`");
                    }
                }
                if (Option == "here" || Option == "Channel")
                {
                    if (Context.User.Id != Context.Guild.OwnerId)
                    {
                        await Context.Channel.SendMessageAsync("`You are not the guild owner`");
                        return;
                    }
                    if (_Twitch.NotificationList.Exists(x => x.Type == "channel" & x.Twitch == Channel.ToLower() & x.Guild == Context.Guild.Id & x.Channel == Context.Channel.Id))
                    {
                        _Twitch.NotificationList.RemoveAll(x => x.Type == "channel" & x.Twitch == Channel & x.Guild == Context.Guild.Id & x.Channel == Context.Channel.Id);

                        File.Delete($"Twitch\\channel-{Context.Guild.Id.ToString()}-{Context.Guild.Id}-{Channel.ToLower()}.json");
                        await Context.Channel.SendMessageAsync($"`You removed notifications for {Channel} from this channel`");
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("`This twitch channel does not exist for the channel notifications`");
                    }
                }
            }
        }
    }

    

   
    
}

