using Bot.Utils;
using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Game
{
    public class BotClass
    {
        public ulong ID = 0;
        public string Name = "";
        public string Api = "";
        public string Invite = "";
        public string Github = "";
        public string Description = "";
        public string Libary = "";
        public List<ulong> OwnersID = new List<ulong>();
        public string Prefix = "";
        public string Website = "";
        public int LastDay;
        public string ServerCount = "0";
        public List<string> Tags = new List<string>();
        public int Points = 0;
        public bool Certified = false;
    }
    public class _BotApi
    {
        private static BotClass MainDiscordBots(string ID)
        {
            BotClass ThisBot = new BotClass();
            int LastDay = 0;
            if (LastDay == 0 || LastDay == DateTime.Now.Day)
            {
                dynamic Data = null;
                Data = _Utils_Http.GetJsonObject("https://bots.discord.pw/api/bots/" + ID, _Config.Tokens.Dbots) ?? null;
                if (Data == null)
                {
                    ThisBot = null;
                    return ThisBot;
                }
                ThisBot.ID = Convert.ToUInt64(ID);
                ThisBot.Invite = Data.invite_url;
                ThisBot.Description = Data.description;
                ThisBot.Libary = Data.library;
                ThisBot.Name = Data.name;
                foreach (var i in Data.owner_ids)
                {
                    ThisBot.OwnersID.Add(Convert.ToUInt64(i));
                }
                ThisBot.Prefix = Data.prefix;
                ThisBot.Website = Data.website;
                ThisBot.Api = "(Main) Discord Bots";
                dynamic ServerCount = _Utils_Http.GetJsonObject("https://bots.discord.pw/api/bots/" + ID + "/stats", _Config.Tokens.Dbots) ?? null;
                if (ServerCount != null)
                {
                    ThisBot.ServerCount = ServerCount.stats[0].server_count;
                }
            }
            return ThisBot;
        }
        private static BotClass DiscordBotsList(string ID)
        {
            BotClass ThisBot = new BotClass();
            int LastDay = 0;
            if (LastDay == 0 || LastDay == DateTime.Now.Day)
            {
                dynamic Data = null;
                Data = _Utils_Http.GetJsonObject("https://discordbots.org/api/bots/" + ID, _Config.Tokens.Dbots);
                if (Data == null)
                {
                    ThisBot = null;
                    return ThisBot;
                }
                ThisBot.ID = Convert.ToUInt64(ID);
                ThisBot.Invite = Data.invite;
                ThisBot.Description = Data.shortdesc;
                ThisBot.Libary = Data.lib;
                ThisBot.Name = Data.username;
                foreach (var i in Data.owners)
                {
                    ThisBot.OwnersID.Add(Convert.ToUInt64(i));
                }
                ThisBot.Prefix = Data.prefix;
                ThisBot.Certified = Data.certifiedBot;
                ThisBot.Github = Data.github;
                ThisBot.Points = Data.points;
                ThisBot.Website = Data.website;
                ThisBot.Api = "Discord Bots List";
                ThisBot.ServerCount = Data.server_count;
            }
            return ThisBot;
        }

        public static async Task GetInfo(ITextChannel Channel, string User, string Api)
        {
            BotClass GetBot = null;
            if (Channel.Guild.Id == 264445053596991498 || Api.Contains("list"))
            {
                Api = "list";
                GetBot = _BotApi.DiscordBotsList(_Utils_Discord.MentionToID(User));

                if (GetBot == null)
                {
                    GetBot = _BotApi.MainDiscordBots(_Utils_Discord.MentionToID(User));
                    Api = "";
                }
            }
            else
            {
                GetBot = _BotApi.MainDiscordBots(_Utils_Discord.MentionToID(User));
                if (GetBot == null)
                {
                    GetBot = _BotApi.DiscordBotsList(_Utils_Discord.MentionToID(User));
                    Api = "list";
                }
            }
            if (GetBot == null)
            {
                await Channel.SendMessageAsync("`Could not find bot`").ConfigureAwait(false);
                return;
            }
            string Owner = $"Owner: {GetBot.OwnersID[0].ToString()}";
            string Bot = $"{GetBot.Name} - {GetBot.ID}";
            IGuildUser GetOwner = await Channel.Guild.GetUserAsync(GetBot.OwnersID[0]).ConfigureAwait(false);
            if (GetOwner != null)
            {
                Owner = $"Owner: <@{GetBot.OwnersID[0].ToString()}>";
            }
            IGuildUser GetBotUser = await Channel.Guild.GetUserAsync(GetBot.ID).ConfigureAwait(false);
            if (GetBotUser != null)
            {
                Bot = $"{GetBot.Name} <@{GetBot.ID.ToString()}>";
            }
            string Links = "";
            if (GetBot.Invite != "")
            {
                if (GetBot.Invite.Contains("redirect_uri"))
                {
                    Links = $"Invite: {GetBot.Invite}" + Environment.NewLine;
                }
                else
                {
                    Links = $"[Invite]({GetBot.Invite}) ";
                }
            }
            if (GetBot.Website != "")
            {
                Links = Links + $"[Website]({GetBot.Website}) ";
            }
            if (GetBot.Github != "")
            {
                Links = Links + $"[Github]({GetBot.Github})";
            }
            if (GetBot.OwnersID.Count != 1)
            {
                if (GetBot.OwnersID.Count == 2)
                {
                    Owner = $"{Owner} +{GetBot.OwnersID.Count() - 1} Other";
                }
                else
                {
                    Owner = $"{Owner} +{GetBot.OwnersID.Count() - 1} Others";
                }
            }
            var embed = new EmbedBuilder();
            if (Api.Contains("list"))
            {
                embed = new EmbedBuilder()
                {
                    Color = _Utils_Discord.GetRoleColor(Channel as ITextChannel),
                    Description = $"{Bot} | {Owner} ```md" + Environment.NewLine + $"<Prefix {GetBot.Prefix}> <Lib {GetBot.Libary}>" + Environment.NewLine + $"<Guilds {GetBot.ServerCount}> <Tags {string.Join(", ", GetBot.Tags)}>" + Environment.NewLine + $"<Points {GetBot.Points}> <Certified {GetBot.Certified}>```" + Links + Environment.NewLine + GetBot.Description
                };
            }
            else
            {
                embed = new EmbedBuilder()
                {
                    Color = _Utils_Discord.GetRoleColor(Channel as ITextChannel),
                    Description = $"{Bot} | {Owner} ```md" + Environment.NewLine + $"<Prefix {GetBot.Prefix}> <Lib {GetBot.Libary}>" + Environment.NewLine + $"<Guilds {GetBot.ServerCount}> <Tags {string.Join(", ", GetBot.Tags)}>```" + Links + Environment.NewLine + GetBot.Description
                };
            }
            await Channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        public static void GetInvite(ITextChannel Channel, string User, string Api)
        {
            IGuildUser GuildUser = _Utils_Discord.MentionGetUser(Channel.Guild, User).GetAwaiter().GetResult();
            BotClass GetBot = null;
            if (Channel.Guild.Id == 264445053596991498 || Api.Contains("list"))
            {
                Api = "list";
                GetBot = _BotApi.DiscordBotsList(_Utils_Discord.MentionToID(User));

                if (GetBot == null)
                {
                    GetBot = _BotApi.MainDiscordBots(_Utils_Discord.MentionToID(User));
                    Api = "";
                }
            }
            else
            {
                GetBot = _BotApi.MainDiscordBots(_Utils_Discord.MentionToID(User));
                if (GetBot == null)
                {
                    GetBot = _BotApi.DiscordBotsList(_Utils_Discord.MentionToID(User));
                    Api = "list";
                }
            }
            if (GetBot == null)
            {
                Channel.SendMessageAsync("`Could not find bot`").GetAwaiter().GetResult();
                return;
            }
            if (GetBot.Invite == null)
            {
                Channel.SendMessageAsync("`This bot does not have an invite`").GetAwaiter().GetResult();
                return;
            }
            var embed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                { Name = $"Invite for {GetBot.Name}", IconUrl = GuildUser?.GetAvatarUrl() },
                Description = $"<@{GetBot.ID}> [Invite This Bot]({GetBot.Invite})",
                Color = _Utils_Discord.GetRoleColor(Channel as ITextChannel)
            };
            Channel.SendMessageAsync("", false, embed.Build()).GetAwaiter();
        }

        public static void GetOwner(ITextChannel Channel, string User, string Api)
        {
                IGuildUser GuildUser = _Utils_Discord.MentionGetUser(Channel.Guild, User).GetAwaiter().GetResult();
                BotClass GetBot = null;
                if (Channel.Guild.Id == 264445053596991498 || Api.Contains("list"))
                {
                    Api = "list";
                    GetBot = _BotApi.DiscordBotsList(_Utils_Discord.MentionToID(User));

                    if (GetBot == null)
                    {
                        GetBot = _BotApi.MainDiscordBots(_Utils_Discord.MentionToID(User));
                        Api = "";
                    }
                }
                else
                {
                    GetBot = _BotApi.MainDiscordBots(_Utils_Discord.MentionToID(User));
                    if (GetBot == null)
                    {
                        GetBot = _BotApi.DiscordBotsList(_Utils_Discord.MentionToID(User));
                        Api = "list";
                    }
                }
                if (GetBot == null)
                {
                    Channel.SendMessageAsync("`Could not find bot`").GetAwaiter().GetResult();
                    return;
                }
                List<string> Owners = new List<string>();
                foreach (var Owner in GetBot.OwnersID)
                {
                    Owners.Add($"<@{Owner}>");
                }
                var embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    { Name = $"Owners for {GetBot.Name}", IconUrl = GuildUser?.GetAvatarUrl() },
                    Description = $"{string.Join(Environment.NewLine, Owners)}",
                    Color = _Utils_Discord.GetRoleColor(Channel as ITextChannel)
                };
            Channel.SendMessageAsync("", false, embed.Build()).GetAwaiter();
        }

        public static void GetBots(ITextChannel Channel, string ID)
        {
            if (ID.Contains("<@"))
            {
                ID = _Utils_Discord.MentionToID(ID);
            }
            dynamic Json = _Utils_Http.GetJsonObject("https://discordbots.org/api/bots?search=owners," + ID, _Config.Tokens.DbotsV2);
            List<string> Bots = new List<string>();
            JArray a = (JArray)Json.results;

            foreach (JObject o in a.Children<JObject>())
            {
                string User = "";
                string Discrim = "";
                string GetID = "";
                foreach (JProperty p in o.Properties())
                {
                    if (p.Name == "username")
                    {
                        User = (string)p.Value;
                    }
                    if (p.Name == "discriminator")
                    {
                        Discrim = (string)p.Value;
                    }
                    if (p.Name == "id")
                    {
                        GetID = (string)p.Value;
                    }
                }
                Bots.Add($"`{User}#{Discrim}` <@{GetID}>");
            }
            if (Bots.Count == 0)
            {
                var embederror = new EmbedBuilder()
                {
                    Description = $"<@{ID}> Does not have any bots"
                };
                Channel.SendMessageAsync("", false, embederror.Build()).GetAwaiter();
                return;
            }
            var embed = new EmbedBuilder()
            {
                Description = $"<@{ID}> owns {Bots.Count} Bots" + Environment.NewLine + string.Join(Environment.NewLine, Bots),
                Color = _Utils_Discord.GetRoleColor(Channel as ITextChannel)
            };
            Channel.SendMessageAsync("", false, embed.Build()).GetAwaiter();
        }

        public static void GetUpvotes(ITextChannel Channel, string ID)
        {
            if (ID.Contains("<@"))
            {
                ID = _Utils_Discord.MentionToID(ID);
            }
            dynamic Json = _Utils_Http.GetJsonArray("https://discordbots.org/api/bots/" + ID + "/votes", _Config.Tokens.DbotsV2);
            List<string> Users = new List<string>();
            JArray a = (JArray)Json;
            foreach (JObject o in a.Children<JObject>())
            {
                string User = "";
                string Discrim = "";
                string GetID = "";
                foreach (JProperty p in o.Properties())
                {
                    if (p.Name == "username")
                    {
                        User = (string)p.Value;
                    }
                    if (p.Name == "discriminator")
                    {
                        Discrim = (string)p.Value;
                    }
                    if (p.Name == "id")
                    {
                        GetID = (string)p.Value;
                    }
                }
                Users.Add($"`{User}#{Discrim}` <@{GetID}>");
            }
            if (Users.Count == 0)
            {
                Channel.SendMessageAsync("This bot does not have any upvotes");
                return;
            }
            var embed = new EmbedBuilder()
            {
                Description = $"<@{ID}> {Users.Count} Upvotes" + Environment.NewLine + string.Join(Environment.NewLine, Users),
                Color = _Utils_Discord.GetRoleColor(Channel as ITextChannel)
            };
            Channel.SendMessageAsync("", false, embed.Build()).GetAwaiter();
        }
    }

}