﻿using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using RiotApi.Net.RestClient.Configuration;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OverwatchAPI;

namespace PixelBot.Apis
{
    public enum RequestStatus
    {
        OK, UnknownRegion, UnknownPlayer, Other
    }
    public class WOT
    {
        #region WOT
        public class Player
        {
            public RequestStatus Status = RequestStatus.Other;
            public string Region = "";
            public string User = "";
            public string ID = "";
            public DateTime CreatedAt = DateTime.MinValue;
            public DateTime LastBattle = DateTime.MinValue;
            public string Raiting = "";
            public string Win = "";
            public string Loss = "";
            public string Shots = "";
            public string Hits = "";
            public string Battles = "";
            public string Draws = "";
        }
        public static Player GetUserStats(string Region, string User)
        {
            Player Player = new Player();
                Player.Region = Region.ToUpper();
                Player.User = User;
                string RegionUrl = GetRegionUrl(Player, Region);
                if (Player.Status == RequestStatus.UnknownRegion)
                {
                    return Player;
                }
                dynamic GetID = Utils.HttpRequest.GetJsonObject(RegionUrl + "/wot/account/list/?application_id=" + PixelBot.Tokens.Wargaming + "&search=" + User + "&limit=1");
                if (GetID == null)
                {
                    Player.Status = RequestStatus.UnknownPlayer;
                    return Player;
                }
            int Count = GetID.meta.count;
            if (Count == 0)
            {
                Player.Status = RequestStatus.UnknownPlayer;
                return Player;
            }

                Player.ID = GetID.data[0].account_id;
                dynamic GetInfo = Utils.HttpRequest.GetJsonObject(RegionUrl + "/wot/account/info/?application_id=" + PixelBot.Tokens.Wargaming + "&account_id=" + Player.ID);
            JObject ConvertInfo = (JObject)GetInfo;
            dynamic Info = ConvertInfo.Last.First.First.First;
            if (Info == null)
                {
                    Player.Status = RequestStatus.Other;
                    return Player;
                }
                Player.Region = Info.Raiting;
            long Last = Info.last_battle_time;
            long Created = Info.created_at;
            Player.LastBattle = Utils.OtherUtils.UnixToDateTime(Last);
            Player.CreatedAt = Utils.OtherUtils.UnixToDateTime(Created);
            Player.Raiting = Info.global_rating;
            Player.Win = Info.statistics.all.wins;
            Player.Loss = Info.statistics.all.losses;
            Player.Battles = Info.statistics.all.battles;
            Player.Draws = Info.statistics.all.draws;
            Player.Hits = Info.statistics.all.hits;
            Player.Shots = Info.statistics.all.shots;
            Player.Status = RequestStatus.OK;
            return Player;
        }
        
        private static string GetRegionUrl(Player Player, string Region)
        {
            string ThisRegion = "null";
            switch (Region.ToUpper())
            {
                case "RU":
                    ThisRegion = "https://api.worldoftanks.ru";
                    break;
                case "EU":
                    ThisRegion = "https://api.worldoftanks.eu";
                    break;
                case "NA":
                    ThisRegion = "https://api.worldoftanks.com";
                    break;
                case "AS":
                    ThisRegion = "https://api.worldoftanks.asia";
                    break;
                default:
                    ThisRegion = "null";
                    break;
            }
            if (ThisRegion == "null")
            {
                Player.Status = RequestStatus.UnknownRegion;
            }
            return ThisRegion;
        }
        #endregion
    }
    public class LOL
    {
        #region LOL
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
        #endregion
    }
    public class Vainglory
    {
        #region Vainglory
        public class Player
        {
            public string Region;
            public string User;
            public string ID = "";
            public DateTime Created = DateTime.MinValue;
            public int KarmaLevel = 0;
            public int Level = 0;
            public string LifetimeGold = "";
            public int XP = 0;
            public int Wins = 0;
            public int Loss = 0;
            public int Played = 0;
            public int PlayedRanked = 0;
            public int SkillTier = 0;
        }
        public class Match
        {

        }
        public static Player GetPlayerStats(string Region, string User)
        {
            Player Player = new Player(); 
                dynamic Request = Utils.HttpRequest.GetJsonObject("https://api.dc01.gamelockerapp.com/shards/" + Region + "/players?filter[playerNames]=" + User, PixelBot.Tokens.Vainglory, "X-TITLE-ID", "semc-vainglory");
                
                Player.ID = Request.data[0].id;
                Player.User = User;
                Player.Region = Region;
                Player.Created = Request.data[0].attributes.createdAt;
                Player.KarmaLevel = Request.data[0].attributes.stats.karmaLevel;
                Player.Level = Request.data[0].attributes.stats.level;
                Player.LifetimeGold = Request.data[0].attributes.stats.lifetimeGold;
                Player.XP = Request.data[0].attributes.stats.xp;
                Player.Wins = Request.data[0].attributes.stats.wins;
                Player.Played = Request.data[0].attributes.stats.played;
                Player.Loss = Player.Wins = Player.Played;
                Player.SkillTier = Request.data[0].attributes.stats.skillTier;
                Player.PlayedRanked = Request.data[0].attributes.stats.played_ranked;
            return Player;
        }
        public static dynamic GetPlayerMatch(string Region, string User)
        {
            Match Match = new Match();
            dynamic Dynamic = Utils.HttpRequest.GetJsonObject("https://api.dc01.gamelockerapp.com/shards/" + Region + "/matches?sort=createdAt&page[limit]=3&filter[playerNames]=" + User, PixelBot.Tokens.Vainglory, "X-TITLE-ID", "semc-vainglory");
            return Match;
        }
        #endregion
    }
    public class Poke
    {
        #region Pokemon
        public class PokemonClass
        {
            public class Pokemon
            {
                public int id { get; set; }
                public string name { get; set; }
                public string height { get; set; }
                public string weight { get; set; }
                public List<string> types { get; set; }
            }
        }
        public class PokemonRevolution
        {
            public static List<string> GetMainTable(string url, string Class, int TableNum)
            {
                List<string> List = new List<string>();
                WebClient webClient = new WebClient();
                string page = webClient.DownloadString(url);
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(page);
                List<List<string>> table = doc.DocumentNode.SelectSingleNode("//table[" + TableNum + "][@class='" + Class + "']")
                                .Descendants("tr")
                                .Skip(1)
                                .Where(tr => tr.Elements("td").Count() > 1)
                                .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                                .ToList();
                int Count = 0;
                foreach (var Item in table)
                {
                    foreach (var Item2 in Item)
                    {
                        if (Count != 55)
                        {
                            Count++;
                            List.Add(Item2);
                        }
                    }
                }
                List.RemoveAt(0);
                List.RemoveAt(0);
                List.RemoveAt(0);
                List.RemoveAt(0);
                List.RemoveAt(0);

                return List;
            }

            public static List<string> GetPlaytimeTable(string url, string Class, int TableNum)
            {
                List<string> List = new List<string>();
                WebClient webClient = new WebClient();
                string page = webClient.DownloadString(url);
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(page);
                List<List<string>> table = doc.DocumentNode.SelectSingleNode("//table[" + TableNum + "][@class='" + Class + "']")
                            .Descendants("tr")
                            .Skip(1)
                            .Where(tr => tr.Elements("td").Count() > 1)
                            .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                            .ToList();
                int Count = 0;
                foreach (var Item in table)
                {
                    foreach (var Item2 in Item)
                    {
                        if (Count != 33)
                        {
                            Count++;
                            List.Add(Item2);
                        }
                    }
                }
                List.RemoveAt(0);
                List.RemoveAt(0);
                List.RemoveAt(0);
                return List;
            }
        }

        public static int GetPokemonID(string Pokemon)
        {
            int ID = 0;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://pokeapi.co/api/v2/pokemon/" + Pokemon + "/");
            httpWebRequest.Method = WebRequestMethods.Http.Get;
            httpWebRequest.Accept = "application/json";
            HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
            if (response.StatusCode != HttpStatusCode.NotFound)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                var Req = readStream.ReadToEnd();
                dynamic Data = JObject.Parse(Req);
                ID = Data.id;
            }

            return ID;
        }
        #endregion
    }
    public class Bots
    {
        #region Bots
        public enum BotApiType
        {
            DiscordBots, DiscordBotsList
        }
        public static List<BotClass> BotsList = new List<BotClass>();
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
        public static BotClass MainDiscordBots(string ID)
        {
            BotClass ThisBot = new BotClass();
            int LastDay = 0;
            if (BotsList.Exists(x => x.ID.ToString() == ID))
            {
                LastDay = BotsList.Find(x => x.ID.ToString() == ID).LastDay;
            }
            if (LastDay == 0 || LastDay == DateTime.Now.Day)
            {
                dynamic Data = null;
                Data = Utils.HttpRequest.GetJsonObject("https://bots.discord.pw/api/bots/" + ID, PixelBot.Tokens.Dbots);
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
                dynamic ServerCount = Utils.HttpRequest.GetJsonObject("https://bots.discord.pw/api/bots/" + ID + "/stats", PixelBot.Tokens.Dbots);
                ThisBot.ServerCount = ServerCount.stats[0].server_count;
            }
            else
            {
                ThisBot = BotsList.Find(x => x.ID.ToString() == ID);
            }
            JsonSerializer serializer = new JsonSerializer();
            if (ThisBot != null)
            {
                using (StreamWriter file = File.CreateText(PixelBot.BotPath + $"Users\\{ID.ToString()}.json"))
                {
                    serializer.Serialize(file, ThisBot);
                }
            }
            return ThisBot;
        }
        public static BotClass DiscordBotsList(string ID)
        {
            BotClass ThisBot = new BotClass();
            int LastDay = 0;
            if (BotsList.Exists(x => x.ID.ToString() == ID))
            {
                LastDay = BotsList.Find(x => x.ID.ToString() == ID).LastDay;
            }
            if (LastDay == 0 || LastDay == DateTime.Now.Day)
            {
                dynamic Data = null;
                Data = Utils.HttpRequest.GetJsonObject("https://discordbots.org/api/bots/" + ID, PixelBot.Tokens.Dbots);
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
            else
            {
                ThisBot = BotsList.Find(x => x.ID.ToString() == ID);
            }
            JsonSerializer serializer = new JsonSerializer();
            if (ThisBot != null)
            {
                using (StreamWriter file = File.CreateText(PixelBot.BotPath + $"Users\\{ID.ToString()}.json"))
                {
                    serializer.Serialize(file, ThisBot);
                }
            }
            return ThisBot;
        }
        #endregion
    }
    public class Overwatch
    {
        public class Player
        {
            public RequestStatus Status = RequestStatus.Other;
            public string Region = "";
            public string User = "";
            public string ProfileUrl = "";
            public int Achievements = 0;
            public int Level = 0;
            public int CompetitiveRank = 0;
            public DateTime LastPlayed = DateTime.MinValue;
            public double CasualPlayed = 0;
            public double CasualPlaytime = 0;
            public double RankPlayed = 0;
            public double RankPlaytime = 0;
        }
        public static Player GetPlayerStat(string User)
        {
            Player Player = new Player();
            if (!User.Contains("#") | OverwatchAPIHelpers.IsValidBattletag(User) == false)
            {
                Player.Status = RequestStatus.UnknownPlayer;
                return Player;
            }
            OverwatchPlayer OWplayer = new OverwatchPlayer(User);
             OWplayer.DetectPlatform().GetAwaiter();
            OWplayer.DetectRegionPC().GetAwaiter();
            OWplayer.UpdateStats().GetAwaiter();
            if (OWplayer == null)
            {
                Player.Status = RequestStatus.UnknownPlayer;
                return Player;
            }
            var CasualStats = OWplayer.CasualStats.GetHero("AllHeroes");
            var RankedStats = OWplayer.CompetitiveStats.GetHero("AllHeroes");
            int Achievements = 0;

            foreach (var A in OWplayer.Achievements)
            {
                foreach (var B in A)
                {
                    
                    if (B.IsUnlocked)
                    {
                        Achievements++;
                    }
                }
            }
            Player.Achievements = Achievements;
            Player.Level = OWplayer.PlayerLevel;
            Player.CompetitiveRank = OWplayer.CompetitiveRank;
            Player.ProfileUrl = OWplayer.ProfileURL;
            Player.LastPlayed = OWplayer.ProfileLastDownloaded;
            Player.CasualPlayed = CasualStats.GetCategory("Game").GetStat("Games Won").Value;
            Player.CasualPlaytime = CasualStats.GetCategory("Game").GetStat("Time Played").Value;
            Player.RankPlayed = RankedStats.GetCategory("Game").GetStat("Games Won").Value;
            Player.RankPlaytime = RankedStats.GetCategory("Game").GetStat("Time Played").Value;
            
            return Player;
        }
    }
}