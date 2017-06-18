using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class OwnerCommands : ModuleBase
{
    [Command("o")]
    [RequireOwner]
    public async Task OwnerList()
    {
        await Context.Message.DeleteAsync();
        await Context.Channel.SendMessageAsync("`chatlog (ID) | invite (ID) | info (ID) | leavehere | leave (ID) | botcol | clear | blacklist (add/remove/list) | toggle`");
    }
    
    [Command("owner")]
    public async Task Owner()
    {
        var embed = new EmbedBuilder()
        {
            Title = "xXBuilderBXx#9113 owns this bot",
            Description = "<@190590364871032834>",
            Color = PixelBot.Utils.DiscordUtils.GetRoleColor(Context)
        };
        await Context.Channel.SendMessageAsync("", false, embed);
    }

    [Group("o")]
    public class MC : ModuleBase
    {
        [Command("chatlog")]
        [RequireOwner]
        public async Task Chatlog(ulong ID = 0)
        {
            if (ID == 0)
            {
                PixelBot.Properties.Settings.Default.ChatLogGuild = 0;
                await Context.Channel.SendMessageAsync("`Chat log has been turned off`");
            }
            else
            {
                if (ID == 1)
                {
                    PixelBot.Properties.Settings.Default.ChatLogGuild = 1;
                    await Context.Channel.SendMessageAsync("`Chat log has been set to ALL`");
                }
                else
                {
                    PixelBot.Properties.Settings.Default.ChatLogGuild = ID;
                    await Context.Channel.SendMessageAsync($"`Chat log has been set to {ID}`");
                }
            }

        }

        [Command("invite")]
        [RequireOwner]
        public async Task Invite(ulong ID)
        {
            IGuild Guild = await Context.Client.GetGuildAsync(ID);
            IGuildChannel Chan = await Guild.GetDefaultChannelAsync();
            var Invite = await Chan.CreateInviteAsync();
            await Context.Channel.SendMessageAsync(Invite.Code);
        }

        [Command("info")]
        [RequireOwner]
        public async Task Oinfo(ulong ID)
        {
            try
            {
                var Guild = await Context.Client.GetGuildAsync(ID);
                string Owner = "NO OWNER";
                var Users = await Guild.GetUsersAsync();
                try
                {
                    IGuildUser ThisOwner = await Guild.GetOwnerAsync();
                    Owner = $"{ThisOwner.Username}#{ThisOwner.Discriminator} - {ThisOwner.Id}";
                }
                catch
                {

                }
                var embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = $"{Guild.Name}",
                        IconUrl = Guild.IconUrl
                    },
                    Description = $"Owner: {Owner}" + Environment.NewLine + $"Users {Users.Where(x => !x.IsBot).Count()}/{Users.Where(x => x.IsBot).Count()} Bots",
                    Color = PixelBot.Utils.DiscordUtils.GetRoleColor(Context),
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Created {Guild.CreatedAt.Day}/{Guild.CreatedAt.Month}/{Guild.CreatedAt.Year}"
                    }
                };
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            catch
            {
                await Context.Channel.SendMessageAsync($"`Cannot find guild {ID}`");
            }
        }

        [Command("leavehere")]
        [RequireOwner]
        public async Task Leavehere()
        {
            await Context.Guild.LeaveAsync();
        }

        [Command("clear")]
        [RequireOwner]
        public async Task Clear()
        {
            await Context.Message.DeleteAsync();
            Console.Clear();
            Console.WriteLine("Console cleared");
            await Context.Channel.SendMessageAsync("`Console cleared`");
        }

        [Command("toggle")]
        [RequireOwner]
        public async Task Toggle()
        {
            await Context.Message.DeleteAsync();
            PixelBot.Properties.Settings.Default.CommandOutput = !PixelBot.Properties.Settings.Default.CommandOutput;
            await Context.Channel.SendMessageAsync($"Command output set to {PixelBot.Properties.Settings.Default.CommandOutput}");
        }

        [Command("botcol")]
        [RequireOwner]
        public async Task Botcol(int Number = 100)
        {
            await Context.Message.DeleteAsync();
            var Guilds = await Context.Client.GetGuildsAsync();
            List<string> GuildList = new List<string>();
            foreach (var Guild in Guilds)
            {
                if (Guild.Id == 110373943822540800 || Guild.Id == 264445053596991498)
                {

                }
                else
                {
                    IGuildUser Owner = null;
                    try
                    {
                        Owner = await Guild.GetOwnerAsync();
                        var Users = await Guild.GetUsersAsync();
                        if (Users.Count(x => x.IsBot) >= Number || Users.Count(x => !x.IsBot) == 1)
                        {
                            GuildList.Add($"{Guild.Name} ({Guild.Id}) - Owner: {Owner.Username} ({Owner.Id}) - {Users.Where(x => !x.IsBot).Count()}/{Users.Where(x => x.IsBot).Count()}");
                        }
                    }
                    catch
                    {
                        GuildList.Add($"{Guild.Name} ({Guild.Id}) - NO OWNER!");
                    }
                }

            }
            string AllGuilds = string.Join(Environment.NewLine, GuildList.ToArray());
                IDMChannel DM = await Context.User.CreateDMChannelAsync();
                foreach (var g in GuildList)
                {
                    await DM.SendMessageAsync(g);
                }
        }

        [Command("leave")]
        [RequireOwner]
        public async Task Leave(ulong ID)
        {
            await Context.Message.DeleteAsync();
            IGuild Guild = null;
            try
            {
                Guild = await Context.Client.GetGuildAsync(ID);
            }
            catch
            {
                await Context.Channel.SendMessageAsync($"`Could not find guild by id {ID}`");
                return;
            }
            try
            {
                IGuildUser Owner = await Guild.GetOwnerAsync();
                await Guild.LeaveAsync();
                await Context.Channel.SendMessageAsync($"`Left guild {Guild.Name} - {Guild.Id} | Owned by {Owner.Username}#{Owner.Discriminator}`");
            }
            catch
            {
                await Guild.LeaveAsync();
                await Context.Channel.SendMessageAsync($"`Left guild {Guild.Name} - {Guild.Id}`");
            }
        }

        [Command("blacklist")]
        [RequireOwner]
        public async Task Blacklist(string Option = "", ulong ID = 0)
        {
            await Context.Message.DeleteAsync();
            if (Option == "")
            {
                await Context.Channel.SendMessageAsync("`Invalid option > list/add/remove`");
                return;
            }
            if (Option == "list")
            {
                List<string> GuildList = new List<string>();
                foreach (var Item in PixelBot.Properties.Settings.Default.Blacklist)
                {
                    GuildList.Add(Item);
                }
                string BlacklistList = string.Join(Environment.NewLine, GuildList.ToArray());
                await Context.Channel.SendMessageAsync("**Bot Guild Blacklist**" + Environment.NewLine + BlacklistList);
            }
            if (Option == "add")
            {
                if (ID == 0)
                {
                    await Context.Channel.SendMessageAsync("Input an Guild ID");
                    return;
                }
                if (PixelBot.Properties.Settings.Default.Blacklist.Contains(ID.ToString()))
                {
                    await Context.Channel.SendMessageAsync($"{ID} is already in the blacklist");
                }
                else
                {
                    try
                    {
                        IGuild Guild = await Context.Client.GetGuildAsync(ID);
                        PixelBot.Properties.Settings.Default.Blacklist.Add(ID.ToString());
                        PixelBot.Properties.Settings.Default.Save();
                        await Context.Channel.SendMessageAsync($"`Adding {Guild.Name} {ID} to blacklist`");
                        await Guild.LeaveAsync();
                    }
                    catch
                    {
                        PixelBot.Properties.Settings.Default.Blacklist.Add(ID.ToString());
                        PixelBot.Properties.Settings.Default.Save();
                        await Context.Channel.SendMessageAsync($"`Adding {ID} to blacklist`");
                    }
                }
            }
            if (Option == "remove")
            {
                if (ID == 0)
                {
                    await Context.Channel.SendMessageAsync("Input an Guild ID");
                    return;
                }
                if (PixelBot.Properties.Settings.Default.Blacklist.Contains(ID.ToString()))
                {
                    PixelBot.Properties.Settings.Default.Blacklist.Remove(ID.ToString());
                    PixelBot.Properties.Settings.Default.Save();
                    await Context.Channel.SendMessageAsync($"`Removed {ID} from blacklist`");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"`{ID} is not in the blacklist`");
                }
            }
        }
    }
}

