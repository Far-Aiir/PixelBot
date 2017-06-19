using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
namespace PixelBot.Utils
{
    public class HttpRequest
    {
        public static string GetString(string Url)
        {
            string Response = "";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.Proxy = null;
                request.Method = WebRequestMethods.Http.Get;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream stream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            Response = reader.ReadToEnd();
                        }
                    }
                }
            return Response;
        }
        public static dynamic GetJsonObject(string Url, string Auth = "", string OtherHeader = "", string OtherValue = "")
        {
            dynamic Response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.Proxy = null;
                request.Method = WebRequestMethods.Http.Get;
                request.Accept = "application/json";
                if (Auth != "")
                {
                    request.Headers.Add("Authorization", Auth);
                }
                if (OtherHeader != "")
                {
                    request.Headers.Add(OtherHeader, OtherValue);
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream stream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string ResponseText = reader.ReadToEnd();
                            Response = Newtonsoft.Json.Linq.JObject.Parse(ResponseText);
                        }

                    }
                }
            }
            catch { }
            return Response;
        }
        public static dynamic GetJsonArray(string Url, string Auth = "", string OtherHeader = "", string OtherValue = "")
        {
            dynamic Response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.Proxy = null;
                request.Method = WebRequestMethods.Http.Get;
                request.Accept = "application/json";
                if (Auth != "")
                {
                    request.Headers.Add("Authorization", Auth);
                }
                if (OtherHeader != "")
                {
                    request.Headers.Add(OtherHeader, OtherValue);
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream stream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string ResponseText = reader.ReadToEnd();
                            Response = Newtonsoft.Json.Linq.JArray.Parse(ResponseText);
                        }
                    }
                }
            }
            catch { }
            return Response;
        }
    }

    public class DiscordUtils
    {
        public static async Task<IGuildUser> StringToUser(IGuild Guild, string Username)
        {
            IGuildUser GuildUser = null;
            if (Username.StartsWith("<@"))
            {
                string RealUsername = Username;
                RealUsername = RealUsername.Replace("<@", "").Replace(">", "");
                if (RealUsername.Contains("!"))
                {
                    RealUsername = RealUsername.Replace("!", "");
                }
                GuildUser = await Guild.GetUserAsync(Convert.ToUInt64(RealUsername));
            }
            else
            {
                GuildUser = await Guild.GetUserAsync(Convert.ToUInt64(Username));
            }
            return GuildUser;
        }
        public static string StringToUserID(string User)
        {
            if (User.StartsWith("("))
            {
                User = User.Replace("(", "");
            }
            if (User.EndsWith(")"))
            {
                User = User.Replace(")", "");
            }
            if (User.StartsWith("<@"))
            {
                User = User.Replace("<@", "").Replace(">", "");
                if (User.Contains("!"))
                {
                    User = User.Replace("!", "");
                }
            }
            return User;
        }
        public static Color GetRoleColor(ICommandContext Command)
        {
            Color RoleColor = new Discord.Color(30, 0, 200);
            IGuildUser BotUser = null;
            if (Command.Guild != null)
            {
                Utils.DiscordUtils.GuildBotCache.TryGetValue(Command.Guild.Id, out BotUser);
                if (BotUser.GetPermissions(Command.Channel as ITextChannel).EmbedLinks)
                {
                    if (BotUser != null)
                    {
                        if (BotUser.RoleIds.Count != 0)
                        {
                            foreach (var Role in BotUser.Guild.Roles.OrderBy(x => x.Position))
                            {
                                if (BotUser.RoleIds.Contains(Role.Id))
                                {
                                    RoleColor = Role.Color;
                                }
                            }
                        }
                    }
                }
            }
            return RoleColor;
        }

        private static PaginationService.Full.Service _paginationfull = PixelBot._PagFull;

        public static Dictionary<ulong, IGuildUser> GuildBotCache = new Dictionary<ulong, IGuildUser>();

        public static async void UpdateUptimeGuilds()
        {
            try
            {
                var Dbots = PixelBot._client.GetGuild(110373943822540800);
                await Dbots.DownloadUsersAsync();
                var DbotsV2 = PixelBot._client.GetGuild(264445053596991498);
                await DbotsV2.DownloadUsersAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static async Task SendPaginator(List<string> pages, string title, ICommandContext context, EmbedBuilder fallback)
        {
            IGuildUser PixelBot = null;
            GuildBotCache.TryGetValue(context.Guild.Id, out PixelBot);
            if (PixelBot.GetPermissions(context.Channel as ITextChannel).ManageMessages & PixelBot.GetPermissions(context.Channel as ITextChannel).AddReactions)
            {
                var message = new PaginationService.Full.Message(pages, title, Utils.DiscordUtils.GetRoleColor(context), true, "", context.User);

                await _paginationfull.SendPagFullMessageAsync(context.Channel, message).ConfigureAwait(false);
            }
            else
            {
                if (PixelBot.GetPermissions(context.Channel as ITextChannel).AddReactions)
                {
                    var message = new PaginationService.Full.Message(pages, title, Utils.DiscordUtils.GetRoleColor(context), false, "- Cannot delete reactions | No perm manage messages", context.User);
                    await _paginationfull.SendPagFullMessageAsync(context.Channel, message);
                }
                else
                {
                    if (PixelBot.GetPermissions(context.Channel as ITextChannel).EmbedLinks)
                    {
                        fallback.Color = GetRoleColor(context);
                        await context.Channel.SendMessageAsync("", false, fallback.Build());
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync("This bot needs permission `embed links` to function");
                    }
                }

            }
        }
    }

    public class OtherUtils
    {
        public static DateTime UnixToDateTime(long Unix)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Unix).ToLocalTime();
            return dtDateTime;
        }
    }
}