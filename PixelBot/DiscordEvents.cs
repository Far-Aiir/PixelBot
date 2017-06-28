using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PixelBot.Services;

namespace PixelBot
{
    public class DiscordEvents
    {
        public DiscordSocketClient _Client;

        public DiscordEvents(DiscordSocketClient Client)
        {
            _Client = Client;

            

            _Client.UserJoined += (u) =>
            {
                if (!Config.ALLUSERS.Exists(x => x.Id == u.Id))
                {
                    Config.ALLUSERS.Add(u);
                }
                return Task.CompletedTask;
            };

            _Client.UserLeft += (u) =>
            {
                if (!Config.ALLUSERS.Exists(x => x.Id == u.Id & x.GuildId == u.Guild.Id))
                {
                    Config.ALLUSERS.Add(u);
                }
                return Task.CompletedTask;
            };

        }
    }
}
