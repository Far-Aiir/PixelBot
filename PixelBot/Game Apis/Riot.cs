namespace Bot.Game
{
    class _Riot
    {
        public enum RegionTag
        {
            BR, EUNE, EUW, JP, KR, LAN, LAS, NA, OCE, TR, RU
        }
        public class UserRegion
        {
            public RegionTag Tag;
            public string Host = "";
        }
        public static void CheckGetApi(string RegionTag, out UserRegion Region, out string Request)
        {
            Request = "";
            CheckRegion(Region = new UserRegion(), RegionTag);
            if (Region.Host == "")
            {
                _Log.ThrowError("Unknown region use p/lol regions");
            }
            else
            {
                Request = _Utils.Http.JsonObject(Region.Host + "/lol/status/v3/shard-data", "", "X-Riot-Token", _Config.Tokens.Riot);
                
            }
        }

        public string GetChampions(UserRegion Region)
        {
            return "";
        }

        public static bool CheckRegion(UserRegion UserRegion, string Tag)
        {
            switch (Tag.ToUpper())
            {
                case "NA":
                    UserRegion.Tag = RegionTag.NA;
                    UserRegion.Host = "https://na1.api.riotgames.com";
                    break;
                case "EUW":
                    UserRegion.Tag = RegionTag.EUW;
                    UserRegion.Host = "https://euw1.api.riotgames.com";
                    break;
                case "EUN":
                case "EUNE":
                    UserRegion.Tag = RegionTag.EUNE;
                    UserRegion.Host = "https://eun1.api.riotgames.com";
                    break;
                case "LAN":
                    UserRegion.Tag = RegionTag.LAN;
                    UserRegion.Host = "https://la1.api.riotgames.com";
                    break;
                case "LAS":
                    UserRegion.Tag = RegionTag.LAS;
                    UserRegion.Host = "https://la2.api.riotgames.com";
                    break;
                case "BR":
                case "BRAZIL":
                    UserRegion.Tag = RegionTag.BR;
                    UserRegion.Host = "https://br1.api.riotgames.com";
                    break;
                case "JP":
                case "JAPAN":
                    UserRegion.Tag = RegionTag.JP;
                    UserRegion.Host = "https://jp1.api.riotgames.com";
                    break;
                case "RU":
                case "RUSSIA":
                    UserRegion.Tag = RegionTag.RU;
                    UserRegion.Host = "https://ru.api.riotgames.com";
                    break;
                case "TR":
                case "TURKEY":
                    UserRegion.Tag = RegionTag.TR;
                    UserRegion.Host = "https://tr1.api.riotgames.com";
                    break;
                case "OC":
                case "OCE":
                case "OCEANIA":
                    UserRegion.Tag = RegionTag.OCE;
                    UserRegion.Host = "https://oc1.api.riotgames.com";
                    break;
                case "KR":
                case "KOREA":
                    UserRegion.Tag = RegionTag.KR;
                    UserRegion.Host = "https://kr.api.riotgames.com";
                    break;
            }
            if (UserRegion.Host == "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
