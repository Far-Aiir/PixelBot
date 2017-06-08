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

public class Utils
{
    private static PagFull.Service _paginationfull = new PagFull.Service(Program._client);
    private static PagMin.Service _paginationmin = new PagMin.Service(Program._client);
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
            Utils.GuildBotCache.TryGetValue(Command.Guild.Id, out BotUser);
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
            var message = new PagFull.Message(pages, title, Utils.GetRoleColor(context), context.User);
            await _paginationfull.SendPagFullMessageAsync(context.Channel, message);
        }
        else
        {
            if (PixelBot.GetPermissions(context.Channel as ITextChannel).AddReactions)
            {
                var message = new PagMin.Message(pages, title, Utils.GetRoleColor(context), context.User);
                await _paginationmin.SendPagMinMessageAsync(context.Channel, message);
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
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(WOTURL + Program.TokenMap.Wargaming + "&account_id=" + ID);
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
public class WOT
{
    public class API
    {
        public static string GetUserID(string Region, [Remainder] string User)
        {
            string WOTURL = Region + "/wot/account/list/?application_id=";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(WOTURL + Program.TokenMap.Wargaming + "&search=" + User + "&limit=1");
            httpWebRequest.Method = WebRequestMethods.Http.Get;
            httpWebRequest.Accept = "application/json";
            HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            var Req = readStream.ReadToEnd();
            dynamic JA = Newtonsoft.Json.Linq.JObject.Parse(Req);
            string ID = JA.data[0].account_id;
            return ID;
        }
        public static Data.PlayerData GetUserData(string Region, string ID = "")
        {
            Data.PlayerData Player = new Data.PlayerData();
            dynamic Stat = null;
            string WOTURL = Region + "/wot/account/info/?application_id=";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(WOTURL + Program.TokenMap.Wargaming + "&account_id=" + ID);
            httpWebRequest.Method = WebRequestMethods.Http.Get;
            httpWebRequest.Accept = "application/json";
            HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            var Req = readStream.ReadToEnd();
            var Get = Newtonsoft.Json.Linq.JObject.Parse(Req);
            Stat = Get.Last.First.First.First;
            long Last = Stat.last_battle_time;
            long Created = Stat.created_at;
            Player.LastBattle = Utils.UnixToDateTime(Last);
            Player.CreatedAt = Utils.UnixToDateTime(Created);
            Player.Raiting = Stat.global_rating;
            Player.Win = Stat.statistics.all.wins;
            Player.Loss = Stat.statistics.all.losses;
            Player.Battles = Stat.statistics.all.battles;
            Player.Draws = Stat.statistics.all.draws;
            Player.Hits = Stat.statistics.all.hits;
            Player.Shots = Stat.statistics.all.shots;
            return Player;
        }
    }
    public class Data
    {
        public class PlayerData
        {
            public DateTime CreatedAt { get; set; }
            public DateTime LastBattle { get; set; }
            public string Raiting { get; set; }
            public string Win { get; set; }
            public string Loss { get; set; }
            public string Shots { get; set; }
            public string Hits { get; set; }
            public string Battles { get; set; }
            public string Draws { get; set; }
        }
        public static string Regions(string Tag)
        {
            string Region = "null";
            switch (Tag.ToLower())
            {
                case "ru":
                    Region = "https://api.worldoftanks.ru";
                    break;
                case "eu":
                    Region = "https://api.worldoftanks.eu";
                    break;
                case "na":
                    Region = "https://api.worldoftanks.com";
                    break;
                case "as":
                    Region = "https://api.worldoftanks.asia";
                    break;
            }
            return Region;
        }
    }
}
public class LOL
{
    
    public class Data
    {
        public static RiotApiConfig.Regions GetRegion(string Tag)
        {
            RiotApiConfig.Regions UserRegion = RiotApiConfig.Regions.Global;
            switch (Tag.ToUpper())
            {
                case "NA":
                    UserRegion = RiotApiConfig.Regions.NA;
                    break;
                case "EUW":
                    UserRegion = RiotApiConfig.Regions.EUW;
                    break;
                case "EUN":
                case "EUNE":
                    UserRegion = RiotApiConfig.Regions.EUNE;
                    break;
                case "LAN":
                    UserRegion = RiotApiConfig.Regions.LAN;
                    break;
                case "LAS":
                    UserRegion = RiotApiConfig.Regions.LAS;
                    break;
                case "BR":
                case "BRAZIL":
                    UserRegion = RiotApiConfig.Regions.BR;
                    break;
                case "JP":
                case "JAPAN":
                    UserRegion = RiotApiConfig.Regions.TR;
                    break;
                case "RU":
                case "RUSSIA":
                    UserRegion = RiotApiConfig.Regions.RU;
                    break;
                case "TR":
                case "TURKEY":
                    UserRegion = RiotApiConfig.Regions.TR;
                    break;
                case "OC":
                case "OCE":
                case "OCEANIA":
                    UserRegion = RiotApiConfig.Regions.OCE;
                    break;
                case "KR":
                case "KOREA":
                    UserRegion = RiotApiConfig.Regions.KR;
                    break;
            }
            return UserRegion;
        }
    }
}
public class Vainglory
{
    public class API
    {

    }
}