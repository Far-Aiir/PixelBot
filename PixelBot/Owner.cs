using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PixelBot
{
    public class OwnerCommands : ModuleBase
    {
        [Command("owner")]
        [Alias("o")]
        [RequireOwner]
        public async Task Owner()
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync("`leave (ID) | botcol (dm) | clear | blacklist (add/remove/list) | toggle`");
        }

        [Command("o clear")]
        [RequireOwner]
        public async Task Clear()
        {
            await Context.Message.DeleteAsync();
            Console.Clear();
            Console.WriteLine("Console cleared");
            await Context.Channel.SendMessageAsync("`Console cleared`");
        }

        [Command("o toggle")]
        [RequireOwner]
        public async Task Toggle()
        {
            await Context.Message.DeleteAsync();
            Properties.Settings.Default.CommandOutput = !Properties.Settings.Default.CommandOutput;
            await Context.Channel.SendMessageAsync($"Command output set to {Properties.Settings.Default.CommandOutput}");
        }

        [Command("o botcol")]
        [RequireOwner]
        public async Task Botcol(string Option = "", int Number = 100)
        {
            await Context.Message.DeleteAsync();
            var Guilds = await Context.Client.GetGuildsAsync();
            List<string> GuildList = new List<string>();
            foreach (var Guild in Guilds)
            {
                if (Guild.Id != 110373943822540800)
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
            if (Option == "dm")
            {
                IDMChannel DM = await Context.User.CreateDMChannelAsync();
                await DM.SendMessageAsync(AllGuilds);
            }
            else
            {
                Console.WriteLine("-----");
                Console.WriteLine(AllGuilds);
                Console.WriteLine("-----");
            }
        }

        [Command("o leave")]
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

        [Command("o blacklist")]
        [RequireOwner]
        public async Task Leave(string Option = "", ulong ID = 0)
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
                foreach (var Item in Properties.Settings.Default.Blacklist)
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
                if (Properties.Settings.Default.Blacklist.Contains(ID.ToString()))
                {
                    await Context.Channel.SendMessageAsync($"{ID} is already in the blacklist");
                }
                else
                {
                    try
                    {
                        IGuild Guild = await Context.Client.GetGuildAsync(ID);
                        Properties.Settings.Default.Blacklist.Add(ID.ToString());
                        Properties.Settings.Default.Save();
                        await Context.Channel.SendMessageAsync($"Adding {Guild.Name} {ID} to blacklist");
                        await Guild.LeaveAsync();
                    }
                    catch
                    {
                        Properties.Settings.Default.Blacklist.Add(ID.ToString());
                        Properties.Settings.Default.Save();
                        await Context.Channel.SendMessageAsync($"Adding {ID} to blacklist");
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
                if (Properties.Settings.Default.Blacklist.Contains(ID.ToString()))
                {
                    Properties.Settings.Default.Blacklist.Remove(ID.ToString());
                    Properties.Settings.Default.Save();
                    await Context.Channel.SendMessageAsync($"Removed {ID} from blacklist");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{ID} is not in the blacklist");
                }
            }
        }
    }
}
