using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.IO;
using System.Net;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Newtonsoft.Json;
using SteamStoreQuery;
using PortableSteam;
using TwitchCSharp.Clients;
using Discord.Addons.InteractiveCommands;
using Discord.Addons.Preconditions;
using OverwatchAPI;
using RiotApi.Net.RestClient;
using RiotApi.Net.RestClient.Configuration;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    public static bool DevMode = true;
    public static string BotPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\PixelBot\\";
    public static bool FirstStart = false;
    public static DiscordSocketClient _client;
    public static CommandService _commands = new CommandService();
    public static PaginationService.Min.Service _PagMin;
    public static PaginationService.Full.Service _PagFull;
    public static TokenClass _Token = new TokenClass();
    static void Main()
    {
        DisableConsoleQuickEdit.Go();
        if (File.Exists(BotPath + "LIVE.txt"))
        {
            Console.Title = "PixelBot";
            Program.DevMode = false;
        }
        else
        {
            Console.Title = "[DevMode] PixelBot";
        }
        using (StreamWriter file = File.CreateText(BotPath + "TokensTemplate" + ".json"))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(file, _Token);

        }
        string TokenPath = BotPath + "Tokens.txt";
        Directory.CreateDirectory(BotPath + "Uptime\\");
        Directory.CreateDirectory(BotPath + "Twitch\\");
        foreach (var File in Directory.GetFiles(BotPath + "Uptime\\"))
        {
                using (StreamReader reader = new StreamReader(File))
                {
                Services.UptimeBotsList.Add(Convert.ToUInt64(File.Replace(BotPath + "Uptime\\", "").Replace(".txt", "")), Convert.ToInt32(reader.ReadLine()));
                }
        }
        foreach (var File in Directory.GetFiles(BotPath + "Twitch\\"))
        {
            using (StreamReader reader = new StreamReader(File))
            {
                JsonSerializer serializer = new JsonSerializer();
                Services.TwitchNotifyClass Item = (Services.TwitchNotifyClass)serializer.Deserialize(reader, typeof(Services.TwitchNotifyClass));
                Services.TwitchNotifications.Add(Item);
            }
        }
        if (PixelBot.Properties.Settings.Default.Blacklist == null)
        {
            PixelBot.Properties.Settings.Default.Blacklist = new System.Collections.Specialized.StringCollection();
        }
        
        using (StreamReader file = File.OpenText(BotPath + "Tokens.json"))
        {
            JsonSerializer serializer = new JsonSerializer();
            TokenClass Items = (TokenClass)serializer.Deserialize(file, typeof(TokenClass));
            _Token = Items;
        }
        //new Program().Task2().ConfigureAwait(false);
        new Program().RunBot().GetAwaiter().GetResult();
    }
    public async Task RunBot()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            AlwaysDownloadUsers = true
        });
        await InstallCommands();
        

        _client.Connected += () =>
        {
            if (DevMode == true)
            {
                Console.Title = "[DevMode] PixelBot - Online!";
                Console.WriteLine("[PixelBot] Connected - DevMode is enabled!");
            }
            else
            {
                Console.Title = "PixelBot - Online!";
                Console.WriteLine("[PixelBot] Connected");
            }
            return Task.CompletedTask;
        };

        _client.UserJoined += (User) =>
        {
            if (DevMode == false & User.Guild.Id == 81384788765712384 || User.Guild.Id == 264445053596991498 & User.IsBot & !File.Exists(Program.BotPath + "Uptime\\" + User.Id.ToString() + ".txt"))
            {
                  System.IO.File.WriteAllText(BotPath + "Uptime\\" + User.Id.ToString() + ".txt", "75");
            }
            return Task.CompletedTask;
        };

        _client.Disconnected += (r) =>
        {
            if (DevMode == true)
            {
                Console.Title = "[DevMode] PixelBot - Offline!";
            }
            else
            {
                Console.Title = "PixelBot - Offline!";
            }
            Console.WriteLine("[PixelBot] Disconnected");
            return Task.CompletedTask;
        };

        _client.Ready += async () =>
        {
            Console.WriteLine($"[PixelBot] Online in {_client.Guilds.Count} Guilds");
            Services._Timer_Twitch.Interval = 60000;
            Services._Timer_Twitch.Elapsed += Services.TwitchNotification;
            Services._Timer_Twitch.Start();
            if (DevMode == false)
            {
                await _client.SetGameAsync($"p/help | {_client.Guilds.Count} Guilds | https://blaze.ml");
                if (FirstStart == false)
                {
                    _Utils.UpdateUptimeGuilds();
                    Services.BotGuildCount(null, null);
                    
                    Services._Timer_Stats.Interval = 600000;
                    Services._Timer_Stats.Elapsed += Services.BotGuildCount;
                    Services._Timer_Stats.Start();
                    Services._Timer_Uptime.Interval = 600000;
                    Services._Timer_Uptime.Elapsed += Services.Uptime;
                    Services._Timer_Uptime.Start();
                    Console.WriteLine("[PixelBot] Timer Service Online");
                }
            }
            FirstStart = true;
        };

        _client.LeftGuild += async (g) =>
        {
            _Utils.GuildBotCache.Remove(g.Id);
            Console.WriteLine($"[Left] > {g.Name} - {g.Id}");
            if (DevMode == false)
            {
                await _client.SetGameAsync($"p/help | {_client.Guilds.Count} Guilds | https://blaze.ml");
            }
        };

        _client.JoinedGuild += async (g) =>
        {
            if (DevMode == false & PixelBot.Properties.Settings.Default.Blacklist.Contains(g.Id.ToString()))
            {
                Console.WriteLine($"[Blacklist] Removed {g.Name} - {g.Id}");
                await g.DefaultChannel.SendMessageAsync($"This guild has been blacklist by the owner ({g.Id}) contact `xXBuilderBXx#9113` for more info");
                await g.LeaveAsync();
            }
            IGuildUser BotUser = g.GetUser(_client.CurrentUser.Id);
            _Utils.GuildBotCache.Add(g.Id, BotUser);
            Console.WriteLine($"[Joined] {g.Name} - {g.Id}");
                await _client.SetGameAsync($"p/help | {_client.Guilds.Count} Guilds | https://blaze.ml ");
        };

        _client.GuildAvailable += async (g) =>
        {
            if (DevMode == false & PixelBot.Properties.Settings.Default.Blacklist.Contains(g.Id.ToString()))
            {
                Console.WriteLine($"[Blacklist] Removed {g.Name} - {g.Id}");
                await g.DefaultChannel.SendMessageAsync($"This guild has been blacklist by the owner ({g.Id}) contact `xXBuilderBXx#9113` for more info");
                await g.LeaveAsync();
            }
            else
            {
                IGuildUser BotUser = g.GetUser(_client.CurrentUser.Id);
                _Utils.GuildBotCache.Add(g.Id, BotUser);
            }
        };
        _client.MessageReceived += (g) =>
        {
            if (g.Channel is IGuildChannel)
            {
                var GU = g.Channel as ITextChannel;
                if (GU.GuildId == 323533467608416256)
                {
                    Console.WriteLine($"{g.Author.Username} | {g.Content}");
                }
            }
            return Task.CompletedTask;
        };
        try
        {
            await _client.LoginAsync(TokenType.Bot, _Token.Discord);
            await _client.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        await Task.Delay(-1);
    }
    

    private async Task InstallCommands()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        _PagFull = new PaginationService.Full.Service(_client);
        _PagMin = new PaginationService.Min.Service(_client);
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
            var result = await _commands.ExecuteAsync(context, argPos);
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
        }
        else
        {
            if (!(message.HasStringPrefix("p/", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
            var context = new CommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, argPos);
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
            await Context.Channel.SendMessageAsync("`You need to mention a bot or insert the ID | p/uptime @Bot`");
        }
        else
        {
            GuildUser = await _Utils.GetFullUserAsync(Context.Guild, User);
            if (!GuildUser.IsBot)
            {
                await Context.Channel.SendMessageAsync("`This is not a bot`");
            }
            else
            {
                if (Services.UptimeBotsList.Keys.Contains(GuildUser.Id))
                {
                    int Uptime = 75;
                    Services.UptimeBotsList.TryGetValue(GuildUser.Id, out Uptime);
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
    [RequireOwner]
    public async Task Test(string Region = "",[Remainder] string User = "")
    {
        RiotApiConfig.Regions UserRegion = RiotApiConfig.Regions.Global;
        UserRegion = _Apis.LOL.Data.GetRegion(Region);
        if (Region == "" | User == "" || UserRegion == RiotApiConfig.Regions.Global)
        {
            var embed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    IconUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRidv7J3fXl5wUJIOTb-8-Pd3JM5IYD52JVBsCSk0lMFnz4tsPXpPvoLA",
                    Name = "League Of Legends",
                    Url = "http://leagueoflegends.com"
                },
                
                Color = _Utils.GetRoleColor(Context),
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
                IRiotClient RiotClient = new RiotClient(Program._Token.Riot);
                var Summoner = RiotClient.Summoner.GetSummonersByName(UserRegion, User);
                var ID = Summoner.Values.First().Id;
                var LatestGame = RiotClient.Game.GetRecentGamesBySummonerId(UserRegion, ID);
                bool HasGame = false;
                if (LatestGame != null)
                {
                    HasGame = true;
                    var First = LatestGame.Games.First();
                    //First.CreateDate
                }
                var embed = new EmbedBuilder()
                {
                    Title = $"[{Region}] {User}",
                    Description = "```md" + Environment.NewLine + $"```",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Last Played {_Utils.EpochToDateTime(Summoner.First().Value.RevisionDate)}"
                    }
                };
                embed.AddInlineField("Info", "```md" + Environment.NewLine + $"<Level {Summoner.Values.First().SummonerLevel}>" + Environment.NewLine + $"<ID {ID}>```");
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    [Command("testt")]
    [RequireOwner]
    public async Task Testt(string all)
    {
        switch (all)
        {
            case "all":
                    var messages = Context.Channel.GetMessagesAsync(100);
                var mes = await messages.Flatten();
                if (mes.Count() != 0)
                {
                    goto case "test";
                }
                break;
            case "test":
                var messages2 = Context.Channel.GetMessagesAsync(100);
                var mes2 = await messages2.Flatten();
                foreach(var i in mes2)
                {
                    await i.DeleteAsync();
                }
                goto case "all";
                break;
        }
        await Context.Channel.SendMessageAsync("Messages prunes");
    
    }

    [Command("poke")]
    [Remarks("poke")]
    [Summary("Pokemon info and commands")]
    public async Task Pokemon(string Name)
    {
        int Data = _Apis.Poke.GetPokemonID(Name);
        if (Data == 0)
        {
            await Context.Channel.SendMessageAsync($"`Cannot find pokemone {Name}`");
            return;
        }
        var embed = new EmbedBuilder()
        {
            Color = _Utils.GetRoleColor(Context),
            ImageUrl = "https://raw.githubusercontent.com/PokeAPI/pokeapi/master/data/v2/sprites/pokemon/" + Data + ".png"
        };
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
            ApiKey = Program._Token.Youtube
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
        
    }

    [Command("yt notify list")]
    public async Task Ytlist()
    {
        List<string> YTList = new List<string>();
        await Context.Channel.SendMessageAsync("```fix" + Environment.NewLine + "Feature under construction```");

        //MySQLConnection myConn;
        //MySQLDataReader MyReader = null;
        //myConn = new MySQLConnection(new MySQLConnectionString(Program.TokenMap.MysqlHost, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlUser, Program.TokenMap.MysqlPass).AsString);
        //myConn.Open();
        //string stm = $"SELECT channel FROM ytnotify WHERE id='{Context.User.Id}'";
        //MySQLCommand cmd = new MySQLCommand(stm, myConn);
        //MyReader = cmd.ExecuteReaderEx();
        //while (MyReader.Read())
        //{
            //YTList.Add(MyReader.GetString(0));
        //}
        //MyReader.Close();
        //myConn.Close();
        //string line = string.Join(Environment.NewLine, YTList.ToArray());
        //await Context.Channel.SendMessageAsync(line);
    }

    [Command("yt notify remove")]
    public async Task Ytdel([Remainder] string User = "null")
    {
        await Context.Channel.SendMessageAsync("```fix" + Environment.NewLine + "Feature under construction```");
        
    }

    [Command("profile")]
    [RequireOwner]
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
        List<string> UList = new List<string>();
        string RS = "";
        if (RS == "")
        {
            char[] arr = User.ToCharArray();
            arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))));
            string str = new string(arr);
            string SS = $"INSERT INTO profiles(name, discrim, id) VALUES('{str}', '{User}', '{User}')";
            await Context.Channel.SendMessageAsync("Your profile has been created");
            return;
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
            Color = _Utils.GetRoleColor(Context),
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
            Color = _Utils.GetRoleColor(Context),
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
            var embed = new EmbedBuilder()
            {
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"For a list of all commands do | p/help all"
                },
                Color = _Utils.GetRoleColor(Context)
            };
            int Guilds = Program._client.Guilds.Count();
            embed.AddField(x =>
            {
                x.Name = ":information_source: Info"; x.Value = "```md" + Environment.NewLine + "<Language C#>" + Environment.NewLine + "<Lib .net 1.0>" + Environment.NewLine + $"<Guilds {Guilds}>" + Environment.NewLine + $"<Uptime_Bots {Services.UptimeBotsList.Count()}>```" + Environment.NewLine + "**Created by**" + Environment.NewLine + "xXBuilderBXx#9113" + Environment.NewLine + "<@190590364871032834>"; x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = ":video_game: Features"; x.Value = "```diff" + Environment.NewLine + "+ Twitch" + Environment.NewLine + "- Youtube" + Environment.NewLine + "+ Minecraft" + Environment.NewLine + "- Vainglory" + Environment.NewLine + "+ Overwatch" + Environment.NewLine + "+ Xbox" + Environment.NewLine + "+ Steam" + Environment.NewLine + "+ Osu```"; x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = ":globe_with_meridians: Links"; x.Value = $"" + Environment.NewLine + "[Website](https://blaze.ml)" + Environment.NewLine + "[Invite Bot](https://goo.gl/GsnmZP)" + Environment.NewLine + "[My Anime List](https://goo.gl/PtGU7C)" + Environment.NewLine + "[Monstercat](https://goo.gl/FgW5sT)" + Environment.NewLine + "[PixelBot Github](https://goo.gl/ORjWNh)" + Environment.NewLine + "[Selfbot Windows](https://goo.gl/c9T9oG)" + Environment.NewLine + "[Selfbot Linux](https://goo.gl/6sotGS)"; x.IsInline = true;
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
        await Context.Channel.SendMessageAsync($":game_die: {Context.User.Username} Rolled a {randomValue}");
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
                    Color = _Utils.GetRoleColor(Context)
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

    [Command("cat")]
    [Remarks("cat")]
    [Summary("Random cat pic/gif")]
    public async Task Cat()
    {
        Console.WriteLine(DateTime.Now.Second);
        WebRequest request = WebRequest.Create("http://random.cat/meow");
        request.Proxy = null;
        WebResponse response = request.GetResponse();
        Stream dataStream = response.GetResponseStream();
        StreamReader reader = new StreamReader(dataStream, System.Text.Encoding.UTF8);
        dynamic Item = Newtonsoft.Json.Linq.JObject.Parse(reader.ReadToEnd());
        var embed = new EmbedBuilder()
        {
            Title = "Random Cat :cat:",
            Url = Item.file,
            ImageUrl = Item.file,
            Color = _Utils.GetRoleColor(Context)
        };
        reader.Close();
        response.Close();
        await Context.Channel.SendMessageAsync("", false, embed);
        Console.WriteLine(DateTime.Now.Second);
    }

    [Command("dog")]
    [Remarks("dog")]
    [Summary("Random dog pic/gif")]
    public async Task Dog()
    {
        string Item = "Item";
        switch (Item)
        {
            case "Item":
                WebRequest request = WebRequest.Create("http://random.dog/woof");
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream, System.Text.Encoding.UTF8);
                 Item = reader.ReadToEnd();
                reader.Close();
                response.Close();
                if (Item.Contains(".mp4"))
                {
                    goto case "Item";
                }
                
                break;
        }
        var embed = new EmbedBuilder()
        {
            Title = "Random Dog :dog:",
            Url = "http://random.dog/" + Item,
            ImageUrl = "http://random.dog/" + Item,
            Color = _Utils.GetRoleColor(Context)
        };
        
        await Context.Channel.SendMessageAsync("", false, embed);
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
        [Command("user")]
        [Alias("u")]
        public async Task VaingloryUser(string Region = "eu", string VGUser = "Builderb")
        {
            if (Region == null || VGUser == null)
            {
                await Context.Channel.SendMessageAsync("`No region or user > p/vg (Region) (User) | na | eu | sa | ea | sg`");
                return;
            }
            if (Region == "na" || Region == "eu" || Region == "sa" || Region == "ea" || Region == "sg")
            {
                try
                {
                    dynamic JA = _Apis._Class_Vainglory.API.GetPlayer(Region, VGUser);
                    dynamic JB = _Apis._Class_Vainglory.API.GetPlayerMatch(Region, VGUser);
                    Console.WriteLine(JB);
                    var embed = new EmbedBuilder()
                    {
                    Title = $"Vainglory | {VGUser}",
                    Description = "```md" + Environment.NewLine + $"<Level {JA.data[0].attributes.stats.level}> <XP {JA.data[0].attributes.stats.xp}> <LifetimeGold {JA.data[0].attributes.stats.lifetimeGold}>" + Environment.NewLine + $"<Wins {JA.data[0].attributes.stats.wins}> <Played {JA.data[0].attributes.stats.played}> <PlayedRank {JA.data[0].attributes.stats.played_ranked}>" + Environment.NewLine + $"<KarmaLevel {JA.data[0].attributes.stats.karmaLevel}> <skillTier {JA.data[0].attributes.stats.skillTier}>```"
                    };
                    await Context.Channel.SendMessageAsync("", false, embed);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    await Context.Channel.SendMessageAsync("`Request error or unknown user`");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("`Unknown region p/vg (Region) (User) | na | eu | sa | ea | sg`");
            }
        }

        [Command("match"), Ratelimit(2, 1, Measure.Minutes)]
        [Alias("m")]
        public async Task VaingloryMatch(string Region, [Remainder] string VGUser)
        {
            
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
            GetUserId.Headers.Add("X-AUTH", Program._Token.Xbox);
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
            OnlineHttp.Headers.Add("X-AUTH", Program._Token.Xbox);
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
            HttpUserGamercard.Headers.Add("X-AUTH", Program._Token.Xbox);
            HttpUserGamercard.Accept = "application/json";
            HttpWebResponse GamercardRes = (HttpWebResponse)HttpUserGamercard.GetResponse();
            Stream GamercardStream = GamercardRes.GetResponseStream();
            StreamReader GamercardRead = new StreamReader(GamercardStream, Encoding.UTF8);
            var GamercardJson = GamercardRead.ReadToEnd();
            dynamic stuff = Newtonsoft.Json.Linq.JObject.Parse(GamercardJson);
            HttpWebRequest HttpUserFrineds = (HttpWebRequest)WebRequest.Create("https://xboxapi.com/v2/" + UserID + "/friends");
            HttpUserFrineds.Method = WebRequestMethods.Http.Get;
            HttpUserFrineds.Headers.Add("X-AUTH", Program._Token.Xbox);
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

    [Command("wot")]
    [Remarks("wot")]
    [Summary("World Of Tanks")]
    public async Task Wot(string Region = "", [Remainder] string User = "")
    {
        if (Region == "" || User == "" || _Apis.WOT.Data.GetRegionUrl(Region) == "null")
        {
            var embed = new EmbedBuilder()
            {
                Description = "**p/wot (Region) (User)**",
                Author = new EmbedAuthorBuilder()
                {
                    Name = "World Of Tanks"
                },
                Color = _Utils.GetRoleColor(Context)
            };
            embed.AddInlineField("Regions", "```md" + Environment.NewLine + "<RU Russia>" + Environment.NewLine + "<EU Europe>" + Environment.NewLine + "<NA America>" + Environment.NewLine + "<AS Asia>```");
            embed.AddInlineField("Stats", "Player data available" + Environment.NewLine + "New features coming soon");
            await Context.Channel.SendMessageAsync("", false, embed);
        }
        else
        {
            try
            {
                string RealRegion = _Apis.WOT.Data.GetRegionUrl(Region);
                string UserID = _Apis.WOT.API.GetUserID(RealRegion, User);
                var Player = _Apis.WOT.API.GetUserData(RealRegion, UserID);
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
                    Color = _Utils.GetRoleColor(Context),
                    Description = "```md" + Environment.NewLine + $"<Battles {Player.Battles}> <Win {Player.Win}> <Loss {Player.Loss}> <Draw {Player.Draws}>" + Environment.NewLine + $"<Shots {Player.Shots}> <Hits {Player.Hits}> <Miss {Convert.ToUInt32(Player.Shots) - Convert.ToUInt32(Player.Hits)}>```More stats coming soon this is a test"
                };
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await Context.Channel.SendMessageAsync("`Cannot not find user`");
            }
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



    [Command("pokerev")]
    [Remarks("pokerev")]
    [Summary("Pokemon revolution red stats")]
    public async Task Pokerev()
    {
            List<EmbedBuilder> EmbedList = new List<EmbedBuilder>();
            List<string> List1 = _Apis.Poke.PokemonRevolution.GetMainTable("https://pokemon-revolution-online.net/ladder.php", "auto-style2", 1);
            var embed1 = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = "RED | Pokémon Revolution Online",
                    Url = "https://pokemon-revolution-online.net"
                },
                Description = ":crossed_swords: Ranked",
                Footer = new EmbedFooterBuilder()
                {
                    Text = "| Non-Ranked =>"
                }
            };
            embed1.AddInlineField("Rank | User", $"{List1[0]}. {List1[1]}" + Environment.NewLine + $"{List1[5]}. {List1[6]}" + Environment.NewLine + $"{List1[10]}. {List1[11]}" + Environment.NewLine + $"{List1[15]}. {List1[16]}" + Environment.NewLine + $"{List1[20]}. {List1[21]}" + Environment.NewLine + $"{List1[25]}. {List1[26]}" + Environment.NewLine + $"{List1[30]}. {List1[31]}" + Environment.NewLine + $"{List1[35]}. {List1[36]}" + Environment.NewLine + $"{List1[40]}. {List1[41]}" + Environment.NewLine + $"{List1[45]}. {List1[46]}");
            embed1.AddInlineField("Win/Loss", $"{List1[3]}/{List1[4]}" + Environment.NewLine + $"{List1[8]}/{List1[9]}" + Environment.NewLine + $"{List1[13]}/{List1[14]}" + Environment.NewLine + $"{List1[18]}/{List1[19]}" + Environment.NewLine + $"{List1[23]}/{List1[24]}" + Environment.NewLine + $"{List1[28]}/{List1[29]}" + Environment.NewLine + $"{List1[33]}/{List1[34]}" + Environment.NewLine + $"{List1[38]}/{List1[39]}" + Environment.NewLine + $"{List1[43]}/{List1[44]}" + Environment.NewLine + $"{List1[48]}/{List1[49]}");
            embed1.AddInlineField("Rating", $"{List1[2]}" + Environment.NewLine + $"{List1[7]}" + Environment.NewLine + $"{List1[12]}" + Environment.NewLine + $"{List1[17]}" + Environment.NewLine + $"{List1[22]}" + Environment.NewLine + $"{List1[27]}" + Environment.NewLine + $"{List1[32]}" + Environment.NewLine + $"{List1[37]}" + Environment.NewLine + $"{List1[42]}" + Environment.NewLine + $"{List1[47]}");
            List1.Clear();
            List1 = _Apis.Poke.PokemonRevolution.GetMainTable("https://pokemon-revolution-online.net/ladder.php", "auto-style2", 2);
            var embed2 = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = "RED | Pokémon Revolution Online",
                    Url = "https://pokemon-revolution-online.net"
                },
                Description = ":shield: Non-Ranked",
                Footer = new EmbedFooterBuilder()
                {
                    Text = "<= Non-Ranked | Playtime =>"
                }
            };
            embed2.AddInlineField("Rank | User", $"{List1[0]}. {List1[1]}" + Environment.NewLine + $"{List1[5]}. {List1[6]}" + Environment.NewLine + $"{List1[10]}. {List1[11]}" + Environment.NewLine + $"{List1[15]}. {List1[16]}" + Environment.NewLine + $"{List1[20]}. {List1[21]}" + Environment.NewLine + $"{List1[25]}. {List1[26]}" + Environment.NewLine + $"{List1[30]}. {List1[31]}" + Environment.NewLine + $"{List1[35]}. {List1[36]}" + Environment.NewLine + $"{List1[40]}. {List1[41]}" + Environment.NewLine + $"{List1[45]}. {List1[46]}");
            embed2.AddInlineField("Win/Loss", $"{List1[2]}/{List1[3]}" + Environment.NewLine + $"{List1[7]}/{List1[8]}" + Environment.NewLine + $"{List1[12]}/{List1[13]}" + Environment.NewLine + $"{List1[17]}/{List1[18]}" + Environment.NewLine + $"{List1[22]}/{List1[23]}" + Environment.NewLine + $"{List1[27]}/{List1[28]}" + Environment.NewLine + $"{List1[32]}/{List1[33]}" + Environment.NewLine + $"{List1[37]}/{List1[38]}" + Environment.NewLine + $"{List1[42]}/{List1[43]}" + Environment.NewLine + $"{List1[47]}/{List1[48]}");
            List1.Clear();
            List1 = _Apis.Poke.PokemonRevolution.GetPlaytimeTable("https://pokemon-revolution-online.net/ladder.php", "auto-style2", 3);
            var embed3 = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = "RED | Pokémon Revolution Online",
                    Url = "https://pokemon-revolution-online.net"
                },
                Description = ":stopwatch: Playtime:",
                Footer = new EmbedFooterBuilder()
                {
                    Text = "<= Non-Ranked |"
                }
            };
            embed3.AddInlineField("Rank | User", $"{List1[0]}. {List1[1]}" + Environment.NewLine + $"{List1[3]}. {List1[4]}" + Environment.NewLine + $"{List1[6]}. {List1[7]}" + Environment.NewLine + $"{List1[9]}. {List1[10]}" + Environment.NewLine + $"{List1[12]}. {List1[13]}" + Environment.NewLine + $"{List1[15]}. {List1[16]}" + Environment.NewLine + $"{List1[18]}. {List1[19]}" + Environment.NewLine + $"{List1[21]}. {List1[22]}" + Environment.NewLine + $"{List1[24]}. {List1[25]}" + Environment.NewLine + $"{List1[27]}. {List1[28]}");
            embed3.AddInlineField("Playtime", $"{List1[2]}" + Environment.NewLine + $"{List1[5]}" + Environment.NewLine + $"{List1[8]}" + Environment.NewLine + $"{List1[11]}" + Environment.NewLine + $"{List1[14]}" + Environment.NewLine + $"{List1[17]}" + Environment.NewLine + $"{List1[20]}" + Environment.NewLine + $"{List1[23]}" + Environment.NewLine + $"{List1[26]}" + Environment.NewLine + $"{List1[29]}");
            EmbedList.Add(embed1);
            EmbedList.Add(embed2);
            EmbedList.Add(embed3);
            PaginationService.Min.Message message = new PaginationService.Min.Message(EmbedList, "Test", _Utils.GetRoleColor(Context), Context.User);
            await Program._PagMin.SendPagMessageAsync(Context.Channel, message);
        
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
            
            
            SteamIdentity SteamUser = null;
            SteamWebAPI.SetGlobalKey(Program._Token.Steam);
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
            HttpWebRequest GetUserId = (HttpWebRequest)WebRequest.Create("https://osu.ppy.sh/api/get_user?k=" + Program._Token.Osu + "&u=" + User);
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
        var client = new TwitchAuthenticatedClient(Program._Token.Twitch, Program._Token.TwitchAuth);
        var Usearch = client.SearchChannels(Search).List;
        var embed = new EmbedBuilder()
        {
            Title = "Twitch Channels",
            Description = $"{Usearch[0].Name} | {Usearch[1].Name} | {Usearch[2].Name}",
            Color = _Utils.GetRoleColor(Context)
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
            Color = _Utils.GetRoleColor(Context)
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
        var client = new TwitchAuthenticatedClient(Program._Token.Twitch, Program._Token.TwitchAuth);
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
    [Summary("Recieve notifications from a twitch channel")]
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
        var client = new TwitchAuthenticatedClient(Program._Token.Twitch, Program._Token.TwitchAuth);
        var t = client.GetChannel(Channel);
        if (t.CreatedAt.Year == 0001)
        {
            await Context.Channel.SendMessageAsync("`Cannot find this channel`");
            return;
        }
        if (Option == "me")
        {
            if (Services.TwitchNotifications.Exists(x => x.Twitch == Channel.ToLower() & x.User == Context.User.Id & x.Type == "user"))
            {
                await Context.Channel.SendMessageAsync($"`You already have {Channel} listed`");
                return;
            }
            Services.TwitchNotifyClass NewNotif = new Services.TwitchNotifyClass()
            {
                Type = "user",
                Guild = Context.Guild.Id,
                Channel = Context.Channel.Id,
                User = Context.User.Id,
                Twitch = Channel.ToLower(),
                Live = false
            };
            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter file = File.CreateText(Program.BotPath + $"Twitch\\user-{Context.User.Id}-{Channel.ToLower()}.json"))
            {
                serializer.Serialize(file, NewNotif);
            }
            Services.TwitchNotifications.Add(NewNotif);
                await Context.Channel.SendMessageAsync($"`You added notifications for {Channel} to yourself`");
            
        }
        else if (Option == "here" || Option == "channel")
        {
            if (Context.User.Id != Context.Guild.OwnerId)
            {
                await Context.Channel.SendMessageAsync("`You are not the guild owner`");
                return;
            }
            if (Services.TwitchNotifications.Exists(x => x.Twitch == Channel.ToLower() & x.Guild == Context.Guild.Id & x.Type == "channel" & x.Channel == Context.Channel.Id))
            {
                await Context.Channel.SendMessageAsync($"`This channel already has {Channel} listed`");
                return;
            }
            Services.TwitchNotifyClass NewNotif = new Services.TwitchNotifyClass()
            {
                Type = "channel",
                Guild = Context.Guild.Id,
                Channel = Context.Channel.Id,
                User = Context.User.Id,
                Twitch = Channel.ToLower(),
                Live = false
            };
            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter file = File.CreateText(Program.BotPath + $"Twitch\\channel-{Context.Guild.Id.ToString()}-{Context.Channel.Id}-{Channel.ToLower()}.json"))
            {
                serializer.Serialize(file, NewNotif);
            }
            Services.TwitchNotifications.Add(NewNotif);
            await Context.Channel.SendMessageAsync($"`You added notifications for {Channel} to this channel`");
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
            List<string> TWList = Services.TwitchNotifications.Where(x => x.User == Context.User.Id & x.Type == "user").Select(x => x.Twitch).ToList();
            var embed = new EmbedBuilder()
            { Title = "Twitch Notifications For You", Description = string.Join(", ", TWList), Color = _Utils.GetRoleColor(Context) };
            await Context.Channel.SendMessageAsync("", false, embed);
        }
        if (Option == "guild")
        {
            List<string> TWList = Services.TwitchNotifications.Where(x => x.Guild == Context.Guild.Id & x.Type == "channel").Select(x => x.Twitch).ToList();
            var embed = new EmbedBuilder()
            { Title = "Twitch Notifications For This Guild", Description = string.Join(", ", TWList), Color = _Utils.GetRoleColor(Context) };
            await Context.Channel.SendMessageAsync("", false, embed);
        }
        if (Option == "here")
        {
            List<string> TWList = Services.TwitchNotifications.Where(x => x.Channel == Context.Channel.Id & x.Type == "channel").Select(x => x.Twitch).ToList();
            var embed = new EmbedBuilder()
            {
                Title = $"Twitch Notifications For #{Context.Channel.Name}",
                Description = string.Join(", ", TWList),
                Color = _Utils.GetRoleColor(Context)
            };
            await Context.Channel.SendMessageAsync("", false, embed);
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
            if (Services.TwitchNotifications.Exists(x => x.Type == "user" & x.Twitch == Channel.ToLower() & x.User == Context.User.Id))
            {
                    Services.TwitchNotifications.RemoveAll(x => x.Type == "user" & x.Twitch == Channel.ToLower() & x.User == Context.User.Id);
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
            if (Services.TwitchNotifications.Exists(x => x.Type == "channel" & x.Twitch == Channel.ToLower() & x.Guild == Context.Guild.Id & x.Channel == Context.Channel.Id))
            {
                    Services.TwitchNotifications.RemoveAll(x => x.Type == "channel" & x.Twitch == Channel & x.Guild == Context.Guild.Id & x.Channel == Context.Channel.Id);
                
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

        if (Context.Channel is IPrivateChannel || Option == "all")
        {
            if (Option == "all")
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Username} I have sent you a full list of commands");
            }
            var allemebed = new EmbedBuilder()
            {
                Title = "Commands List",
                Color = _Utils.GetRoleColor(Context)
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
            IGuildUser BotUser = null;
            _Utils.GuildBotCache.TryGetValue(Context.Guild.Id, out BotUser);
            string HelpText = "```md" + Environment.NewLine + "[ p/misc ]( Info/Dice Roll )" + Environment.NewLine + "[ p/game ]( Steam/Minecraft )" + Environment.NewLine + "[ p/media ]( Twitch )" + Environment.NewLine + "[ p/prune ]( Prune Messages )```";
            if (!BotUser.GetPermissions(Context.Channel as ITextChannel).EmbedLinks)
            {
                await Context.Channel.SendMessageAsync(HelpText);
                return;
            }
            string PermReact = "Add Reactions :x:";
            string PermManage = "Manage Messages :x:";
            var embed = new EmbedBuilder()
            {
            };
            embed.AddInlineField("Commands list", HelpText + Environment.NewLine + "For a list of all the bot commands do **p/help all** | " + Environment.NewLine + "or visit the website **p/website**");
            embed.AddInlineField("Interactive Help", "For an interactive help menu" + Environment.NewLine + "Add these permissions" + Environment.NewLine + Environment.NewLine + PermReact + Environment.NewLine + Environment.NewLine + "(Optional)" + Environment.NewLine + PermManage);
            if (!BotUser.GetPermissions(Context.Channel as ITextChannel).AddReactions || !BotUser.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                
                if (BotUser.GetPermissions(Context.Channel as ITextChannel).AddReactions)
                {
                    PermReact = "Add Reactions :white_check_mark: ";
                }
                if (BotUser.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
                {
                    PermManage = "Manage Messages :white_check_mark: ";
                }
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
            await _Utils.SendPaginator(pages, "Commands List", Context, embed);
        }
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
        await Context.Channel.SendMessageAsync("Prefix is `p/` e.g **p/help**");
    }

    [Command("website")]
    public async Task Website()
    {
        var embed = new EmbedBuilder()
        {
            Description = ":globe_with_meridians: [Website](https://blaze.ml)" + Environment.NewLine + "For more info/links do **p/bot**",
            Color = _Utils.GetRoleColor(Context)
        };
        await Context.Channel.SendMessageAsync("", false, embed);
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
        SteamWebAPI.SetGlobalKey(Program._Token.Steam);
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
        }
        else
        {
            await Context.Channel.SendMessageAsync("`Account not claimed`");
        }
    }
}