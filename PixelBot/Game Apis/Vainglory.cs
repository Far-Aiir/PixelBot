using System;

namespace Bot.Game
{
    public class _Vainglory
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
            dynamic Request = _Utils.Http.JsonObject("https://api.dc01.gamelockerapp.com/shards/" + Region + "/players?filter[playerNames]=" + User, _Config.Tokens.Vainglory, "X-TITLE-ID", "semc-vainglory");

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
            dynamic Dynamic = _Utils.Http.JsonObject("https://api.dc01.gamelockerapp.com/shards/" + Region + "/matches?sort=createdAt&page[limit]=3&filter[playerNames]=" + User, _Config.Tokens.Vainglory, "X-TITLE-ID", "semc-vainglory");
            return Match;
        }
        #endregion
    }
}
