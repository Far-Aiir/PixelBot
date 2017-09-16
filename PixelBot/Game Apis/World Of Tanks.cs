using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Bot.Game
{
    public class _WOT
    {
        public enum RequestStatus
        {
            OK, UnknownRegion, UnknownPlayer, Other
        }
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
            Player Player = new Player()
            {
                Region = Region.ToUpper(),
                User = User
            };
            string RegionUrl = GetRegionUrl(Player, Region);
            if (Player.Status == RequestStatus.UnknownRegion)
            {
                return Player;
            }
            dynamic GetID = Utils._Utils_Http.GetJsonObject(RegionUrl + "/wot/account/list/?application_id=" + _Config.Tokens.Wargaming + "&search=" + User + "&limit=1");
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
            dynamic GetInfo = Utils._Utils_Http.GetJsonObject(RegionUrl + "/wot/account/info/?application_id=" + _Config.Tokens.Wargaming + "&account_id=" + Player.ID);
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
            Player.LastBattle = Utils._Utils_Other.UlongToDateTime(Last);
            Player.CreatedAt = Utils._Utils_Other.UlongToDateTime(Created);
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
    }
}
