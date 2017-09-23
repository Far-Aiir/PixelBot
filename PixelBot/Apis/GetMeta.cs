using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.IO;

namespace Bot.Apis
{
    public class GetMeta
    {
       public static void Test()
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri("http://store.steampowered.com/app/252950/Rocket_League/"));
            request.Method = WebRequestMethods.Http.Get;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());

            String responseString = reader.ReadToEnd();

            response.Close();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(responseString);

            String title = (from x in doc.DocumentNode.Descendants()
                            where x.Name.ToLower() == "title"
                            select x.InnerText).FirstOrDefault();

            String desc = (from x in doc.DocumentNode.Descendants()
                           where x.Name.ToLower() == "meta"
                           && x.Attributes["name"] != null
                           && x.Attributes["name"].Value.ToLower() == "description"
                           select x.Attributes["content"].Value).FirstOrDefault();

            List<String> imgs = (from x in doc.DocumentNode.Descendants()
                                 where x.Name.ToLower() == "img"
                                 select x.Attributes["src"].Value).ToList<String>();
            Console.WriteLine(title);
            Console.WriteLine(desc);
        }
    }
}
