using System.Timers;
using System.Net;
using Discord.WebSocket;

namespace Bot.Services
{
    public class DiscordStatus
    {
        private DiscordSocketClient _Client;
        private _Bot _Bot;
        public DiscordStatus(DiscordSocketClient Client, _Bot Bot)
        {
            if (_Config.DevMode == false)
            {
                _Client = Client;
                _Bot = Bot;
                Timer StatusTimer = new Timer()
                { Interval = 60000 };
                StatusTimer.Elapsed += StatusTimer_Elapsed;
                StatusTimer.Start();
            }
        }

        private void StatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool HasError = false;
            WebClient WC = new WebClient();
            string Page = WC.DownloadString("https://status.discordapp.com/");
            HtmlAgilityPack.HtmlDocument HtmlDoc = new HtmlAgilityPack.HtmlDocument();
            HtmlDoc.LoadHtml(Page);
            var Root = HtmlDoc.DocumentNode;
            try
            {
                var Nodes = Root.SelectNodes("//div[@class='update']");
                if (Nodes != null)
                {
                    HasError = true;
                }
            }
            catch
            {
            }
            if (HasError == true)
            {
                _Bot.SetStatusAsync($"{_Config.Prefix}help [!Discord Issue!] p/discord").GetAwaiter();
            }
            else
            {
                if (_Client.CurrentUser.Game.ToString().Contains("[!Discord Issue!]"))
                {

                    _Bot.SetStatusAsync("").GetAwaiter();
                }
            }
        }
    }
}
