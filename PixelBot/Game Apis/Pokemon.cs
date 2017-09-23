using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Bot.Game
{
    public class _Poke
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

        public static dynamic GetPokemon(string Pokemon)
        {
            dynamic Data = null;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://pokeapi.co/api/v2/pokemon/" + Pokemon + "/");
            httpWebRequest.Method = WebRequestMethods.Http.Get;
            httpWebRequest.Accept = "application/json";
            HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
            if (response.StatusCode != HttpStatusCode.NotFound)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                var Req = readStream.ReadToEnd();
                Data = JObject.Parse(Req);
            }

            return Data;
        }
        #endregion
    }
}
