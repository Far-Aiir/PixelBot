using OverwatchAPI;
using System;

namespace Bot.Game
{
    public class _Overwatch
    {
        public enum RequestStatus
        {
            OK, UnknownRegion, UnknownPlayer, Other
        }
        public class Player
        {
            public RequestStatus Status = RequestStatus.Other;
            public string Region = "";
            public string Platform = "";
            public string User = "";
            public string ProfileUrl = "";
            public string ProfileIcon = "";
            public int Achievements = 0;
            public int Level = 0;
            public int CompetitiveRank = 0;
            public DateTime LastPlayed = DateTime.MinValue;
            public double CasualPlayed = 0;
            public double CasualPlaytime = 0;
            public double RankPlayed = 0;
            public double RankPlaytime = 0;
            public string RankIcon = "https://cdn2.iconfinder.com/data/icons/overwatch-players-icons/512/Overwatch-512.png";
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
            OWplayer.UpdateStatsAsync().GetAwaiter().GetResult();
            if (OWplayer == null)
            {
                Player.Status = RequestStatus.UnknownPlayer;
                return Player;
            }
            var CasualStats = OWplayer.CasualStats["AllHeroes"];
            var RankedStats = OWplayer.CompetitiveStats["AllHeroes"];
            //int Achievements = 0;

            foreach (var A in OWplayer.Achievements)
            {
                //foreach (var B in A)
                //{

                //if (A.IsUnlocked)
                //{
                //Achievements++;
                //}
                //}
            }

            if (OWplayer.CompetitiveRankImg != "")
            {
                Player.RankIcon = OWplayer.CompetitiveRankImg;
            }
            Player.ProfileIcon = OWplayer.ProfilePortraitURL;
            Player.Region = OWplayer.Region.ToString();
            Player.Platform = OWplayer.Platform.ToString();
            Player.Achievements = 0;
            Player.Level = OWplayer.PlayerLevel;
            Player.CompetitiveRank = OWplayer.CompetitiveRank;
            Player.ProfileUrl = OWplayer.ProfileURL;
            Player.LastPlayed = OWplayer.ProfileLastDownloaded;
            Player.CasualPlayed = OWplayer.CasualStats["AllHeroes"]["Game"]["Games Won"];
            Player.CasualPlaytime = OWplayer.CasualStats["AllHeroes"]["Game"]["Time Played"];
            Player.RankPlayed = OWplayer.CompetitiveStats["AllHeroes"]["Game"]["Games Won"];
            Player.RankPlaytime = OWplayer.CasualStats["AllHeroes"]["Game"]["Time Played"];

            return Player;
        }
    }
}
