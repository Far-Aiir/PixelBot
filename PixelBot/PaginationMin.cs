﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PagMin
{
    public class Service
    {
        const string FIRST = "⏮";
        const string BACK = "◀";
        const string NEXT = "▶";
        const string END = "⏭";
        const string STOP = "⏹";


        private readonly Dictionary<ulong, Message> _messages;

        public Service(DiscordSocketClient Client)
        {
            _messages = new Dictionary<ulong, Message>();
            Client.ReactionAdded += OnReactionAdded;
            Client.ReactionRemoved += OnReactionRemoved;
        }

        public async Task<IUserMessage> SendPagMinMessageAsync(IMessageChannel channel, Message paginated)
        {
            var message = await channel.SendMessageAsync("", embed: paginated.GetEmbed());
            await message.AddReactionAsync(new Emoji(BACK));
            await message.AddReactionAsync(new Emoji(NEXT));
            await message.AddReactionAsync(new Emoji(STOP));
            _messages.Add(message.Id, paginated);

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
            if (_messages.TryGetValue(message.Id, out Message page))
            {
                if (reaction.UserId == Program._client.CurrentUser.Id) return;
                if (page.User != null && reaction.UserId != page.User.Id)
                {
                   // var _ = message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                    return;
                }
                //await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                switch (reaction.Emote.Name)
                {
                    case BACK:
                        if (page.CurrentPage == 1) break;
                        page.CurrentPage--;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        break;
                    case NEXT:
                        if (page.CurrentPage == page.Count) break;
                        page.CurrentPage++;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        break;
                    case STOP:
                        await message.DeleteAsync();
                        _messages.Remove(message.Id);
                        return;
                    default:
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
            if (!reaction.User.IsSpecified)
            {
                return;
            }
            if (_messages.TryGetValue(message.Id, out Message page))
            {
                if (reaction.UserId == Program._client.CurrentUser.Id) return;
                if (page.User != null && reaction.UserId != page.User.Id)
                {
                    // var _ = message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                    return;
                }
                //await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                switch (reaction.Emote.Name)
                {
                    case BACK:
                        if (page.CurrentPage == 1) break;
                        page.CurrentPage--;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        break;
                    case NEXT:
                        if (page.CurrentPage == page.Count) break;
                        page.CurrentPage++;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                        break;
                    case STOP:
                        await message.DeleteAsync();
                        _messages.Remove(message.Id);
                        return;
                    default:
                        break;
                }
            }
        }
    }

    public class Message
    {
        public Message(IReadOnlyCollection<string> pages, string title = "", Color? embedColor = null, IUser user = null)
        {
            Pages = pages;
            Title = title;
            EmbedColor = embedColor ?? Color.Default;
            User = user;
            CurrentPage = 1;
        }

        internal Embed GetEmbed()
        {
            return new EmbedBuilder()
                .WithColor(EmbedColor)
                .WithTitle(Title)
                .WithDescription(Pages.ElementAtOrDefault(CurrentPage - 1) ?? "")
                .WithFooter(footer =>
                {
                    footer.Text = $"Page {CurrentPage}/{Count} - Cannot delete reactions | No perm manage messages";
                })
                .Build();
        }

        internal string Title { get; }
        internal Color EmbedColor { get; }
        internal IReadOnlyCollection<string> Pages { get; }
        internal IUser User { get; }
        internal int CurrentPage { get; set; }
        internal int Count => Pages.Count;
    }
}