using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Classes
{
    public class GameInfo
    {
        public string Name = "";
        public string Desc = "";
        public string Url = "";
        public string ImageUrl = "";
        public int Rating = 0;
        public int RatingUsers = 0;
        public List<string> Websites = new List<string>();
        public GameInfo(dynamic Data)
        {
            Name = Data.name;
            Desc = Data.summary;
            Url = Data.url;
            Rating = Convert.ToInt32(Data.rating);
            RatingUsers = Data.rating_count;
            ImageUrl = "https:" + Data.cover.url;
            foreach(var i in Data.websites)
            {
                switch((int)i.category)
                {
                    case 1:
                        Websites.Insert(0, $"[Official]({i.url})");
                        break;
                    case 2:
                        Websites.Add($"[Wikia]({i.url})");
                        break;
                    case 3:
                        Websites.Add($"[Wikipedia]({i.url})");
                        break;
                    case 4:
                        Websites.Add($"[Facebook]({i.url})");
                        break;
                    case 5:
                        Websites.Add($"[Twitter]({i.url})");
                        break;
                    case 6:
                        Websites.Add($"[Twitch]({i.url})");
                        break;
                    case 8:
                        Websites.Add($"[Instagram]({i.url})");
                        break;
                    case 9:
                        Websites.Add($"[Youtube]({i.url})");
                        break;
                    case 10:
                        Websites.Add($"[Iphone]({i.url})");
                        break;
                    case 11:
                        Websites.Add($"[Ipad]({i.url})" );
                        break;
                    case 12:
                        Websites.Add($"[Android]({i.url})");
                        break;
                    case 13:
                        Websites.Insert(1, $"[Steam]({i.url})");
                        break;
                }
            }
        }
    }
    
}
