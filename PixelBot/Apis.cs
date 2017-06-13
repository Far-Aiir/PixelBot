using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using RiotApi.Net.RestClient.Configuration;
using System.Data;
using Newtonsoft.Json.Linq;

public class _Apis
{
    public class WOT
    {
        public class API
        {
            public static string GetUserID(string Region, [Remainder] string User)
            {
                string WOTURL = Region + "/wot/account/list/?application_id=";
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(WOTURL + Program._Token.Wargaming + "&search=" + User + "&limit=1");
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
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(WOTURL + Program._Token.Wargaming + "&account_id=" + ID);
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
                Player.LastBattle = _Utils.UnixToDateTime(Last);
                Player.CreatedAt = _Utils.UnixToDateTime(Created);
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
            public static string GetRegionUrl(string Region)
            {
                string ThisRegion = "null";
                switch (Region.ToLower())
                {
                    case "ru":
                        ThisRegion = "https://api.worldoftanks.ru";
                        break;
                    case "eu":
                        ThisRegion = "https://api.worldoftanks.eu";
                        break;
                    case "na":
                        ThisRegion = "https://api.worldoftanks.com";
                        break;
                    case "as":
                        ThisRegion = "https://api.worldoftanks.asia";
                        break;
                }
                return ThisRegion;
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
    public class _Class_Vainglory
    {
        public class API
        {
            public static dynamic GetPlayer(string Region, string Player)
            {
                dynamic Dynamic = null;
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.dc01.gamelockerapp.com/shards/" + Region + "/players?filter[playerNames]=" + Player);
                httpWebRequest.Method = WebRequestMethods.Http.Get;
                httpWebRequest.Headers.Add("Authorization", Program._Token.Vainglory);
                httpWebRequest.Headers.Add("X-TITLE-ID", "semc-vainglory");
                httpWebRequest.Accept = "application/json";

                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                var Req = readStream.ReadToEnd();
                Dynamic = Newtonsoft.Json.Linq.JObject.Parse(Req);
                return Dynamic;
            }
            public static dynamic GetPlayerMatch(string Region, string Player)
            {
                dynamic Dynamic = null;
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.dc01.gamelockerapp.com/shards/" + Region + "/matches?sort=createdAt&page[limit]=3&filter[playerNames]=" + Player);
                httpWebRequest.Method = WebRequestMethods.Http.Get;
                httpWebRequest.Headers.Add("Authorization", Program._Token.Vainglory);
                httpWebRequest.Headers.Add("X-TITLE-ID", "semc-vainglory");
                httpWebRequest.Accept = "application/json";

                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                var Req = readStream.ReadToEnd();
                Dynamic = Newtonsoft.Json.Linq.JObject.Parse(Req);
                return Dynamic;
            }
        }
    }
    public class Poke
    {
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
    }
}