using PixelBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;

namespace PixelBot
{
    public class ServiceProvider
    {
        public IServiceProvider AddServices(DiscordSocketClient _Client, CommandService _CommandService, CommandHandler _CommandHandler)
        {
            var TwitchService = new Twitch(_Client);
            var PruneService = new PruneService();
            IServiceProvider _Services = new ServiceCollection()
               .AddSingleton(_Client)
               .AddSingleton(PruneService)
               .AddSingleton(new PaginationService.Full.Service(_Client))
               .AddSingleton(new PaginationService.Min.Service(_Client))
               .AddSingleton(new BotInfo())
               .AddSingleton(new DiscordEvents(_Client))
               .AddSingleton(_CommandHandler)
               .AddSingleton(_CommandService)
               .AddSingleton(new BlacklistService())
               .AddSingleton(new Stats(_Client))
               .AddSingleton(TwitchService)
            .AddSingleton<PixelBot>()
               .BuildServiceProvider();
            _CommandHandler.AddServices(_Services);
            return _Services;
        }
    }
}
