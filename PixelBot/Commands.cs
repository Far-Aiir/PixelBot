using Bot.Game;
using Bot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OverwatchAPI;
using PortableSteam;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Commands
{
    public class HelpGroup : ModuleBase
    {
        private CommandService _Commands;
        private PaginationFull _PagFull;
        public HelpGroup(CommandService Commands, PaginationFull PagFull)
        {
            _Commands = Commands;
            _PagFull = PagFull;
        }

        [Command("help")]
        public async Task Help(string Option = "")
        {
            if (_Config.MiscHelp == "")
            {
                _Config.SetupHelpMenu(_Commands);
            }
            if (Option.ToLower() == "full" || Option.ToLower() == "all" || Context.Guild == null)
            {
                try
                {
                    await Context.Message.AddReactionAsync(Discord.Addons.EmojiTools.EmojiExtensions.FromText(":ok_hand:"));
                }
                catch { }
                var DMEmbed = new EmbedBuilder()
                {
                    Title = "Commands",
                    Color = _Utils.Discord.GetRoleColor(Context.Channel as ITextChannel)
                };
                DMEmbed.AddField(x =>
                {
                    x.Name = "Misc"; x.Value = "```md" + Environment.NewLine + _Config.MiscHelp + "```";
                });
                DMEmbed.AddField(x =>
                {
                    x.Name = "Game"; x.Value = "```md" + Environment.NewLine + _Config.GameHelp + "```";
                });
             //   DMEmbed.AddField(x =>
            //    {
            //        x.Name = "Media"; x.Value = "```md" + Environment.NewLine + _Config.MediaHelp + "```";
             //   });
                DMEmbed.AddField(x =>
                {
                    x.Name = "Prune"; x.Value = "```md" + Environment.NewLine + _Config.ModHelp + "```";
                });
                DMEmbed.AddField(x =>
                {
                    x.Name = "Dev"; x.Value = "```md" + Environment.NewLine + _Config.DevHelp + "```";
                });
                var DM = await Context.User.GetOrCreateDMChannelAsync();
                await DM.SendMessageAsync("", false, DMEmbed.Build());
            }
            else
            {
                var EmbedPages = new List<PaginationFull.Page>
                {
                    new PaginationFull.Page(){Description = $"Use the arrows to view the commands\nOr do `p/help all`\n\n" + "[Website](https://blazeweb.ml) | [Invite Bot](https://discordapp.com/oauth2/authorize?&client_id=277933222015401985&scope=bot&permissions=0) | [Github](https://github.com/ArchboxDev/PixelBot) | [My Guild](http://discord.gg/WJTYdNb)"},
                    new PaginationFull.Page(){Description = "```md" + Environment.NewLine + "< ◄ Info |     Misc     | Games ► >" + Environment.NewLine + _Config.MiscHelp + "```"},
                    new PaginationFull.Page(){Description = "```md" + Environment.NewLine + "< ◄ Misc |     Games     | Prune ► >" + Environment.NewLine + _Config.GameHelp + "```"},
                    new PaginationFull.Page(){Description = "```md" + Environment.NewLine + "< ◄ Games |     Prune     | Dev ► >" + Environment.NewLine + _Config.ModHelp + "```"},
                   // new PaginationFull.Page(){Description = "```md" + Environment.NewLine + "< ◄ Media |     Mod     | Dev >" + Environment.NewLine + _Config.ModHelp + "```"},
                    new PaginationFull.Page(){Description = "```md" + Environment.NewLine + "< ◄ Mod |     Dev | >" + Environment.NewLine + _Config.DevHelp + "```"}
                };
                var message = new PaginationFull.PaginatedMessage(EmbedPages, "Commands", _Utils.Discord.GetRoleColor(Context.Channel as ITextChannel), Context.User);
                IGuildUser Bot = await Context.Guild.GetCurrentUserAsync();
                if (Bot.GuildPermissions.ManageMessages)
                {
                    await _PagFull.SendPaginatedMessageAsync(Context.Channel, message, false);
                }
                else
                {
                    await _PagFull.SendPaginatedMessageAsync(Context.Channel, message, true);
                }
            }
            
        }

        [Command("news"), RequireOwner]
        public async Task News()
        {
            var embed = new EmbedBuilder()
            {
                Title = "Minotaur",
                Description = "<@325892959130222592> | [Invite](https://discordapp.com/oauth2/authorize?&client_id=325892959130222592&scope=bot&permissions=0) | [Upvote](https://discordbots.org/bot/325892959130222592) | [Github](https://github.com/ArchboxDev/Starboat)" + Environment.NewLine + "A starboard bot for Discord give your favourite messages a nice star.",
                Color = new Color(66, 131, 244),
                ThumbnailUrl = "https://i.imgur.com/I7Jq2wo.png",
                Footer = new EmbedFooterBuilder()
                {
                    Text = "Prefix: s/help"
                }
            };
            var embed2 = new EmbedBuilder()
            {
                Title = "Roles",
                Description = "You can request a colored role in <#370483498760404992>" + Environment.NewLine + "<@&370489106028822538> - Get pinged for announcements use `?news`" + Environment.NewLine + "<@&275064683130781706> - Ask xXBuilderBXx in DMs to add your bot" + Environment.NewLine + "<@&275738929959796736> - Access the nsfw channel with `?nsfw`" + Environment.NewLine + "<@&275739869358325760> - Show/Hide <#275127911080787980> use `?nogames`" + Environment.NewLine + "<@&370489043814842368> - Show/Hide <#275062471289602049> use `?noshitposting`"
            };
            var WH = new Discord.Webhook.DiscordWebhookClient(370459217578033162, "L7RKGvdIrBsu0NFunpgEN2a76XPk5OF2k2zca-nXKM6CWMHIELq1lD7puw4k0sMz9do9");
            await WH.SendMessageAsync("", false, new Embed[] { embed2.Build() }  , "Discord Universe");
        }

        [Command("misc")]
        public async Task Misc()
        {
            await ReplyAsync("Test");
        }

        [Command("media")]
        public async Task Media()
        {
            await ReplyAsync("Test");
        }
        [Command("dev")]
        public async Task Dev()
        {
            await ReplyAsync("Test");
        }

        [Command("mod"), Alias("kick", "ban", "prune")]
        public async Task Mod()
        {
            await ReplyAsync("Test");
        }
    }

    public class Misc : ModuleBase
    {
        private DiscordSocketClient _Client;
        public Misc(DiscordSocketClient client)
        {
            _Client = client;
        }
        [Command("roll"), Alias("dice"), Remarks("roll"), Summary("Roll the dice")]
        public async Task Roll()
        {
            Random.Org.Random Rng = new Random.Org.Random();
            int Number = Rng.Next(1, 6);
            string TextNum = "";
            switch(Number)
            {
                case 1:
                    TextNum = ":one:";
                    break;
                case 2:
                    TextNum = ":two:";
                    break;
                case 3:
                    TextNum = ":three:";
                    break;
                case 4:
                    TextNum = ":four:";
                    break;
                case 5:
                    TextNum = ":five:";
                    break;
                case 6:
                    TextNum = ":six:";
                    break;
            }
            await ReplyAsync($":game_die: You rolled a {TextNum}");
        }

        [Command("flip"), Remarks("flip"), Summary("Flip the coin")]
        public async Task Flip()
        {
            Random.Org.Random Rng = new Random.Org.Random();
            int Number = Rng.Next(1, 6);
            string Coin = "";
            switch (Number)
            {
                case 1:
                case 3:
                case 5:
                    Coin = "Heads";
                    break;
                case 2:
                case 4:
                case 6:
                    Coin = "Tails";
                    break;
            }
            await ReplyAsync($"You got {Coin}");
        }

        [Command("server"), Alias("guild"), Remarks("server"), Summary("Server info/roles/emotes"), RequireContext(ContextType.Guild)]
        public async Task Guild(string Arg1 = "", string Arg2 = "")
        {
            if (Arg1 == "roles" || Arg1 == "role")
            {
                List<string> Output = new List<string>();
                var Users = await Context.Guild.GetUsersAsync();

                if (Arg2 == "id" || Arg2 == "ids")
                {
                    foreach (var i in Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id))
                    {
                        Output.Add($"<{Users.Where(x => x.RoleIds.Contains(i.Id)).Count()} {i.Name} = {i.Id}>");
                    }
                    var embed = new EmbedBuilder()
                    {
                        Description = "```md" + Environment.NewLine + string.Join(Environment.NewLine, Output) + "```",
                        Color = _Utils.Discord.GetRoleColor(Context.Channel)
                    };
                    if (Context.Guild.Roles.Count > 10)
                    {
                        embed.Title = $"Roles for {Context.Guild.Name}";
                        IDMChannel DM = await Context.User.GetOrCreateDMChannelAsync();
                        await DM.SendMessageAsync("", false, embed.Build());
                    }
                    else
                    {
                        await ReplyAsync("", false, embed.Build());
                    }
                }
                else
                {
                    foreach (var i in Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id))
                    {
                        Output.Add($"<@&{i.Id}> [{Users.Where(x => x.RoleIds.Contains(i.Id)).Count()}]");
                    }
                    var embed = new EmbedBuilder()
                    {
                        Description = string.Join(" | ", Output) + Environment.NewLine + Environment.NewLine + $"To get IDs use {Context.Message.Content} id",
                        Color = _Utils.Discord.GetRoleColor(Context.Channel)
                    };
                    await ReplyAsync("", false, embed.Build());
                }
            }
            else if (Arg1 == "emotes" || Arg1 == "emojis" || Arg1 == "emote" || Arg1 == "emoji")
            {
                List<string> Emotes = new List<string>();
                foreach (var i in Context.Guild.Emotes)
                {
                    Emotes.Add($"<:{i.Name}:{i.Id}>");
                }
                var embed = new EmbedBuilder()
                {
                    Description = string.Join(" | ", Emotes),
                    Color = _Utils.Discord.GetRoleColor(Context.Channel)
                    };
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
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
                int CustomEmotes = 0;
                foreach (var emoji in Context.Guild.Emotes)
                {
                    CustomEmotes++;
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
                    Color = _Utils.Discord.GetRoleColor(Context.Channel as ITextChannel),
                    Description = $"Owner: {Owner.Mention}" + Environment.NewLine + $"{Context.Message.Content} roles | {Context.Message.Content} emotes```md" + Environment.NewLine + $"[Online](Offline)" + Environment.NewLine + $"<Users> [{MembersOnline}]({Members}) <Bots> [{BotsOnline}]({Bots})" + Environment.NewLine + $"Channels <Text {TextChan}> <Voice {VoiceChan}>" + Environment.NewLine + $"<Roles {Context.Guild.Roles.Count}> <Region {Context.Guild.VoiceRegionId}>" + Environment.NewLine + $"<CustomEmotes {CustomEmotes}>```",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Created {Context.Guild.CreatedAt.Date.Day} {Context.Guild.CreatedAt.Date.DayOfWeek} {Context.Guild.CreatedAt.Year}"
                    }
                };
                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("user"), Alias("info"), Remarks("user (@Mention/ID)"), Summary("Info about a user")]
        public async Task User(string User = "")
        {
            if (User == "")
            {
                User = Context.User.Id.ToString();
            }
            string Mention = "";
            User = _Utils.Discord.MentionToID(User);
            IGuildUser GU = null;
            int GuildCount = 0;
            Color EmbedColor = new Color(255, 165, 0);
            foreach (var Guild in _Client.Guilds)
            {
                IGuildUser FindUser = Guild.GetUser(Convert.ToUInt64(User));
                if (FindUser != null)
                {
                    GuildCount++;
                    if (Guild.Id == Context.Guild.Id)
                    {
                        EmbedColor = new Color(0, 200, 0);
                        Mention = $"{FindUser.Username}#{FindUser.Discriminator} - <@{FindUser.Id}>";
                        GU = FindUser;
                        break;
                    }
                    else if (GU == null)
                    {
                        Mention = $"{FindUser.Username}#{FindUser.Discriminator} - Not in this guild";
                        GU = FindUser;
                        break;
                    }
                }

            }
            if (GU == null)
            {
                await ReplyAsync($"`Could not find user in {_Client.Guilds.Count} guilds`");
                return;
            }
            if (GU.IsBot)
            {
                Mention = Mention + " <:botTag:230105988211015680>";
            }
            var embed = new EmbedBuilder()
            {
                ThumbnailUrl = GU.GetAvatarUrl(),
                Color = EmbedColor,
                Description = Mention + Environment.NewLine + "```md" + Environment.NewLine + $"<Discrim {GU.Discriminator}> <ID {GU.Id}>" + Environment.NewLine + $"<Joined_Guild {GU.JoinedAt.Value.Day} {GU.JoinedAt.Value.Date.ToString("MMMM")} {GU.JoinedAt.Value.Year}>" + Environment.NewLine + $"<Created_Account {GU.CreatedAt.Day} {GU.CreatedAt.DateTime.ToString("MMMM")} {GU.CreatedAt.Year}>" + Environment.NewLine + $"Found in {GuildCount} guilds```",
                Footer = new EmbedFooterBuilder()
                { Text = "To lookup a discrim use | p/discrim 0000" }
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("avatar"), Remarks("avatar (@Mention/ID)"), Summary("Get a users avatar")]
        public async Task Avatar(string User = "")
        {
            if (User == "")
            {
                User = Context.User.Id.ToString();
            }
            IGuildUser GU = null;
            foreach (var Guild in _Client.Guilds)
            {
                IGuildUser FindUser = Guild.GetUser(Convert.ToUInt64(_Utils.Discord.MentionToID(User)));
                if (FindUser != null)
                {
                    GU = FindUser;
                    break;
                }
            }
            if (GU == null)
            {
                await ReplyAsync($"`Could not find user in {_Client.Guilds.Count} guilds`");
                return;
            }
            if (GU.GetAvatarUrl() == null || GU.GetAvatarUrl() == "")
            {
                await ReplyAsync($"`{GU.Username} does not have an profile pic/avatar set`");
                return;
            }
            var embed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = $"Avatar for {GU.Username}",
                    Url = GU.GetAvatarUrl()
                },
                Color = _Utils.Discord.GetRoleColor(Context.Channel),
                ImageUrl = GU.GetAvatarUrl()
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("discrim"), Remarks("discrim (0000)"), Summary("list all user with a discrim")]
        public async Task Discrim(string Discrim = "0", string Option = "")
        {
            if (Discrim == "0")
            {
                await Context.Channel.SendMessageAsync($"`You can search for a discrim using p/discrim {Context.User.Discriminator}`");
                return;
            }
            if (Discrim == "0000")
            {
                await Context.Channel.SendMessageAsync($"`0000 is a discrim only used by webhooks`");
                return;
            }
            var Guilds = await Context.Client.GetGuildsAsync();
            Dictionary<ulong, string> GuildList = new Dictionary<ulong, string>();
            Dictionary<ulong, string> GlobalList = new Dictionary<ulong, string>();
            ulong ID = 0;
            if (Context.Channel is ITextChannel)
            {
                ID = Context.Guild.Id;
                var Users = await Context.Guild.GetUsersAsync();
                foreach (IGuildUser User in Users)
                {
                    if (User.Id != Context.User.Id && User.Discriminator == Discrim)
                    {
                        string UserID = "";
                        if (Option.ToLower() == "id")
                        {
                            UserID = $"= {User.Id.ToString()} ";
                        }
                        if (User.IsBot)
                        {
                            GuildList.Add(User.Id, $@"<Bot    {User.Username}#{User.Discriminator}".PadRight(40) + $"{UserID}>");
                        }
                        else
                        {
                            GuildList.Add(User.Id, $@"<User   {User.Username}#{User.Discriminator}".PadRight(40) + $"{UserID}>");
                        }
                    }
                }
            }
            foreach (IGuild Guild in Guilds)
            {
                if (Guild.Id != ID)
                {
                    var Users = await Guild.GetUsersAsync();
                    foreach (var User in Users)
                    {
                        if (User.Id != Context.User.Id && !GlobalList.ContainsKey(User.Id) && !GuildList.ContainsKey(User.Id) && User.Discriminator == Discrim)
                        {
                            string UserID = "";
                            if (Option.ToLower() == "id")
                            {
                                UserID = $"= {User.Id.ToString()} ";
                            }
                            if (User.IsBot)
                            {
                                GlobalList.Add(User.Id, $@"<Bot    {User.Username}#{User.Discriminator}".PadRight(40) + $"{UserID}>");
                            }
                            else
                            {
                                GlobalList.Add(User.Id, $@"<User   {User.Username}#{User.Discriminator}".PadRight(40) + $"{UserID}>");
                            }
                        }
                    }
                }
            }
            if (GuildList.Keys.Count == 0 && GlobalList.Keys.Count == 0)
            {
                await ReplyAsync($"`Could not find any users with the discrim {Discrim}`");
            }
            else
            {
                string Content = "";
                if (GuildList.Keys.Count != 0) Content = $"#       Guild\n" + string.Join("\n", GuildList.Values);
                if (Content != "")
                {
                    if (GlobalList.Keys.Count != 0) Content = Content + $"\n\n#       Global\n" + string.Join("\n", GlobalList.Values);
                }
                else
                {
                    if (GlobalList.Keys.Count != 0) Content = $"#       Global\n" + string.Join("\n", GlobalList.Values);
                }
                await ReplyAsync("```md\n"+ Content + "```");
            }
        }

        [Command("dog"), Alias("doge"), Remarks("dog"), Summary("Get a random dog")]
        public async Task Dog()
        {
            string Request = "";
            int Count = 0;
            while (Request == "" || Request.EndsWith(".mp4"))
            {
                Count++;
                if (Count == 5) _Log.ThrowError("Failed to get image");
               Request = _Utils.Http.GetString("http://random.dog/woof");
            }
            var embed = new EmbedBuilder()
            {
                ImageUrl = "http://random.dog/" + Request,
                Color = _Utils.Discord.GetRoleColor(Context.Channel)
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("cat"), Remarks("cat"), Summary("Get a random cat")]
        public async Task Cat()
        {
            WebRequest request = WebRequest.Create("http://random.cat/meow");
            request.Proxy = null;
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream, System.Text.Encoding.UTF8);
            dynamic Item = JObject.Parse(reader.ReadToEnd());
            var embed = new EmbedBuilder()
            {
                ImageUrl = Item.file,
                Color = _Utils.Discord.GetRoleColor(Context.Channel)
            };
            reader.Close();
            response.Close();
            await ReplyAsync("", false, embed.Build());
        }

        [Command("neko"), Remarks("neko"), Summary("Get a random neko")]
        public async Task Neko()
        {
            dynamic Request = _Utils.Http.JsonObject("https://nekos.life/api/neko", "", "key", "dnZ4fFJbjtch56pNbfrZeSRfgWqdPDgf");
            
            var embed = new EmbedBuilder()
            {
                ImageUrl = Request.neko,
                Color = _Utils.Discord.GetRoleColor(Context.Channel)
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("hug"), Remarks("hug | p/hug (@Mention)"), Summary("Random hug pic/gif")]
        public async Task Hug(string User = "")
        {
            dynamic Request = _Utils.Http.JsonObject("https://nekos.life/api/hug", "", "key", "dnZ4fFJbjtch56pNbfrZeSRfgWqdPDgf");
            
            var embed = new EmbedBuilder()
            {
                ImageUrl = Request.url,
                Color = _Utils.Discord.GetRoleColor(Context.Channel)
            };
            if (Context.Guild != null && User != "")
            {
                IGuildUser GU = await Context.Guild.GetUserAsync(Convert.ToUInt64(_Utils.Discord.MentionToID(User)));
                if (GU != null)
                {
                    embed.Title = $"{Context.User.Username} hugged {GU.Username}";
                }
            }
            await ReplyAsync("", false, embed.Build());
        }

        [Command("pat"), Remarks("pat | p/pat (@Mention)"), Summary("Random pat pic/gif")]
        public async Task Pat(string User = "")
        {
            dynamic Request = _Utils.Http.JsonObject("https://nekos.life/api/pat", "", "key", "dnZ4fFJbjtch56pNbfrZeSRfgWqdPDgf");
            
            var embed = new EmbedBuilder()
            {
                ImageUrl = Request.url,
                Color = _Utils.Discord.GetRoleColor(Context.Channel)
            };
            if (Context.Guild != null && User != "")
            {
                IGuildUser GU = await Context.Guild.GetUserAsync(Convert.ToUInt64(_Utils.Discord.MentionToID(User)));
                if (GU != null)
                {
                    embed.Title = $"{Context.User.Username} patted {GU.Username}";
                }
            }
            await ReplyAsync("", false, embed.Build());
        }

        [Command("kiss"), Remarks("kiss | kiss (@Mention)"), Summary("Random kiss pic/gif")]
        public async Task Kiss(string User = "")
        {
            dynamic Request = _Utils.Http.JsonObject("https://nekos.life/api/kiss", "", "key", "dnZ4fFJbjtch56pNbfrZeSRfgWqdPDgf");
            
            var embed = new EmbedBuilder()
            {
                ImageUrl = Request.url,
                Color = _Utils.Discord.GetRoleColor(Context.Channel)
            };
            if (Context.Guild != null && User != "")
            {
                IGuildUser GU = await Context.Guild.GetUserAsync(Convert.ToUInt64(_Utils.Discord.MentionToID(User)));
                if (GU != null)
                {
                    embed.Title = $"{Context.User.Username} kissed {GU.Username}";
                }
            }
            await ReplyAsync("", false, embed.Build());
        }

        [Command("bot"), Remarks("bot"), Summary("Bot info/links/invite")]
        public async Task Info()
        {
                var embed = new EmbedBuilder()
                {
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"For a list of all commands do | p/help all"
                    },
                    Color = _Utils.Discord.GetRoleColor(Context.Channel as ITextChannel)
                };
                int Guilds = (await Context.Client.GetGuildsAsync()).Count();
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
                    x.Name = ":globe_with_meridians: Links"; x.Value = $"" + Environment.NewLine + "[Website](https://blazeweb.ml)" + Environment.NewLine + "[Invite Bot](https://goo.gl/GsnmZP)" + Environment.NewLine + "[My Anime List](https://goo.gl/PtGU7C)" + Environment.NewLine + "[Monstercat](https://goo.gl/FgW5sT)" + Environment.NewLine + "[PixelBot Github](https://goo.gl/ORjWNh)"; x.IsInline = true;
                });
                await ReplyAsync("", false, embed.Build());
        }

        [Command("math"), Alias("calc"), Remarks("math (1 + 5 * 10)"), Summary("Do some math")]
        public async Task Math([Remainder] string Math)
        {
            var interpreter = new DynamicExpresso.Interpreter();
            var result = interpreter.Eval(Math);
            await ReplyAsync($"```md\n< {Math} = {result.ToString()} >```");
        }

        [Command("invite"), Remarks("invite"), Summary("Get the bot invite")]
        public async Task Invite()
        {
            await ReplyAsync("T");
        }
    }

    public class Game : ModuleBase
    {
        private PaginationFull _PagFull;
        public Game(PaginationFull pagfull)
        {
            _PagFull = pagfull;
        }

        [Command("game"), Remarks("game (Name)"), Summary("Lookup a game")]
        public async Task GameCommand([Remainder]string Name = "")
        {
            if (Name == "")
            {
                await ReplyAsync("Use `p/game (Name)` | `p/game rocket league` to search for a game");
                return;
            }
            var Data = _Utils.Http.JsonArray("https://api-2445582011268.apicast.io/games/?search=" + Name.Replace(" ", "+") +"&fields=*", "", "user-key", _Config.Tokens.GameInfo);
            
            if ((Data as JArray).Count == 0)
            {
                await ReplyAsync($"`Could not find game {Name}`");
                return;
            }
            Classes.GameInfo Game = new Classes.GameInfo(Data[0]);
            var embed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = Game.Name,
                    Url = Game.Url
                },
                ThumbnailUrl = Game.ImageUrl,
                Color = _Utils.Discord.GetRoleColor(Context.Channel),
                Description = Game.Desc + Environment.NewLine + string.Join(" | ", Game.Websites),
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Rating: {Game.Rating}% of {Game.RatingUsers} users"
                }
                
            };
            await ReplyAsync("", false, embed.Build());
        }

        [Command("steam"), Remarks("steam (User)"), Summary("Lookup a steam user")]
        public async Task Steam(string User = "")
        {
            if (User == "")
            {
                var embed2 = new EmbedBuilder()
                {
                    Description = "p/steam (User) | p/steam builderb",
                    ImageUrl = "http://i.imgur.com/pM9dff5.png"
                };
                await ReplyAsync("", false, embed2.Build());
                return;
            }
            else
            {
                try
                {
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
                    Console.WriteLine(Friends.Data.Friends.Count + " T1");
                    Console.WriteLine(Games.Data.GameCount + " T2");
                    Console.WriteLine(Badges.Data.Badges.Count + " T3");
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        [Command("ow"), Remarks("ow (User#Tag)"), Summary("Overwatch user info/stats")]
        public async Task Ow(string User = "")
        {
            await ReplyAsync("`API Issue that is currently being fixed`");
            return;
            if (!User.Contains("#"))
            {
                await ReplyAsync("`Overwatch user/tag not found | Example SirDoombox#2603`");
                return;
            }
            var Player = await _Config.OverwatchClient.GetPlayerAsync(User);
            
            if (Player == null)
            {
                await ReplyAsync("`Could not find player`");
                return;
            }
           
            var embed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = $"{User}",
                    Url = Player.ProfileUrl
                },
                Description = $"Level: {Player.PlayerLevel} | Achievements: {Player.Achievements.Where(x => x.IsEarned).Count()} | Playform: {Player.Platform}",
                ThumbnailUrl = Player.ProfilePortraitUrl,
                Color = _Utils.Discord.GetRoleColor(Context.Channel as ITextChannel),
                
            };
            embed.AddField("Casual", "-", true);
            embed.AddField("Ranked", "-", true);
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("mc"), Alias("minecraft"), Remarks("mc"), Summary("Invite my Minecraft bot")]
        public async Task Minecraft(string Placeholder = "")
        {
            var embed = new EmbedBuilder()
            {
                Description = "All Minecraft commands have been moved to my Minecraft bot > [Invite Bot](https://discordapp.com/oauth2/authorize?&client_id=346346285953056770&scope=bot&permissions=0)" + Environment.NewLine + "For more info visit my [Website](https://blazeweb.ml/minecraft/)",
                Color = _Utils.Discord.GetRoleColor(Context.Channel)
            };
            await ReplyAsync("", false, embed.Build());
        }
       
        [Command("vg")]
        public async Task Vainglory()
        {
            await ReplyAsync("`Coming soon`");
        }

        [Command("xbox"), Remarks("xbox (User)"), Summary("Xbox live user info/stats")]
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
            if (!LiveR.Contains("Up and Running"))
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
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            catch
            {
                await PWM.DeleteAsync();
                await Context.Channel.SendMessageAsync($"`Could not find user | {Status}`");
                return;
            }
        }

        [Command("osu"), Remarks("osu (User)"), Summary("osu! user info/stats")]
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

        [Command("poke"), Alias("Pokemon"), Remarks("poke"), Summary("Pokemon lookup")]
        public async Task Pokemon(string Name)
        {
            dynamic Data = _Utils.Http.JsonObject("http://pokeapi.co/api/v2/pokemon/" + Name + "/");
            if (Data.Json == null)
            {
                await Context.Channel.SendMessageAsync($"`Cannot find pokemone {Name}`");
                return;
            }
            var embed = new EmbedBuilder()
            {
                Title = $"{Data.Json.name} [{Data.Json.id}]",
                Description = "```md" + Environment.NewLine + $"<Height {Data.Json.height}> <Weight {Data.Json.weight}>```",
                Color = _Utils.Discord.GetRoleColor(Context.Channel),
                ThumbnailUrl = "https://raw.githubusercontent.com/PokeAPI/pokeapi/master/data/v2/sprites/pokemon/" + Data.Json.id + ".png"
            };
            await ReplyAsync("", false, embed.Build());
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
            PaginationFull.PaginatedMessage message = new PaginationFull.PaginatedMessage(Embeds, "", _Utils.Discord.GetRoleColor(Context.Channel as ITextChannel), Context.User);
            await _PagFull.SendPaginatedMessageAsync(Context.Channel, message);

        }
    }
    public class TestBot
    {
        public string BotName;
        public ulong BotID;
        public ulong BotOwner;
    }
    public class Media : ModuleBase
    {
        [Command("bottest")]
        public async Task BotTest()
        {
            List<TestBot> ALL = new List<TestBot>();
            var Req = _Utils.Http.JsonArray("https://bots.discord.pw/api/bots", _Config.Tokens.Dbots);
            Dictionary<ulong, ulong> Bots = new Dictionary<ulong, ulong>();
            foreach (var i in Req.Json)
            {
                if (i.owner_ids == null)
                {
                    Console.WriteLine($"Missing Owner - {i.name}");
                }
                else
                {
                    Bots.Add((ulong)i.user_id, (ulong)i.owner_ids[0]);
                }
            }
            List<IGuildUser> Users = new List<IGuildUser>();
            var GetUsers = await Context.Guild.GetUsersAsync();
            foreach(var i in GetUsers)
            {
                Users.Add(i);
            }

            foreach (var i in Users.Where(x => x.IsBot))
            {
                if (Bots.Keys.Contains(i.Id))
                {
                    Bots.TryGetValue(i.Id, out ulong Owner);
                    IGuildUser GU = await Context.Guild.GetUserAsync(Owner);
                    if (GU == null)
                    {
                        ALL.Add(new TestBot() { BotName = i.Username, BotID = i.Id, BotOwner = Owner });
                    }
                }
            }
            using (StreamWriter file = File.CreateText(_Config.BotPath + "BotUsers" + ".json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, ALL);
            }
            await Context.Channel.SendFileAsync(_Config.BotPath + "BotUsers" + ".json");
        }
    }

    public class Mod : ModuleBase
    {
        private readonly TimeSpan twoWeeks = TimeSpan.FromDays(14);
        private readonly PruneService _prune;
        private CommandService _Commands;
        public Mod(PruneService prune, CommandService Commands)
        {
            _prune = prune;
            _Commands = Commands;
        }

        [Command("prune")]
        public async Task Prune(string Option = "", string Pinned = "")
        {
            bool IsInt = int.TryParse(Option, out int INT);
            bool IsUlong = ulong.TryParse(Option, out ulong ULONG);
            if (Option == "")
            {
                var embed = new EmbedBuilder()
                {
                    Title = "Prune Commands",
                    Color = _Utils.Discord.GetRoleColor(Context.Channel),
                    Description = "```md\n[ p/prune all ]( Delete 100 messages )\n[ p/prune 30 ]( Delete 30 messages )\n[ p/prune bots ]( Delete bot messages )\n[ p/prune links ]( Delete messages that have a link )\n[ p/prune images ]( Delete messages that have an image )\n[ p/prune *Text ]( Delete messages that contains Text )\n[ p/prune @Mention ]( Deleted messages by a user )\n[ p/prune UserID ]( Delete messages by a userID )```",
                   // Footer = new EmbedFooterBuilder()
                   // {
                   //     Text = "Add -f at the end to delete pinned messages aswell"
                 //   }
                };
                await ReplyAsync("", false, embed.Build());
                return;

            }
            if (Context.Guild == null) _Log.ThrowError("Cannot run prune commands in Private Messages");
            IGuildUser Bot = await Context.Guild.GetCurrentUserAsync();
            if (!Bot.GuildPermissions.ManageMessages && !Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages) _Log.ThrowError("Bot does not have manage messages permission");
            await Context.Message.DeleteAsync();
            bool AllowPinned = false;
            if (Pinned == "-f")
            {
                AllowPinned = true;
            }
            if (!(Context.User as IGuildUser).GuildPermissions.ManageMessages) _Log.ThrowError("You do not have permission to Manage Messages");
            if (Option.ToLower() == "bot" || Option.ToLower() == "bots")
            {
                await _prune.PruneWhere((ITextChannel)Context.Channel, 30, (x) => x.IsPinned == AllowPinned && x.Author.IsBot).ConfigureAwait(false);
                await ReplyAsync($"`{Context.User.Username}#{Context.User.Discriminator} deleted bot messages`");
            }
            else if (Option.ToLower() == "link" || Option.ToLower() == "links")
            {
                await _prune.PruneWhere((ITextChannel)Context.Channel, 30, (x) => x.IsPinned == AllowPinned && x.Content.Contains("http://") | x.Content.Contains("https://")).ConfigureAwait(false);
                await ReplyAsync($"`{Context.User.Username}#{Context.User.Discriminator} deleted messages with links`");
            }
            else if (Option.ToLower() == "image" || Option.ToLower() == "images")
            {
                await _prune.PruneWhere((ITextChannel)Context.Channel, 30, (x) => x.IsPinned == AllowPinned && x.Attachments.Count != 0).ConfigureAwait(false);
                await ReplyAsync($"`{Context.User.Username}#{Context.User.Discriminator} deleted messages with images`");
            }
            else if (Option.Length == 2 || Option.Length == 1 && IsInt || Option == "all")
            {
                int PruneThis = 100;
                if (IsInt)
                {
                    PruneThis = INT;
                    if (INT < 0) _Log.ThrowError("`Ammount cannot be less than 0`");
                    if (INT > 100) _Log.ThrowError("`Ammount cannot be more than 30`");
                }
                await _prune.PruneWhere((ITextChannel)Context.Channel, PruneThis, (x) => x.IsPinned == AllowPinned).ConfigureAwait(false);
                    await ReplyAsync($"`{Context.User.Username}#{Context.User.Discriminator} deleted messages`");
            }
            else if (Option.StartsWith("*"))
            {
                await _prune.PruneWhere((ITextChannel)Context.Channel, 100, (x) => x.IsPinned == AllowPinned && x.Content.ToLower().Contains(Option.Replace("*", "").ToLower())).ConfigureAwait(false);
                await ReplyAsync($"`{Context.User.Username}#{Context.User.Discriminator} deleted messages that contain {Option.Replace("*", "")}`");
            }
            else if (Option.StartsWith("<@") || IsUlong)
            {
                ulong UserID = Convert.ToUInt64(_Utils.Discord.MentionToID(Option));
                await _prune.PruneWhere((ITextChannel)Context.Channel, 100, (x) => x.IsPinned == AllowPinned && x.Author.Id == UserID).ConfigureAwait(false);
                await ReplyAsync($"`{Context.User.Username}#{Context.User.Discriminator} deleted user messages`");
            }
            else
            {
              await ReplyAsync("`Invalid prune argument use p/prune`");
            }
        }
        
        [Command("pruneplaceholder1"), Remarks("prune all"), Summary("Delete 100 messages")]
        public async Task PruneAll() { }

        [Command("pruneplaceholder2"), Remarks("prune 30"), Summary("Delete 30 messages - Max is 100")]
        public async Task PruneCount() { }

        [Command("pruneplaceholder3"), Remarks("prune bots"), Summary("Delete bot messages")]
        public async Task PruneBots() { }

        [Command("pruneplaceholder4"), Remarks("prune links"), Summary("Delete messages that have a link")]
        public async Task PruneLinks() { }

        [Command("pruneplaceholder5"), Remarks("prune images"), Summary("Delete messages that have an image")]
        public async Task PruneImages() { }

        [Command("pruneplaceholder6"), Remarks("prune *Text"), Summary("Delete messages that contains Text")]
        public async Task PruneText() { }

        [Command("pruneplaceholder7"), Remarks("prune @Mention"), Summary("Deleted messages by a user")]
        public async Task PruneMention() { }

        [Command("pruneplaceholder8"), Remarks("prune UserID"), Summary("Delete messages by a userID")]
        public async Task PruneUserID() { }

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
            IGuildUser Bot = await Context.Guild.GetCurrentUserAsync();
            if (!Bot.GuildPermissions.BanMembers)
            {
                await ReplyAsync("`Bot does not have permission to ban user`");
                return;
            }
            IGuildUser GuildUser = await _Utils.Discord.MentionGetUser(Context.Guild, User);
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
            if (Bot.RoleIds.Where(x => x != Context.Guild.EveryoneRole.Id).Count() == 0)
            {
                await ReplyAsync("`Bot does not have any roles`");
                return;
            }

            IRole BotRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && Bot.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();
            IRole UserRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && GuildUser.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();
            if (BotRole.Position < UserRole.Position + 1)
            {
                await ReplyAsync("`Bot cannot ban user with same or lower role`");
                return;
            }
            if (Context.User.Id == Context.Guild.OwnerId)
            {
                await Context.Guild.AddBanAsync(GuildUser.Id, 0, $"[{Context.User.Username}]" + Reason);
                if (Bot.GuildPermissions.ManageMessages)
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
            if (Bot.GuildPermissions.ManageMessages)
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
            IGuildUser Bot = await Context.Guild.GetCurrentUserAsync();
            if (!Bot.GuildPermissions.KickMembers)
            {
                await ReplyAsync("`Bot does not have permission to kick user`");
                return;
            }
            IGuildUser GuildUser = await _Utils.Discord.MentionGetUser(Context.Guild, User);
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
            if (Bot.RoleIds.Where(x => x != Context.Guild.EveryoneRole.Id).Count() == 0)
            {
                await ReplyAsync("`Bot does not have any roles`");
                return;
            }
            IRole BotRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && Bot.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();
            IRole UserRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && GuildUser.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();
            if (BotRole.Position < UserRole.Position + 1)
            {
                await ReplyAsync("`Bot cannot kick user with same or lower role`");
                return;
            }
            if (Context.User.Id == Context.Guild.OwnerId)
            {
                await GuildUser.KickAsync($"[{Context.User.Username}]" + Reason);
                if (Bot.GuildPermissions.ManageMessages)
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
            if (Bot.GuildPermissions.ManageMessages)
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
            IGuildUser Bot = await Context.Guild.GetCurrentUserAsync();
            if (!Bot.GuildPermissions.BanMembers)
            {
                await ReplyAsync("`Bot does not have permission to ban user`");
                return;
            }
            IGuildUser GuildUser = await _Utils.Discord.MentionGetUser(Context.Guild, User.ToString());
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
            if (Bot.RoleIds.Where(x => x != Context.Guild.EveryoneRole.Id).Count() == 0)
            {
                await ReplyAsync("`Bot does not have any roles`");
                return;
            }

            IRole BotRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && Bot.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();

            if (Context.User.Id == Context.Guild.OwnerId)
            {
                await Context.Guild.AddBanAsync(User, 0, $"[{Context.User.Username}]" + Reason);
                if (Bot.GuildPermissions.ManageMessages)
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
            if (Bot.GuildPermissions.ManageMessages)
            {
                await Context.Message.DeleteAsync();
            }
            await ReplyAsync($"`{Context.User.Username} has banned id {User}`");
        }

        [Command("unban")]
        public async Task Unban(string Lookup = "")
        {
            if (Context.Guild == null)
            {
                await ReplyAsync("`Cannot use command in DMs`");
                return;
            }
            if (Lookup == "")
            {
                await ReplyAsync("`Unban a user id with p/unban (ID)`");
                return;
            }
            IGuildUser Bot = await Context.Guild.GetCurrentUserAsync();
            if (!Bot.GuildPermissions.BanMembers)
            {
                await ReplyAsync("`Bot does not have permission to ban user`");
                return;
            }
            IEnumerable<IRole> GuildRoles = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id);
           
            if (Bot.RoleIds.Where(x => x != Context.Guild.EveryoneRole.Id).Count() == 0)
            {
                await ReplyAsync("`Bot does not have any roles`");
                return;
            }

            IRole BotRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && Bot.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();
            ulong UserID = Convert.ToUInt64(Lookup);
            if (Context.User.Id == Context.Guild.OwnerId)
            {
                var GetBans = await Context.Guild.GetBansAsync();
                IUser GetUser = GetBans.Where(x => x.User.Id == UserID).First().User;
                if (GetUser == null)
                {
                    await ReplyAsync($"`{Lookup} is not on the banlist`");
                    return;
                }
                await Context.Guild.RemoveBanAsync(UserID);

                if (Bot.GuildPermissions.ManageMessages)
                {
                    await Context.Message.DeleteAsync();
                }
                await ReplyAsync($"`{Context.User.Username} has unbanned {GetUser.Username}`");
                return;
            }

            IGuildUser GuildStaff = Context.User as IGuildUser;
            if (!GuildStaff.GuildPermissions.BanMembers)
            {
                await ReplyAsync("`You do not have permission to unban user`");
                return;
            }

            if (GuildStaff.RoleIds.Where(x => x != Context.Guild.EveryoneRole.Id).Count() == 0)
            {
                await ReplyAsync("`You do not have any roles`");
                return;
            }

            IRole StaffRole = Context.Guild.Roles.Where(x => x.Id != Context.Guild.EveryoneRole.Id && GuildStaff.RoleIds.Contains(x.Id)).OrderByDescending(x => x.Position).First();

            var Bans = await Context.Guild.GetBansAsync();
            IUser User = Bans.Where(x => x.User.Id == UserID).First().User;
            if (User == null)
            {
                await ReplyAsync($"`{Lookup} is not on the banlist`");
                return;
            }
            if (Bot.GuildPermissions.ManageMessages)
            {
                await Context.Message.DeleteAsync();
            }
            await ReplyAsync($"`{Context.User.Username} has unbanned {User.Username}`");
        }

    }

    public class Dev : ModuleBase
    {
        _Bot _Bot;
        public Dev(_Bot Bot)
        {
            _Bot = Bot;
        }

        [Command("botlist"), Remarks("botlist"), Summary("All bot listing sites")]
        public async Task BotList()
        {
            IGuild Dbots = _Bot._Client.GetGuild(110373943822540800);
            IGuild DBL = _Bot._Client.GetGuild(264445053596991498);
            IGuild Novo = _Bot._Client.GetGuild(297462937646530562);
            IGuild Dir = _Bot._Client.GetGuild(278641206446260225);
            int Dbots_Users = 0;
            int Dbots_Bots = 0;
            int DBL_Users = 0;
            int DBL_Bots = 0;
            int Novo_Users = 0;
            int Novo_Bots = 0;
            int Dir_Users = 0;
            int Dir_Bots = 0;
            var embed = new EmbedBuilder();
            if (Dbots != null)
            {
                foreach (var i in await Dbots.GetUsersAsync())
                {
                    if (i.IsBot)
                    {
                        Dbots_Bots++;
                    }
                    else
                    {
                        Dbots_Users++;
                    }
                }
                embed.AddField("Dbots - Discord Bots", $"Users: {Dbots_Users} | Bots: {Dbots_Bots}");
            }
            if (DBL != null)
            {
                foreach (var i in await DBL.GetUsersAsync())
                {
                    if (i.IsBot)
                    {
                        DBL_Bots++;
                    }
                    else
                    {
                        DBL_Users++;
                    }
                }
                embed.AddField("DBL - Discord Bot List", $"Users: {DBL_Users} | Bots: {DBL_Bots}");
            }
            if (Novo != null)
            {
                foreach (var i in await Novo.GetUsersAsync())
                {
                    if (i.IsBot)
                    {
                        Novo_Bots++;
                    }
                    else
                    {
                        Novo_Users++;
                    }
                }
                embed.AddField("Novo - Novo Bot List", $"Users: {Novo_Users} | Bots: {Novo_Bots}");
            }
            if (Dir != null)
            {
                foreach (var i in await Dir.GetUsersAsync())
                {
                    if (i.IsBot)
                    {
                        Dir_Bots++;
                    }
                    else
                    {
                        Dir_Users++;
                    }
                }
                embed.AddField("Dir - The Directory", $"Users: {Dir_Users} | Bots: {Dir_Bots}");
            }
            await ReplyAsync("", false, embed.Build());
        }

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

        [Command("getbot"), Remarks("getbot (@Mention/ID)"), Summary("Get info of any bot")]
        public async Task GetBot(string Bot = "", string Api = "")
        {
            await ReplyAsync("Test");
        }

        [Command("getinvite"), Remarks("getinvite (@Mention/ID)"), Summary("Get invite of any bot")]
        public async Task GetInvite(string Bot = "", string Api = "")
        {
            await ReplyAsync("Test");
        }

        [Command("getowner"), Alias("getowners"), Remarks("getowner (@Mention/ID)"), Summary("Get owner or owners of any bot")]
        public async Task GetOwner(string Bot = "", string Api = "")
        {
            await ReplyAsync("Test");
        }

        [Command("getbots"), Remarks("getbots (@Mention/ID)"), Summary("Get bots that a user owns")]
        public async Task GetBots(string Bot = "", string Api = "")
        {
            await ReplyAsync("Test");
        }

        [Command("upvotes")]
        public async Task Upvotes(string Bot = "", string Api = "")
        {
            await ReplyAsync("Test");
        }
    }
}
