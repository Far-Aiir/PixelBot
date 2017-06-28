using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Services
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
    public class BotInfo
    {
        private static BotClass MainDiscordBots(string ID)
        {
            BotClass ThisBot = new BotClass();
            int LastDay = 0;
            if (LastDay == 0 || LastDay == DateTime.Now.Day)
            {
                dynamic Data = null;
                Data = Utils.HttpRequest.GetJsonObject("https://bots.discord.pw/api/bots/" + ID, Config._Configs.Dbots);
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
                dynamic ServerCount = Utils.HttpRequest.GetJsonObject("https://bots.discord.pw/api/bots/" + ID + "/stats", Config._Configs.Dbots);
                ThisBot.ServerCount = ServerCount.stats[0].server_count;
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
                Data = Utils.HttpRequest.GetJsonObject("https://discordbots.org/api/bots/" + ID, Config._Configs.Dbots);
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

        public async Task GetInfo(ITextChannel Channel, string User, string Api)
        {
            IGuildUser GuildUser = await Utils.DiscordUtils.StringToUser(Channel.Guild, User);
            if (GuildUser == null)
            {
                await Channel.SendMessageAsync("`User not found`").ConfigureAwait(false);
                return;
            }
            BotClass GetBot = null;
            if (Channel.Guild.Id == 264445053596991498 || Api.Contains("list"))
            {
                Api = "list";
                GetBot = BotInfo.DiscordBotsList(Utils.DiscordUtils.StringToUserID(User));

                if (GetBot == null)
                {
                    GetBot = BotInfo.MainDiscordBots(Utils.DiscordUtils.StringToUserID(User));
                    Api = "";
                }
            }
            else
            {
                GetBot = BotInfo.MainDiscordBots(Utils.DiscordUtils.StringToUserID(User));
                if (GetBot == null)
                {
                    GetBot = BotInfo.DiscordBotsList(Utils.DiscordUtils.StringToUserID(User));
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
                    //Color = Utils.DiscordUtils.GetRoleColor(Conte),
                    Description = $"{Bot} | {Owner} ```md" + Environment.NewLine + $"<Prefix {GetBot.Prefix}> <Lib {GetBot.Libary}>" + Environment.NewLine + $"<Guilds {GetBot.ServerCount}> <Tags {string.Join(", ", GetBot.Tags)}>" + Environment.NewLine + $"<Points {GetBot.Points}> <Certified {GetBot.Certified}>```" + Links + Environment.NewLine + GetBot.Description
                };
            }
            else
            {
                embed = new EmbedBuilder()
                {
                    //Color = Utils.DiscordUtils.GetRoleColor(Context),
                    Description = $"{Bot} | {Owner} ```md" + Environment.NewLine + $"<Prefix {GetBot.Prefix}> <Lib {GetBot.Libary}>" + Environment.NewLine + $"<Guilds {GetBot.ServerCount}> <Tags {string.Join(", ", GetBot.Tags)}>```" + Links + Environment.NewLine + GetBot.Description
                };
            }
            await Channel.SendMessageAsync("", false, embed).ConfigureAwait(false);
        }
        public async Task GetInvite(ITextChannel Channel, string User)
        {
            IGuildUser GuildUser = await Utils.DiscordUtils.StringToUser(Channel.Guild, User);
            if (GuildUser == null)
            {
                await Channel.SendMessageAsync("`User not found`").ConfigureAwait(false);
                return;
            }
        }
        public async Task GetOwner(ITextChannel Channel, string User)
        {
            IGuildUser GuildUser = await Utils.DiscordUtils.StringToUser(Channel.Guild, User);
            if (GuildUser == null)
            {
                await Channel.SendMessageAsync("`User not found`").ConfigureAwait(false);
                return;
            }
        }
        public async Task GetBots(ITextChannel Channel, string User)
        {
            IGuildUser GuildUser = await Utils.DiscordUtils.StringToUser(Channel.Guild, User);
            if (GuildUser == null)
            {
                await Channel.SendMessageAsync("`User not found`").ConfigureAwait(false);
                return;
            }
        }
    }
}
