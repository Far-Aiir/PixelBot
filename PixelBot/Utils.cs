using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Utils
{
    private static PagFull.Service _paginationfull = new PagFull.Service(Program._client);
    private static PagMin.Service _paginationmin = new PagMin.Service(Program._client);
    public static Dictionary<ulong, IGuildUser> GuildBotCache = new Dictionary<ulong, IGuildUser>();
    public static DateTime LongToDateTime(long LongNum)
        {
            DateTime Time = new DateTime();
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Time = epoch.AddMilliseconds(LongNum);
            return Time;
        }
    public static Color GetRoleColor(ICommandContext Command)
    {
        Color RoleColor = new Discord.Color(30, 0, 200);
        IGuildUser BotUser = null;
        if (Command.Guild != null)
        {
            Utils.GuildBotCache.TryGetValue(Command.Guild.Id, out BotUser);
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
    public static async void UpdateUptimeGuilds()
    {
        try
        {
            var Dbots = Program._client.GetGuild(110373943822540800);
            await Dbots.DownloadUsersAsync();
            var DbotsV2 = Program._client.GetGuild(264445053596991498);
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
            var message = new PagFull.Message(pages, title, Utils.GetRoleColor(context), context.User);
            await _paginationfull.SendPagFullMessageAsync(context.Channel, message);
        }
        else
        {
            if (PixelBot.GetPermissions(context.Channel as ITextChannel).AddReactions)
            {
                var message = new PagMin.Message(pages, title, Utils.GetRoleColor(context), context.User);
                await _paginationmin.SendPagMinMessageAsync(context.Channel, message);
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