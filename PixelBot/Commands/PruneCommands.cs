using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using PixelBot.Services;

namespace PixelBot.Commands
{
    public class PruneCommandsLol : ModuleBase
    {
        private readonly TimeSpan twoWeeks = TimeSpan.FromDays(14);
        private readonly PruneService _prune;
        public PruneCommandsLol(PruneService prune)
        {
            _prune = prune;
        }
        [Command("iprune")]
        public async Task Prune()
        {
            var user = await Context.Guild.GetCurrentUserAsync().ConfigureAwait(false);

            await _prune.PruneWhere((ITextChannel)Context.Channel, 100, x => true).ConfigureAwait(false);
            
        }
    }
}
