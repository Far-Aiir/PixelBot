using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PixelBot;

namespace Bot.Services
{
    public class PaginationFull
    {
        const string BACK = "◀";
        const string NEXT = "▶";
        const string STOP = "⏹";
        private readonly Dictionary<ulong, PaginatedMessage> _messages;
        private readonly DiscordSocketClient _client;

        public PaginationFull(DiscordSocketClient client)
        {
            _messages = new Dictionary<ulong, PaginatedMessage>();
            _client = client;
            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionRemoved;
        }

        public async Task<IUserMessage> SendPaginatedMessageAsync(IMessageChannel channel, PaginatedMessage paginated, bool Limited = true)
        {
            IUserMessage message = null;
            if (Limited == true)
            {
                message = await channel.SendMessageAsync("Limited - No perms Manage Messages", embed: paginated.GetEmbed());
            }
            else
            {
                message = await channel.SendMessageAsync("", embed: paginated.GetEmbed());
            }
            if (message != null)
            {
                await message.AddReactionAsync(new Emoji(BACK));
                await message.AddReactionAsync(new Emoji(NEXT));
                await message.AddReactionAsync(new Emoji(STOP));

                _messages.Add(message.Id, paginated);
            }
            return message;
        }

        internal async Task OnReactionAdded(Cacheable<IUserMessage, ulong> messageParam, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await messageParam.GetOrDownloadAsync();
            if (message == null)
            {
                return;
            }
            if (!reaction.User.IsSpecified)
            {
                return;
            }
            if (message.Author.Id != _client.CurrentUser.Id) return;
            if (_messages.TryGetValue(message.Id, out PaginatedMessage page))
            {
                if (reaction.UserId == _client.CurrentUser.Id) return;
                if (page.User != null && reaction.UserId != page.User.Id)
                {
                    if (!message.Content.StartsWith("Limited"))
                    {
                        var _ = message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                    }
                    return;
                }
                if (!message.Content.StartsWith("Limited"))
                {
                    await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                }
                switch(reaction.Emote.Name)
                {
                    case BACK:
                        if (page.CurrentPage != 1)
                        {
                            page.CurrentPage--;
                            await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        }
                        break;
                    case NEXT:
                        if (page.CurrentPage != page.Count)
                        {
                            page.CurrentPage++;
                            await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        }
                        break;
                    case STOP:
                        await message.DeleteAsync();
                        _messages.Remove(message.Id);
                        break;
                }
            }
        }

        internal async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> messageParam, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await messageParam.GetOrDownloadAsync();
            if (message == null)
            {
                return;
            }
            if (!message.Content.StartsWith("Limited")) return;
            if (!reaction.User.IsSpecified)
            {
                return;
            }
            if (message.Author.Id != _client.CurrentUser.Id) return;
            if (_messages.TryGetValue(message.Id, out PaginatedMessage page))
            {
                if (reaction.UserId == _client.CurrentUser.Id) return;
                if (page.User != null && reaction.UserId != page.User.Id)
                {
                    return;
                }
                switch (reaction.Emote.Name)
                {
                    case BACK:
                        if (page.CurrentPage != 1)
                        {
                            page.CurrentPage--;
                            await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        }
                        break;
                    case NEXT:
                        if (page.CurrentPage != page.Count)
                        {
                            page.CurrentPage++;
                            await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        }
                        break;
                    case STOP:
                        await message.DeleteAsync();
                        _messages.Remove(message.Id);
                        break;
                }
            }
        }

        public class PaginatedMessage
        {
            public PaginatedMessage(IEnumerable<string> pages, string title = "", Color? embedColor = null, IUser user = null)
                => new PaginatedMessage(pages.Select(x => new Page { Description = x }), title, embedColor, user);
            public PaginatedMessage(IEnumerable<Page> pages, string title = "", Color? embedColor = null, IUser user = null)
            {
                var embeds = new List<Embed>();
                int i = 1;
                foreach (var page in pages)
                {

                    var builder = new EmbedBuilder()
                        .WithColor(embedColor ?? Color.Default)
                        .WithTitle(title)
                        .WithDescription(page?.Description ?? "")
                        .WithFooter(footer =>
                        {
                            footer.Text = $"Page {i++}/{pages.Count()}";
                        });
                    if (page.ImageUrl != null)
                    {
                        builder.ImageUrl = new Uri(page.ImageUrl);
                    }
                    if (page.ThumbnailUrl != null)
                    {
                        builder.ThumbnailUrl = new Uri(page.ThumbnailUrl);
                    }
                    if (page.Author != null)
                    {
                        builder.Author = page.Author;
                    }
                    if (page.Fields != null)
                    {
                        builder.Fields = page.Fields?.ToList();
                    }
                    embeds.Add(builder.Build());
                }
                Pages = embeds;
                Title = title;
                EmbedColor = embedColor ?? Color.Default;
                User = user;
                CurrentPage = 1;
            }

            internal Embed GetEmbed()
            {
                return Pages.ElementAtOrDefault(CurrentPage - 1);
            }

            internal string Title { get; }
            internal Color EmbedColor { get; }
            internal IReadOnlyCollection<Embed> Pages { get; }
            internal IUser User { get; }
            internal int CurrentPage { get; set; }
            internal int Count => Pages.Count;
        }

        public class Page
        {
            public EmbedAuthorBuilder Author { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public string ThumbnailUrl { get; set; }
            public IReadOnlyCollection<EmbedFieldBuilder> Fields { get; set; }
        }
    }
}