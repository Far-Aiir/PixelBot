using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RiotApi.Net.RestClient.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class _Utils
{
    private static PaginationService.Full.Service _paginationfull = Program._PagFull;
    public static Dictionary<ulong, IGuildUser> GuildBotCache = new Dictionary<ulong, IGuildUser>();

    public static DateTime EpochToDateTime(long LongNum)
    {
        DateTime Time = new DateTime();
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Time = epoch.AddMilliseconds(LongNum);
        return Time;
    }

    public static DateTime UnixToDateTime(long Unix)
    {
        DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(Unix).ToLocalTime();
        return dtDateTime;
    }

    public static async Task<IGuildUser> GetFullUserAsync(IGuild Guild, string Username)
    {
        IGuildUser GuildUser = null;
        if (Username.StartsWith("<@"))
        {
            string RealUsername = Username;
            RealUsername = RealUsername.Replace("<@", "").Replace(">", "");
            if (RealUsername.Contains("!"))
            {
                RealUsername = RealUsername.Replace("!", "");
            }
            GuildUser = await Guild.GetUserAsync(Convert.ToUInt64(RealUsername));
        }
        else
        {
            GuildUser = await Guild.GetUserAsync(Convert.ToUInt64(Username));
        }
        return GuildUser;
    }

    public static Color GetRoleColor(ICommandContext Command)
    {
        Color RoleColor = new Discord.Color(30, 0, 200);
        IGuildUser BotUser = null;
        if (Command.Guild != null)
        {
            _Utils.GuildBotCache.TryGetValue(Command.Guild.Id, out BotUser);
            if (BotUser.GetPermissions(Command.Channel as ITextChannel).EmbedLinks)
            {
                if (BotUser != null)
                {
                    if (BotUser.RoleIds.Count != 0)
                    {
                        foreach (var Role in BotUser.Guild.Roles.OrderBy(x => x.Position))
                        {
                            if (BotUser.RoleIds.Contains(Role.Id))
                            {
                                RoleColor = Role.Color;
                            }
                        }
                    }
                }
            }
        }
        return RoleColor;
    }

    public static async void UpdateUptimeGuilds()
    {
        try
        {
            var Dbots = Program._client.GetGuild(110373943822540800);
            await Dbots.DownloadUsersAsync();
            var DbotsV2 = Program._client.GetGuild(264445053596991498);
            await DbotsV2.DownloadUsersAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static async Task SendPaginator(List<string> pages, string title, ICommandContext context, EmbedBuilder fallback)
    {
        IGuildUser PixelBot = null;
        GuildBotCache.TryGetValue(context.Guild.Id, out PixelBot);
        if (PixelBot.GetPermissions(context.Channel as ITextChannel).ManageMessages & PixelBot.GetPermissions(context.Channel as ITextChannel).AddReactions)
        {
            var message = new PaginationService.Full.Message(pages, title, _Utils.GetRoleColor(context), true, "", context.User);
            
            await _paginationfull.SendPagFullMessageAsync(context.Channel, message);
        }
        else
        {
            if (PixelBot.GetPermissions(context.Channel as ITextChannel).AddReactions)
            {
                var message = new PaginationService.Full.Message(pages, title, _Utils.GetRoleColor(context), false, "- Cannot delete reactions | No perm manage messages", context.User);
                await _paginationfull.SendPagFullMessageAsync(context.Channel, message);
            }
            else
            {
                if (PixelBot.GetPermissions(context.Channel as ITextChannel).EmbedLinks)
                {
                    fallback.Color = GetRoleColor(context);
                    await context.Channel.SendMessageAsync("", false, fallback.Build());
                }
                else
                {
                    await context.Channel.SendMessageAsync("This bot needs permission `embed links` to function");
                }
            }

        }
    }

    public class WOT
    {


        public static dynamic t(string ID)
        {
            dynamic Stat = null;
            string WOTURL = "https://api.worldoftanks.eu/wot/account/info/?application_id=";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(WOTURL + Program._Token.Wargaming + "&account_id=" + ID);
            httpWebRequest.Method = WebRequestMethods.Http.Get;
            httpWebRequest.Accept = "application/json";
            HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            var Req = readStream.ReadToEnd();
            dynamic JA = Newtonsoft.Json.Linq.JObject.Parse(Req);
            //string ID = JA.data[0].account_id;
            return Stat;
        }
    }

}
public class TokenClass
{
    public string Discord { get; set; } = "";
    public string MysqlHost { get; set; } = "";
    public string MysqlUser { get; set; } = "";
    public string MysqlPass { get; set; } = "";
    public string Twitch { get; set; } = "";
    public string TwitchAuth { get; set; } = "";
    public string Steam { get; set; } = "";
    public string Osu { get; set; } = "";
    public string Xbox { get; set; } = "";
    public string Vainglory { get; set; } = "";
    public string Youtube { get; set; } = "";
    public string Dbots { get; set; } = "";
    public string DbotsV2 { get; set; } = "";
    public string Riot { get; set; } = "";
    public string Wargaming { get; set; } = "";
}