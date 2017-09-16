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
    public class Main : ModuleBase
    {
        private DiscordSocketClient _Client;
        public Main(DiscordSocketClient client)
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

        [Command("cat")]
       public async Task Cat()
        {
            var Check = _Utils_Http.GetJsonObject("http://random.cat/meow");
            if (Check.Success == false)
            {
                await ReplyAsync($"`{Check.Error}");
                return;
            }
            var embed = new EmbedBuilder()
            {
                Title = "Random Cat :cat:",
                Url = Check.Json.file,
                ImageUrl = Check.Json.file,
                Color = _Utils_Discord.GetRoleColor(Context.Channel)
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("dog")]
        public async Task Dog()
        {
            var Check = _Utils_Http.GetString("http://random.dog/woof");
            if (Check.Success == false)
            {
                await ReplyAsync($"`{Check.Error}");
                return;
            }
            var embed = new EmbedBuilder()
            {
                Title = "Random Dog :dog:",
                Url = "http://random.dog/" + Check.Content,
                ImageUrl = "http://random.dog/" + Check.Content,
                Color = _Utils_Discord.GetRoleColor(Context.Channel)
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("math"), Alias("calc")]
        public async Task Math([Remainder] string Math)
        {
            var interpreter = new DynamicExpresso.Interpreter();
            var result = interpreter.Eval(Math);
            await Context.Channel.SendMessageAsync("```md" + Environment.NewLine + $"< {Math} = {result.ToString()} >");
        }

        [Command("discrim"), Remarks("discrim (0000)"), Summary("list of guild and global user with a discrim")]
        public async Task Discrim(int Discrim = 0, string Option = "", string Option2 = "")
        {
            if (Discrim == 0)
            {
                await Context.Channel.SendMessageAsync($"`You can search for a discrim using p/discrim {Context.User.Discriminator} guild/global | The guild or global is optional`");
                return;
            }
            if (Discrim == 0000)
            {
                await Context.Channel.SendMessageAsync($"`0000 is a prefix only used by webhooks and not users`");
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

            Dictionary<ulong, string> DiscrimList = new Dictionary<ulong, string>();
            if (Context.Channel is IPrivateChannel)
            {
                foreach (var User in GuildUsers.Where(x => x.DiscriminatorValue == Discrim))
                {
                    string UserID = "";
                    if (Option == "id" || Option2 == "id")
                    {
                        UserID = $"= {User.Id.ToString()} ";
                    }
                    if (!DiscrimList.Keys.Contains(User.Id) && User.Id != Context.User.Id)
                    {
                        DiscrimList.Add(User.Id, $"< {User.Username}#{User.Discriminator}".PadRight(40) + $"{UserID}>");
                    }
                }
            }
            else
            {
                if (Option.ToLower() != "global")
                {
                    foreach (var GuildUser in GuildUsers.Where(x => x.GuildId == Context.Guild.Id))
                    {
                        string UserID = "";
                        if (Option == "id" || Option2 == "id")
                        {
                            UserID = $"= {GuildUser.Id.ToString()} ";
                        }
                        if (!DiscrimList.Keys.Contains(GuildUser.Id) && GuildUser.DiscriminatorValue == Discrim)
                        {
                            if (GuildUser.Id == Context.User.Id)
                            {
                                DiscrimList.Add(GuildUser.Id, $@"<You         {GuildUser.Username}#{GuildUser.Discriminator}".PadRight(40) + $"{UserID}>");
                            }
                            else
                            {
                                if (GuildUser.IsBot)
                                {
                                    DiscrimList.Add(GuildUser.Id, $@"<Guild-Bot   {GuildUser.Username}#{GuildUser.Discriminator}".PadRight(40) + $"{UserID}>");
                                }
                                else
                                {
                                    DiscrimList.Add(GuildUser.Id, $@"<Guild       {GuildUser.Username}#{GuildUser.Discriminator}".PadRight(40) + $"{UserID}>");
                                }
                            }
                        }
                    }
                }
                if (Option.ToLower() != "guild")
                {
                    foreach (var GuildUser in GuildUsers.Where(x => x.GuildId != Context.Guild.Id))
                    {
                        string UserID = "";
                        if (Option == "id" || Option2 == "id")
                        {
                            UserID = $"= {GuildUser.Id.ToString()} ";
                        }
                        if (!DiscrimList.Keys.Contains(GuildUser.Id) && GuildUser.DiscriminatorValue == Discrim)
                        {
                            if (GuildUser.Id == Context.User.Id)
                            {
                                DiscrimList.Add(GuildUser.Id, $@"<You         {GuildUser.Username}#{GuildUser.Discriminator}".PadRight(40) + $"{UserID}>");
                            }
                            else
                            {
                                if (GuildUser.IsBot)
                                {
                                    DiscrimList.Add(GuildUser.Id, $@"<Global-Bot  {GuildUser.Username}#{GuildUser.Discriminator}".PadRight(40) + $"{UserID}>");
                                }
                                else
                                {
                                    DiscrimList.Add(GuildUser.Id, $@"<Global      {GuildUser.Username}#{GuildUser.Discriminator}".PadRight(40) + $"{UserID}>");
                                }
                            }
                        }
                    }
                }
            }

            if (DiscrimList.Values.Count == 0)
            {
                await Context.Channel.SendMessageAsync($"`Could not find any users with the discrim {Discrim}`");
            }
            else
            {
                string Users = string.Join(Environment.NewLine, DiscrimList.Values.ToArray());
                await Context.Channel.SendMessageAsync($"Found **{DiscrimList.Values.Count}** users with the discrim {Discrim}" + Environment.NewLine + "```md" + Environment.NewLine + Users + "```Options > id | guild | global");
            }
        }

        [Command("guild"), Remarks("guild"), Summary("Info about the guild | Owner/Roles")]
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
                var UserDM = await User.GetOrCreateDMChannelAsync();
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
                Color = _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel),
                Description = $"Owner: {Owner.Mention}```md" + Environment.NewLine + $"[Online](Offline)" + Environment.NewLine + $"<Users> [{MembersOnline}]({Members}) <Bots> [{BotsOnline}]({Bots})" + Environment.NewLine + $"Channels <Text {TextChan}> <Voice {VoiceChan}>" + Environment.NewLine + $"<Roles {Context.Guild.Roles.Count}> <Region {Context.Guild.VoiceRegionId}>" + Environment.NewLine + "List of roles | p/guild roles```",
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Created {Context.Guild.CreatedAt.Date.Day} {Context.Guild.CreatedAt.Date.DayOfWeek} {Context.Guild.CreatedAt.Year}"
                }
            };
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("roll"), Remarks("roll"), Summary("Roll the dice!"), Alias("dice")]
        public async Task Roll()
        {
            var random = new Random((int)DateTime.Now.Ticks); var randomValue = random.Next(1, 7);
            await Context.Channel.SendMessageAsync($":game_die: {Context.User.Username} Rolled a {randomValue}");
        }

        [Command("flip"), Remarks("flip"), Summary("Flip a coin!"), Alias("coin")]
        public async Task Flip()
        {
            var random = new Random((int)DateTime.Now.Ticks); var randomValue = random.Next(1, 3);
            if (randomValue == 1)
            { await Context.Channel.SendMessageAsync($"{Context.User.Username} Flipped Heads"); }
            else
            { await Context.Channel.SendMessageAsync($"{Context.User.Username} Flipped Tails"); }
        }
        [Command("user"), Remarks("user (@Mention/User ID)"), Summary("Info about a user")]
        public async Task User(string User = "")
        {
            IGuildUser GuildUser = null;
            string NotInGuild = " - Not In This Guild";
            int Count = 0;
            if (User == "")
            {
                User = Context.User.Id.ToString();
            }
            if (User.StartsWith("<@"))
            {
                User = User.Replace("<@", "").Replace(">", "");
                if (User.Contains("!"))
                {
                    User = User.Replace("!", "");
                }
            }

            foreach (var Guild in _Client.Guilds)
            {
                IGuildUser FindUser = Guild.GetUser(Convert.ToUInt64(User));
                if (FindUser != null)
                {
                    Count++;
                    if (Guild.Id == Context.Guild.Id)
                    {
                        NotInGuild = "";
                    }
                    if (GuildUser == null)
                    {
                        GuildUser = FindUser;
                    }
                }

            }

            if (GuildUser == null)
            {
                await Context.Channel.SendMessageAsync($"`Could not find user in {_Client.Guilds.Count} guilds`").ConfigureAwait(false);
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
                Color = _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel),
                Description = $"<@{GuildUser.Id}>{NotInGuild}" + Environment.NewLine + "```md" + Environment.NewLine + $"<Discrim {GuildUser.Discriminator}> <ID {GuildUser.Id}>" + Environment.NewLine + $"<Joined_Guild {GuildUser.JoinedAt.Value.Day} {GuildUser.JoinedAt.Value.Date.ToString("MMMM")} {GuildUser.JoinedAt.Value.Year}>" + Environment.NewLine + $"<Created_Account {GuildUser.CreatedAt.Day} {GuildUser.CreatedAt.DateTime.ToString("MMMM")} {GuildUser.CreatedAt.Year}>" + Environment.NewLine + $"Found in {Count} guilds```",
                Footer = new EmbedFooterBuilder()
                { Text = "To lookup a discrim use | p/discrim 0000" }
            };
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("bot"), Remarks("bot"), Summary("Info about this bot | Owner/Websites/Stats")]
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
                    Color = _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel)
                };
                int Guilds = (await Context.Client.GetGuildsAsync().ConfigureAwait(false)).Count();
                embed.AddField(x =>
                {
                    x.Name = ":information_source: Info"; x.Value = "```md" + Environment.NewLine + "<Language C#>" + Environment.NewLine + "<Lib .net 1.0>" + Environment.NewLine + $"<Guilds {Guilds}>```" + Environment.NewLine + "**Created by**" + Environment.NewLine + "xXBuilderBXx#9113" + Environment.NewLine + "<@190590364871032834>"; x.IsInline = true;
                });
                embed.AddField(x =>
                {
                    x.Name = ":video_game: Features"; x.Value = "```diff" + Environment.NewLine + "+ Twitch" + Environment.NewLine + "- Youtube" + Environment.NewLine + "+ Minecraft" + Environment.NewLine + "- Vainglory" + Environment.NewLine + "+ Overwatch" + Environment.NewLine + "+ Xbox" + Environment.NewLine + "+ Steam" + Environment.NewLine + "+ Osu```"; x.IsInline = true;
                });
                embed.AddField(x =>
                {
                    x.Name = ":globe_with_meridians: Links"; x.Value = $"" + Environment.NewLine + "[Website](https://blazeweb.ml)" + Environment.NewLine + "[Invite Bot](https://goo.gl/GsnmZP)" + Environment.NewLine + "[My Anime List](https://goo.gl/PtGU7C)" + Environment.NewLine + "[Monstercat](https://goo.gl/FgW5sT)" + Environment.NewLine + "[PixelBot Github](https://goo.gl/ORjWNh)" + Environment.NewLine + "[Selfbot Windows](https://goo.gl/c9T9oG)" + Environment.NewLine + "[Selfbot Linux](https://goo.gl/6sotGS)"; x.IsInline = true;
                });
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

    }
    
    public class Mod : ModuleBase
    {
        [Command("ban")]
        public async Task Ban(string User = "", string Reason = "")
        {
            if (Context.Guild == null)
            {
                await ReplyAsync("`Cannot use command in DMs`");
                return;
            }
            if (User == "")
            {
                await ReplyAsync("`Kick a user with p/hackban (@Mention/ID)`");
                return;
            }
            _Bot.GuildCache.TryGetValue(Context.Guild.Id, out _CacheItem CI);
            if (!CI.Bot.GuildPermissions.BanMembers)
            {
                await ReplyAsync("`Bot does not have permission to ban user`");
                return;
            }
            IGuildUser GuildUser = await _Utils_Discord.MentionGetUser(Context.Guild, User);
            if (GuildUser == null)
            {
                await ReplyAsync("`Could not find user`");
                return;
            }
            if (GuildUser.Id == Context.Guild.OwnerId)
            {
                await ReplyAsync("`Cannot ban the guild owner silly :/`");
                return;
            }
            IEnumerable<IRole> GuildRoles = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id);
            if (GuildRoles.Count() == 0)
            {
                await ReplyAsync("`This guild has no roles`");
                return;
            }
            if (CI.Bot.RoleIds.Where(x => x != Context.Guild.EveryoneRole.Id).Count() == 0)
            {
                await ReplyAsync("`Bot does not have any roles`");
                return;
            }

            IRole BotRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && CI.Bot.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();
            IRole UserRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && GuildUser.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();
            if (BotRole.Position < UserRole.Position + 1)
            {
                await ReplyAsync("`Bot cannot ban user with same or lower role`");
                return;
            }
            if (Context.User.Id == Context.Guild.OwnerId)
            {
                await Context.Guild.AddBanAsync(GuildUser.Id, 0, $"[{Context.User.Username}]" + Reason);
                if (CI.Bot.GuildPermissions.ManageMessages)
                {
                    await Context.Message.DeleteAsync();
                }
                await ReplyAsync($"`{Context.User.Username} has banned {GuildUser.Username}#{GuildUser.Discriminator}`");
                return;
            }
            IGuildUser GuildStaff = Context.User as IGuildUser;
            if (!GuildStaff.GuildPermissions.KickMembers)
            {
                await ReplyAsync("`You do not have permission to ban user`");
                return;
            }
            
            if (GuildStaff.RoleIds.Where(x => x != Context.Guild.EveryoneRole.Id).Count() == 0)
            {
                await ReplyAsync("`You do not have any roles`");
                return;
            }

            IRole StaffRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && GuildStaff.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();
            if (StaffRole.Position < UserRole.Position + 1)
            {
                await ReplyAsync("`You cannot kick user with same or lower role than you`");
                return;
            }
            await Context.Guild.AddBanAsync(GuildUser.Id, 0, $"[{Context.User.Username}]" + Reason);
            if (CI.Bot.GuildPermissions.ManageMessages)
            {
                await Context.Message.DeleteAsync();
            }
            await ReplyAsync($"`{Context.User.Username} has banned {GuildUser.Username}#{GuildUser.Discriminator}`");
        }

        [Command("kick")]
        public async Task Kick(string User = "", string Reason = "")
        {
            if (Context.Guild == null)
            {
                await ReplyAsync("`Cannot use command in DMs`");
                return;
            }
            if (User == "")
            {
                await ReplyAsync("`Kick a user with p/hackban (@Mention/ID)`");
                return;
            }
            _Bot.GuildCache.TryGetValue(Context.Guild.Id, out _CacheItem CI);
            if (!CI.Bot.GuildPermissions.KickMembers)
            {
                await ReplyAsync("`Bot does not have permission to kick user`");
                return;
            }
            IGuildUser GuildUser = await _Utils_Discord.MentionGetUser(Context.Guild, User);
            if (GuildUser == null)
            {
                await ReplyAsync("`Could not find user`");
                return;
            }
            if (GuildUser.Id == Context.Guild.OwnerId)
            {
                await ReplyAsync("`Cannot ban the guild owner silly :/`");
                return;
            }
            IEnumerable<IRole> GuildRoles = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id);
            if (GuildRoles.Count() == 0)
            {
                await ReplyAsync("`This guild has no roles`");
                return;
            }
            if (CI.Bot.RoleIds.Where(x => x != Context.Guild.EveryoneRole.Id).Count() == 0)
            {
                await ReplyAsync("`Bot does not have any roles`");
                return;
            }
            IRole BotRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && CI.Bot.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();
            IRole UserRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && GuildUser.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();
            if (BotRole.Position < UserRole.Position + 1)
            {
                await ReplyAsync("`Bot cannot kick user with same or lower role`");
                return;
            }
            if (Context.User.Id == Context.Guild.OwnerId)
            {
                await GuildUser.KickAsync($"[{Context.User.Username}]" + Reason);
                if (CI.Bot.GuildPermissions.ManageMessages)
                {
                    await Context.Message.DeleteAsync();
                }
                await ReplyAsync($"`{Context.User.Username} has kicked {GuildUser.Username}#{GuildUser.Discriminator}`");
                return;
            }
            IGuildUser GuildStaff = Context.User as IGuildUser;
            if (!GuildStaff.GuildPermissions.KickMembers)
            {
                await ReplyAsync("`You do not have permission to kick user`");
                return;
            }
            if (GuildStaff.RoleIds.Where(x => x != Context.Guild.EveryoneRole.Id).Count() == 0)
            {
                await ReplyAsync("`You do not have any roles`");
                return;
            }
            IRole StaffRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && GuildStaff.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();
            if (StaffRole.Position < UserRole.Position + 1)
            {
                await ReplyAsync("`You cannot kick user with same or lower role than you`");
                return;
            }
            if (CI.Bot.GuildPermissions.ManageMessages)
            {
                await Context.Message.DeleteAsync();
            }
            await GuildUser.KickAsync($"[{Context.User.Username}]" + Reason);
            await ReplyAsync($"`{Context.User.Username} has kicked {GuildUser.Username}#{GuildUser.Discriminator}`");
        }

        [Command("hackban")]
        public async Task Hackban(ulong User = 0, string Reason = "")
        {
            if (Context.Guild == null)
            {
                await ReplyAsync("`Cannot use command in DMs`");
                return;
            }
            if (User == 0)
            {
                await ReplyAsync("`Hackban a user id with p/hackban (ID)`");
                return;
            }
            _Bot.GuildCache.TryGetValue(Context.Guild.Id, out _CacheItem CI);
            if (!CI.Bot.GuildPermissions.BanMembers)
            {
                await ReplyAsync("`Bot does not have permission to ban user`");
                return;
            }
            IGuildUser GuildUser = await _Utils_Discord.MentionGetUser(Context.Guild, User.ToString());
            if (GuildUser != null)
            {
                await ReplyAsync("`This user is in the guild please user p/ban (User) (Reason)`");
                return;
            }
            if (GuildUser.Id == Context.Guild.OwnerId)
            {
                await ReplyAsync("`Cannot ban the guild owner silly :/`");
                return;
            }
            IEnumerable<IRole> GuildRoles = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id);
            if (GuildRoles.Count() == 0)
            {
                await ReplyAsync("`This guild has no roles`");
                return;
            }
            if (CI.Bot.RoleIds.Where(x => x != Context.Guild.EveryoneRole.Id).Count() == 0)
            {
                await ReplyAsync("`Bot does not have any roles`");
                return;
            }

            IRole BotRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && CI.Bot.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();

            if (Context.User.Id == Context.Guild.OwnerId)
            {
                await Context.Guild.AddBanAsync(User, 0, $"[{Context.User.Username}]" + Reason);
                if (CI.Bot.GuildPermissions.ManageMessages)
                {
                    await Context.Message.DeleteAsync();
                }
                await ReplyAsync($"`{Context.User.Username} has banned id {User}`");
                return;
            }
            IGuildUser GuildStaff = Context.User as IGuildUser;
            if (!GuildStaff.GuildPermissions.KickMembers)
            {
                await ReplyAsync("`You do not have permission to ban user`");
                return;
            }

            if (GuildStaff.RoleIds.Where(x => x != Context.Guild.EveryoneRole.Id).Count() == 0)
            {
                await ReplyAsync("`You do not have any roles`");
                return;
            }

            IRole StaffRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && GuildStaff.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();
            
            await Context.Guild.AddBanAsync(User, 0, $"[{Context.User.Username}]" + Reason);
            if (CI.Bot.GuildPermissions.ManageMessages)
            {
                await Context.Message.DeleteAsync();
            }
            await ReplyAsync($"`{Context.User.Username} has banned id {User}`");
        }

        [Group("prune")]
        [Alias("purge", "tidy", "clean")]
        public class PruneGroup : ModuleBase
        {
            private readonly TimeSpan twoWeeks = TimeSpan.FromDays(14);
            private readonly PruneService _prune;
            private CommandService _Commands;
            public PruneGroup(PruneService prune, CommandService Commands)
            {
                _prune = prune;
                _Commands = Commands;
            }
            [Command]
            public async Task Prune()
            {
                List<string> CommandList = new List<string>();
                foreach (var CMD in _Commands.Commands.Where(x => x.Module.Name == "prune"))
                {
                    CommandList.Add($"[ p/{CMD.Remarks} ][ {CMD.Summary} ]");
                }
                string Commands = string.Join(Environment.NewLine, CommandList);
                await Context.Channel.SendMessageAsync("Prune Commands```md" + Environment.NewLine + Commands + "```");
            }

            [Command("all"),Remarks("prune all (Ammount)"),Summary("Prune all messages | Not pinned")]
            public async Task Pruneall(int Ammount = 100)
            {
                IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id).ConfigureAwait(false);
                if (!Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
                {
                    await Context.Channel.SendMessageAsync("`Bot does not have permission to manage messages");
                    return;
                }
                await Context.Message.DeleteAsync().ConfigureAwait(false);
                var GuildUser = Context.User as IGuildUser;
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
                if (Ammount > 100)
                {
                    await Context.Channel.SendMessageAsync("`Ammount cannot be more than 30`");
                    return;
                }
                await _prune.PruneWhere((ITextChannel)Context.Channel, Ammount, (x) => !x.IsPinned).ConfigureAwait(false);
                await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted all messages (Not Pinned)`").ConfigureAwait(false);
            }

            [Command("user"),[Remarks("prune user (@Mention/User ID) (Ammount)"),Summary("Prune messages made by thi user")]
            public async Task Pruneuser(string User = "", int Ammount = 30)
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
                var user = await Context.Guild.GetUserAsync(Convert.ToUInt64(Utils._Utils_Discord.MentionToID(User))).ConfigureAwait(false);
                await _prune.PruneWhere((ITextChannel)Context.Channel, Ammount, (x) => x.Author.Id == user.Id).ConfigureAwait(false);
                await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted user {user.Username} messages`");
            }

            [Command("bot"),Alias("bots"),Remarks("prune bot (Ammount)"),Summary("Prune messages made by bots")]
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
                await _prune.PruneWhere((ITextChannel)Context.Channel, Ammount, (x) => x.Author.IsBot).ConfigureAwait(false);
                await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted bot messages`");
            }

            [Command("image"),Alias("images"),Remarks("prune image (Ammount)"),Summary("Prune messages that have an attachment")]
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
                await _prune.PruneWhere((ITextChannel)Context.Channel, Ammount, (x) => x.Attachments.Count != 0).ConfigureAwait(false);
                await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted images`");
            }

            [Command("embed"),Alias("embeds"),Remarks("prune embed (Ammount)"),Summary("Prune messages that have an embed")]
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
                await _prune.PruneWhere((ITextChannel)Context.Channel, Ammount, (x) => x.Embeds.Count != 0).ConfigureAwait(false);
                await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted embeds`");
            }

            [Command("link"),Alias("links"),Remarks("prune link (Ammount)"),Summary("Prune messages that have a link")]
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
                await _prune.PruneWhere((ITextChannel)Context.Channel, Ammount, (x) => x.Content.Contains("http://") | x.Content.Contains("https://")).ConfigureAwait(false);
                await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted links`");
            }

            [Command("text"),Remarks("prune (Text Here)"),Summary("Prune messages that contain this text")]
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
                await _prune.PruneWhere((ITextChannel)Context.Channel, 100, (x) => x.Content.Contains(Text)).ConfigureAwait(false);
                await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted messages that contain ({Text})`");
            }
        }
    }

    public class Game : ModuleBase
    {
        private PaginationFull _PagFull;
        public Game(CommandService commandservice, PaginationFull pagfull)
        {
            _PagFull = pagfull;
        }

        [Command("xbox"), Remarks("xbox (User)"), Summary("Xbox live user stats")]
        public async Task Xboxuser([Remainder] string User)
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
                GetUserId.Headers.Add("X-AUTH", _Config.Tokens.Xbox);
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
                OnlineHttp.Headers.Add("X-AUTH", _Config.Tokens.Xbox);
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
                HttpUserGamercard.Headers.Add("X-AUTH", _Config.Tokens.Xbox);
                HttpUserGamercard.Accept = "application/json";
                HttpWebResponse GamercardRes = (HttpWebResponse)HttpUserGamercard.GetResponse();
                Stream GamercardStream = GamercardRes.GetResponseStream();
                StreamReader GamercardRead = new StreamReader(GamercardStream, Encoding.UTF8);
                var GamercardJson = GamercardRead.ReadToEnd();
                dynamic stuff = Newtonsoft.Json.Linq.JObject.Parse(GamercardJson);
                HttpWebRequest HttpUserFrineds = (HttpWebRequest)WebRequest.Create("https://xboxapi.com/v2/" + UserID + "/friends");
                HttpUserFrineds.Method = WebRequestMethods.Http.Get;
                HttpUserFrineds.Headers.Add("X-AUTH", _Config.Tokens.Xbox);
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
                        Text = Status + " | More commands coming soon"
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

        [Group("lol"), Summary("League Of Legends")]
        public class LolGroup : ModuleBase
        {
            [Command]
            public async Task LolHelp()
            {

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

        [Command("ow"), Remarks("ow (User#Tag)"), Summary("Overwatch game user stats")]
        public async Task Ow(string User = "")
        {
            if (!User.Contains("#") | OverwatchAPIHelpers.IsValidBattletag(User) == false)
            {
                await Context.Channel.SendMessageAsync("`Overwatch user/tag not found | Example SirDoombox#2603`");
                return;
            }
            _Overwatch.Player Player = null;
            try
            {
                Player = _Overwatch.GetPlayerStat(User);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            if (Player == null || Player.Status == _Overwatch.RequestStatus.UnknownPlayer)
            {
                await ReplyAsync("`Could not find player`");
                return;
            }
            var embed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = $"{User}",
                    IconUrl = Player.RankIcon,
                    Url = Player.ProfileUrl
                },
                ThumbnailUrl = Player.ProfileUrl,
                Color = _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel),
                Timestamp = Player.LastPlayed.Date,
                Description = "```md" + Environment.NewLine + $"<Achievements Broken Atm>" + Environment.NewLine + $"<Level {Player.Level}> <Rank {Player.CompetitiveRank}>" + Environment.NewLine + $"<Casual Games won {Player.CasualPlayed} | Time {Player.CasualPlaytime / 60} Mins>" + Environment.NewLine + $"<Ranked Games played {Player.RankPlayed} | Time {Player.RankPlaytime / 60} Mins>```",
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Platform {Player.Platform} | Region {Player.Region}"
                }
            };
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("poke"), Alias("Pokemon"), Remarks("poke"), Summary("Pokemon lookup")]
        public async Task Pokemon(string Name)
        {
            int Data = _Poke.GetPokemonID(Name);
            if (Data == 0)
            {
                await Context.Channel.SendMessageAsync($"`Cannot find pokemone {Name}`");
                return;
            }
            var embed = new EmbedBuilder()
            {
                Color = _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel),
                ImageUrl = "https://raw.githubusercontent.com/PokeAPI/pokeapi/master/data/v2/sprites/pokemon/" + Data + ".png"
            };
        }

        [Command("pokerev"), Remarks("pokerev"), Summary("Pokemon Revolution Leaderboards")]
        public async Task PokemonRev()
        {
            List<string> List1 = _Poke.PokemonRevolution.GetMainTable("https://pokemon-revolution-online.net/ladder.php", "auto-style2", 1);
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
            embed1.AddField("Rank | User", $"{List1[0]}. {List1[1]}" + Environment.NewLine + $"{List1[5]}. {List1[6]}" + Environment.NewLine + $"{List1[10]}. {List1[11]}" + Environment.NewLine + $"{List1[15]}. {List1[16]}" + Environment.NewLine + $"{List1[20]}. {List1[21]}" + Environment.NewLine + $"{List1[25]}. {List1[26]}" + Environment.NewLine + $"{List1[30]}. {List1[31]}" + Environment.NewLine + $"{List1[35]}. {List1[36]}" + Environment.NewLine + $"{List1[40]}. {List1[41]}" + Environment.NewLine + $"{List1[45]}. {List1[46]}", true);
            embed1.AddField("Win/Loss", $"{List1[3]}/{List1[4]}" + Environment.NewLine + $"{List1[8]}/{List1[9]}" + Environment.NewLine + $"{List1[13]}/{List1[14]}" + Environment.NewLine + $"{List1[18]}/{List1[19]}" + Environment.NewLine + $"{List1[23]}/{List1[24]}" + Environment.NewLine + $"{List1[28]}/{List1[29]}" + Environment.NewLine + $"{List1[33]}/{List1[34]}" + Environment.NewLine + $"{List1[38]}/{List1[39]}" + Environment.NewLine + $"{List1[43]}/{List1[44]}" + Environment.NewLine + $"{List1[48]}/{List1[49]}", true);
            embed1.AddField("Rating", $"{List1[2]}" + Environment.NewLine + $"{List1[7]}" + Environment.NewLine + $"{List1[12]}" + Environment.NewLine + $"{List1[17]}" + Environment.NewLine + $"{List1[22]}" + Environment.NewLine + $"{List1[27]}" + Environment.NewLine + $"{List1[32]}" + Environment.NewLine + $"{List1[37]}" + Environment.NewLine + $"{List1[42]}" + Environment.NewLine + $"{List1[47]}", true);
            List1.Clear();
            List1 = _Poke.PokemonRevolution.GetMainTable("https://pokemon-revolution-online.net/ladder.php", "auto-style2", 2);
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
            embed2.AddField("Rank | User", $"{List1[0]}. {List1[1]}" + Environment.NewLine + $"{List1[5]}. {List1[6]}" + Environment.NewLine + $"{List1[10]}. {List1[11]}" + Environment.NewLine + $"{List1[15]}. {List1[16]}" + Environment.NewLine + $"{List1[20]}. {List1[21]}" + Environment.NewLine + $"{List1[25]}. {List1[26]}" + Environment.NewLine + $"{List1[30]}. {List1[31]}" + Environment.NewLine + $"{List1[35]}. {List1[36]}" + Environment.NewLine + $"{List1[40]}. {List1[41]}" + Environment.NewLine + $"{List1[45]}. {List1[46]}", true);
            embed2.AddField("Win/Loss", $"{List1[2]}/{List1[3]}" + Environment.NewLine + $"{List1[7]}/{List1[8]}" + Environment.NewLine + $"{List1[12]}/{List1[13]}" + Environment.NewLine + $"{List1[17]}/{List1[18]}" + Environment.NewLine + $"{List1[22]}/{List1[23]}" + Environment.NewLine + $"{List1[27]}/{List1[28]}" + Environment.NewLine + $"{List1[32]}/{List1[33]}" + Environment.NewLine + $"{List1[37]}/{List1[38]}" + Environment.NewLine + $"{List1[42]}/{List1[43]}" + Environment.NewLine + $"{List1[47]}/{List1[48]}", true);
            List1.Clear();
            List1 = _Poke.PokemonRevolution.GetPlaytimeTable("https://pokemon-revolution-online.net/ladder.php", "auto-style2", 3);
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
            embed3.AddField("Rank | User", $"{List1[0]}. {List1[1]}" + Environment.NewLine + $"{List1[3]}. {List1[4]}" + Environment.NewLine + $"{List1[6]}. {List1[7]}" + Environment.NewLine + $"{List1[9]}. {List1[10]}" + Environment.NewLine + $"{List1[12]}. {List1[13]}" + Environment.NewLine + $"{List1[15]}. {List1[16]}" + Environment.NewLine + $"{List1[18]}. {List1[19]}" + Environment.NewLine + $"{List1[21]}. {List1[22]}" + Environment.NewLine + $"{List1[24]}. {List1[25]}" + Environment.NewLine + $"{List1[27]}. {List1[28]}", true);
            embed3.AddField("Playtime", $"{List1[2]}" + Environment.NewLine + $"{List1[5]}" + Environment.NewLine + $"{List1[8]}" + Environment.NewLine + $"{List1[11]}" + Environment.NewLine + $"{List1[14]}" + Environment.NewLine + $"{List1[17]}" + Environment.NewLine + $"{List1[20]}" + Environment.NewLine + $"{List1[23]}" + Environment.NewLine + $"{List1[26]}" + Environment.NewLine + $"{List1[29]}", true);
            var Embeds = new List<PaginationFull.Page>
            {
                new PaginationFull.Page(){Description = embed1.Description, Author = embed1.Author, Fields = embed1.Fields }, new PaginationFull.Page(){Description = embed2.Description, Author = embed2.Author, Fields = embed2.Fields }, new PaginationFull.Page(){Description = embed3.Description, Author = embed3.Author, Fields = embed3.Fields }
            };
            PaginationFull.PaginatedMessage message = new PaginationFull.PaginatedMessage(Embeds, "", _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel), Context.User);
            await _PagFull.SendPaginatedMessageAsync(Context.Channel, message);

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

        [Group("steam"), Summary("Steam user/game info")]
        public class Steam : ModuleBase
        {
            [Command]
            public async Task SteamHelp()
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
                await Context.Channel.SendMessageAsync("", false, infoembed.Build());
            }

            [Command("game"), Remarks("steam game (Game)"), Summary("Lookup a steam game")]
            public async Task SteamGame([Remainder]string Game = "")
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

            [Command("user"), Remarks("steam user (User)"), Summary("Steam user info")]
            public async Task SteamUser(string User = "")
            {
                if (User == "")
                {
                    await Context.Channel.SendMessageAsync("p/steam u (User)" + Environment.NewLine + "How to get a steam user? <" + "http://i.imgur.com/pM9dff5.png" + ">");
                    return;
                }
                string Claim = "";


                SteamIdentity SteamUser = null;
                SteamWebAPI.SetGlobalKey(_Config.Tokens.Steam);
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
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        [Command("osu"), Remarks("ose (User)"), Summary("osu! user info")]
        public async Task Osu(string User = "")
        {
            if (User == "")
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
                await Context.Channel.SendMessageAsync("", false, infoembed.Build());
                return;
            }
            try
            {
                HttpWebRequest GetUserId = (HttpWebRequest)WebRequest.Create("https://osu.ppy.sh/api/get_user?k=" + _Config.Tokens.Osu + "&u=" + User);
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
                var embed = new EmbedBuilder()
                {
                    Title = $"{OsuUser} | (Level: {OsuLevel.Split('.').First()})",
                    Description = "```md" + Environment.NewLine + $"<Played {OsuPlaycount}> <Accuracy {OsuAccuracy.Split('.').First()}>" + Environment.NewLine + $"[ A: {OsuRankA} S: {OsuRankS} SS: {OsuRankSS} ](Rank)" + Environment.NewLine + $"[ Total: {OsuTotal} Ranked: {OsuRanked} ](Score)```",
                    ThumbnailUrl = "http://orig09.deviantart.net/0c0c/f/2014/223/1/5/osu_icon_by_gentheminer-d7unrx3.png",
                    Url = "https://osu.ppy.sh/u/" + OsuUser,
                    Color = new Color(255, 105, 180)
                };
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            catch
            {
                await Context.Channel.SendMessageAsync("`Cannot find osu user`");
            }
        }

        [Group("mc"), Alias("minecraft"), Summary("Minecraft skins, color codes and seperate bot")]
        public class MinecraftGroup : ModuleBase
        {
            [Command]
            public async Task MinecraftHelp()
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
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }

            [Command("skin"), Remarks("mc skin (User)"), Summary("User skin")]
            public async Task MinecraftSkin([Remainder] string User = "")
            {
                if (User == "")
                {
                    await Context.Channel.SendMessageAsync($"p/mc skin (User) | `mc/skin Notch`");
                    return;
                }
                UuidAtTimeResponse uuid = new UuidAtTime(User, DateTime.Now).PerformRequest().Result;
                if (!uuid.IsSuccess)
                {
                    await ReplyAsync($"`Player {User} not found`");
                    return;
                }
                var embed = new EmbedBuilder()
                {
                    Color = _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel),
                    ImageUrl = "https://visage.surgeplay.com/full/200/" + User
                };
                await ReplyAsync("", false, embed.Build());
            }
            
            [Command("colors"), Remarks("mc colors"), Summary("Color codes")]
            public async Task MinecraftColors()
            {
                var embed = new EmbedBuilder()
                {
                    Title = "Minecraft Color Codes",
                    ImageUrl = "https://lolis.ml/img-1o4ubn88Z474.png"
                };
                await ReplyAsync("", false, embed.Build());
            }

            [Command("bot"), Remarks("mc bot"), Summary("Invite my main Minecraft Bot")]
            public async Task MinecraftBot()
            {
                var embed = new EmbedBuilder()
                {
                    Title = "Minecraft Bot",
                    Description = "Please use my main Mincraft bot with lots of cool commands such as quiz, minime, ping and name history. It also has community features such as being able to add servers to your community list and language translations in french and spanish. [Invite Bot](https://discordapp.com/oauth2/authorize?&client_id=346346285953056770&scope=bot&permissions=0)"
                };
                await ReplyAsync("", false, embed.Build());
            }

        }

        [Group("vg"), Alias("vainglory"), Summary("Vainglory commands")]
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

    public class Discord : ModuleBase
    {
        [Command("discord"), Remarks("discord"), Summary("Discord status")]
        public async Task Status(string Option = "")
        {
            string Status = "";
            string Time = "";
            WebClient WC = new WebClient();
            string Page = WC.DownloadString("https://status.discordapp.com/");
            HtmlAgilityPack.HtmlDocument HtmlDoc = new HtmlAgilityPack.HtmlDocument();
            HtmlDoc.LoadHtml(Page);
            var Root = HtmlDoc.DocumentNode;
            var embed = new EmbedBuilder()
            {
                Title = "<:discord:314003252830011395> Discord Status",
                Footer = new EmbedFooterBuilder()
                {
                    Text = ""
                }
            };
            switch (Option)
            {
                case "status":
                    try
                    {
                        foreach (var i in Root.SelectNodes("//div[@class='update']"))
                        {
                            var content = i.InnerText;
                            string[] words = content.Split('.');
                            Status = Status + words.First().Trim() + Environment.NewLine + Environment.NewLine;
                            if (Time == "")
                            {
                                Time = words.Last().Trim();
                            }
                        }
                    }
                    catch
                    {
                        Status = "<:online:313956277808005120> All Systems Operational!";
                    }
                    break;
                default:
                    try
                    {
                        var Nodes = Root.SelectNodes("//div[@class='update']");
                        if (Nodes != null)
                        {
                            var NodeItem = Nodes.First();
                            var NodeText = NodeItem.InnerText;
                            string[] NodeSplit = NodeText.Split('.');
                            Status = NodeSplit.First().Trim();
                            Time = NodeSplit.Last().Trim() + " | To get full status do > p/discord status";
                        }
                        else
                        {
                            Status = "All Systems Operational!";
                        }
                    }
                    catch
                    {
                        Status = "All Systems Operational!";
                    }
                    embed.AddField("Links", "[Website](https://discordapp.com/) | [Status](https://status.discordapp.com/) | [Twitter](https://twitter.com/discordapp)");
                    break;
            }
            if (Status == "All Systems Operational!")
            {
                embed.Color = new Color(0, 200, 0);
            }
            else
            {
                embed.Color = new Color(200, 0, 0);
            }
            embed.Description = Status;
            embed.Footer.Text = Time;
            await ReplyAsync("", false, embed.Build());

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
        public async Task Upvotes(string ID = "")
        {
            if (ID == "")
            {
                ID = Context.Client.CurrentUser.Id.ToString();
            }
            _BotApi.GetUpvotes(Context.Channel as ITextChannel, ID);

        }
    }


    public class Media : ModuleBase
    {
        [Group("yt"), Summary("Youtube Commands")]
        public class YoutubeGroup : ModuleBase
        {
            [Command]
            public async Task YoutubeHelp()
            {

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

            [Command("user")]
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

            [Command("notify")]
            [Alias("n")]
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

            [Command("list")]
            [Alias("l")]
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

            [Command("remove")]
            [Alias("r")]
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

    

    public class Help : ModuleBase
    {
        
        private CommandService _Commands;
        private PaginationFull _PagFull;
        public Help(CommandService Commands, PaginationFull PagFull)
        {
            _Commands = Commands;
            _PagFull = PagFull;
        }
        [Command("help"), Alias("commands")]
        public async Task Commands(string Option = "")
        {
            if (_Config.MainHelp == "")
            {
                _Config.SetupHelpMenu(_Commands);
            }
            if (Context.Guild == null || Option == "all")
            {
                if (Option == "all")
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Username} I have sent you a full list of commands");
                }
                var allemebed = new EmbedBuilder()
                {
                    Title = "Commands List",
                    Color = _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel)
                };
                allemebed.AddField(x =>
                {
                    x.Name = "Main"; x.Value = "```md" + Environment.NewLine + _Config.MainHelp + "```";
                });
                allemebed.AddField(x =>
                {
                    x.Name = "Game"; x.Value = "```md" + Environment.NewLine + _Config.GameHelp + "```";
                });
                allemebed.AddField(x =>
                {
                    x.Name = "Media"; x.Value = "```md" + Environment.NewLine + _Config.MediaHelp + "```";
                });
                allemebed.AddField(x =>
                {
                    x.Name = "Mod"; x.Value = "```md" + Environment.NewLine + _Config.ModHelp + "```";
                });
                allemebed.AddField(x =>
                {
                    x.Name = "Discord"; x.Value = "```md" + Environment.NewLine + _Config.DiscordHelp + "```";
                });
                allemebed.Color = new Color(0, 191, 255);
                var DM = await Context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                await DM.SendMessageAsync("", false, allemebed).ConfigureAwait(false);
                return;
            }
            else
            {
                _Bot.GuildCache.TryGetValue(Context.Guild.Id, out _CacheItem CI);
                string HelpText = "```md" + Environment.NewLine + "[ p/main ]( Info/Misc )" + Environment.NewLine + "[ p/game ]( Steam/Minecraft )" + Environment.NewLine + "[ p/media ]( Twitch )" + Environment.NewLine + "[ p/mod ]( Ban/Kick/Prune )" + Environment.NewLine + "[ p/bots ]( Get other bots info )```";
                if (!CI.Bot.GetPermissions(Context.Channel as ITextChannel).EmbedLinks)
                {
                    await Context.Channel.SendMessageAsync(HelpText);
                    return;
                }
                string PermReact = "Add Reactions :x:";
                string PermManage = "Manage Messages :x:";
                var embed = new EmbedBuilder()
                {
                };
                embed.AddField("Commands list", HelpText + Environment.NewLine + "For a list of all the bot commands do **p/help all** | " + Environment.NewLine + "or visit the website **p/website**", true);
                embed.AddField("Interactive Help", "For an interactive help menu" + Environment.NewLine + "Add these permissions" + Environment.NewLine + Environment.NewLine + PermReact + Environment.NewLine + Environment.NewLine + "(Optional)" + Environment.NewLine + PermManage, true);
                if (!CI.Bot.GetPermissions(Context.Channel as ITextChannel).AddReactions || !CI.Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
                {

                    if (CI.Bot.GetPermissions(Context.Channel as ITextChannel).AddReactions)
                    {
                        PermReact = "Add Reactions :white_check_mark: ";
                    }
                    if (CI.Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
                    {
                        PermManage = "Manage Messages :white_check_mark: ";
                    }
                }
                var Guilds = await Context.Client.GetGuildsAsync();
                var EmbedPages = new List<PaginationFull.Page>
                {
                    new PaginationFull.Page(){Description = "```md" + Environment.NewLine + "< | Info     | Commands ► >" + Environment.NewLine + "<Language C#> <Library .net 1.0>" + Environment.NewLine + $"<Guilds {Guilds.Count}>``` For a full list of commands do **p/help all** or visit the website" + Environment.NewLine + "[Website](https://blaze.ml) | [Invite Bot](https://discordapp.com/oauth2/authorize?&client_id=277933222015401985&scope=bot&permissions=0) | [Github](https://github.com/ArchboxDev/PixelBot) | [My Guild](http://discord.gg/WJTYdNb)"},
                    new PaginationFull.Page(){Description = "```md" + Environment.NewLine + "< ◄ Info |     Main     | Games ► >" + Environment.NewLine + _Config.MainHelp + "```"},
                    new PaginationFull.Page(){Description = "```md" + Environment.NewLine + "< ◄ Main |     Games     | Media ► >" + Environment.NewLine + _Config.GameHelp + "```"},
                    new PaginationFull.Page(){Description = "```md" + Environment.NewLine + "< ◄ Games |     Media     | Mod ► >" + Environment.NewLine + _Config.MediaHelp + "```"},
                    new PaginationFull.Page(){Description = "```md" + Environment.NewLine + "< ◄ Media |     Mod     | Bots ► >" + Environment.NewLine + _Config.ModHelp + "```"},
                    new PaginationFull.Page(){Description = "```md" + Environment.NewLine + "< ◄ Mod |     Bots | >" + Environment.NewLine + _Config.DiscordHelp + "```"}
                };
                var message = new PaginationFull.PaginatedMessage(EmbedPages, "Commands", _Utils_Discord.GetRoleColor(Context.Channel as ITextChannel), Context.User);
                if (CI.Bot.GuildPermissions.ManageMessages)
                {
                    await _PagFull.SendPaginatedMessageAsync(Context.Channel, message, false);
                }
                else
                {
                    await _PagFull.SendPaginatedMessageAsync(Context.Channel, message, true);
                }
            }

        }

        [Command("main"), Alias("misc")]
        public async Task Main()
        {
            List<string> CommandList = new List<string>();
            foreach (var CMD in _Commands.Commands.Where(x => x.Module.Name == "prune"))
            {
                CommandList.Add($"[ p/{CMD.Remarks} ][ {CMD.Summary} ]");
            }
            string Commands = string.Join(Environment.NewLine, CommandList);
            await Context.Channel.SendMessageAsync("Prune Commands```md" + Environment.NewLine + Commands + "```");
        }

        [Command("game"), Alias("games")]
        public async Task Game()
        {

        }
        [Command("media")]
        public async Task Media()
        {

        }
        [Command("mod")]
        public async Task Mod()
        {

        }

        [Command("bots")]
        public async Task Bots()
        {

        }
    }

    public class Profile : InteractiveModuleBase
    {
        [Command("profile")]
        public async Task UserProfile(string User = "")
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

        [Command("createprofile")]
        public async Task CreateProfile()
        {

        }

        [Command("claimsteam", RunMode = RunMode.Async)]
        public async Task Steamclaim([Remainder] string User = null)
        {
            if (User == null)
            {
                await Context.Channel.SendMessageAsync("`No user set | p/steam claim (User)");
                return;
            }
            SteamWebAPI.SetGlobalKey(_Config.Tokens.Steam);
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
}
