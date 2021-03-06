﻿using System.Threading.Tasks;
using CommunityBot.Features.GlobalAccounts;
using Discord;
using Discord.Commands;

namespace CommunityBot.Modules
{
    [Group("Announcements"), Alias("Announcement"), Summary("Settings for announcements")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Announcement : ModuleBase<SocketCommandContext>
    {
        [Command("SetChannel"), Alias("Set"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Sets the channel where to post announcements")]
        public async Task SetAnnouncementChannel(ITextChannel channel)
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            guildAcc.AnnouncementChannelId = channel.Id;
            GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);
            await ReplyAsync("The Announcement-Channel has been set to " + channel.Mention);
        }

        [Command("UnsetChannel"), Alias("Unset", "Off"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Turns posting announcements to a channel off")]
        public async Task UnsetAnnouncementChannel()
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            guildAcc.AnnouncementChannelId = 0;
            GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);
            await ReplyAsync("Now there is no Announcement-Channel anymore! No more Announcements from now on... RIP!");
        }
    }
    [Group("Welcome")]
    [Summary("DM a joining user a random message out of the ones defined.")]
    public class WelcomeMessages : ModuleBase<SocketCommandContext>
    {
        [Command("add"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Example: `welcome add <usermention>, welcome to **<guildname>**! " +
                 "Try using ```@<botname>#<botdiscriminator> help``` for all the commands of <botmention>!`\n" +
                 "Possible placeholders are: `<usermention>`, `<username>`, `<guildname>`, " +
                 "`<botname>`, `<botdiscriminator>`, `<botmention>` ")]
        public async Task AddWelcomeMessage([Remainder] string message)
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            var response = $"Failed to add this Welcome Message...";
            if (guildAcc.WelcomeMessages.Contains(message) == false)
            {
                guildAcc.WelcomeMessages.Add(message);
                GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);
                response =  $"Successfully added ```\n{message}\n``` as Welcome Message!";
            }

            await ReplyAsync(response);
        }

        [Command("remove"), Remarks("Removes a Welcome Message from the ones availabe")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveWelcomeMessage(int messageIndex)
        {
            var messages = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id).WelcomeMessages;
            var response = $"Failed to remove this Welcome Message... Use the number shown in `welcome list` next to the `#` sign!";
            if (messages.Count > messageIndex - 1)
            {
                messages.RemoveAt(messageIndex - 1);
                GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);
                response =  $"Successfully removed message #{messageIndex} as possible Welcome Message!";
            }

            await ReplyAsync(response);
        }

        [Command("list"), Remarks("Shows all currently set Welcome Messages")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ListWelcomeMessages()
        {
            var welcomeMessages = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id).WelcomeMessages;
            var embB = new EmbedBuilder().WithTitle("No Welcome Messages set yet... add some if you want to greet incoming people! =)");
            if (welcomeMessages.Count > 0) embB.WithTitle("Possible Welcome Messages:");

            for (var i = 0; i < welcomeMessages.Count; i++)
            {
                embB.AddField($"Message #{i + 1}:", welcomeMessages[i]);
            }
            await ReplyAsync("", false, embB.Build());
        }
    }

    [Group("Leave")]
    [Summary("Announce a leaving user in the set announcement channel" +
             "with a random message out of the ones defined.")
    ]
    public class LeaveMessages : ModuleBase<SocketCommandContext>
    {
        [Command("add"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Example: `leave add Oh noo! <usermention>, left <guildname>...`\n" +
                 "Possible placeholders are: `<usermention>`, `<username>`, `<guildname>`, " +
                 "`<botname>`, `<botdiscriminator>`, `<botmention>`")]
        public async Task AddLeaveMessage([Remainder] string message)
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            var response = $"Failed to add this Leave Message...";
            if (guildAcc.LeaveMessages.Contains(message) == false)
            {
                guildAcc.LeaveMessages.Add(message);
                GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);
                response =  $"Successfully added `{message}` as Leave Message!";
            }

            await ReplyAsync(response);
        }

        [Command("remove"), Remarks("Removes a Leave Message from the ones available")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveLeaveMessage(int messageIndex)
        {
            var messages = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id).LeaveMessages;
            var response = $"Failed to remove this Leave Message... Use the number shown in `leave list` next to the `#` sign!";
            if (messages.Count > messageIndex - 1)
            {
                messages.RemoveAt(messageIndex - 1);
                GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);
                response =  $"Successfully removed message #{messageIndex} as possible Welcome Message!";
            }

            await ReplyAsync(response);
        }

        [Command("list"), Remarks("Shows all currently set Leave Messages")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ListLeaveMessages()
        {
            var leaveMessages = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id).LeaveMessages;
            var embB = new EmbedBuilder().WithTitle("No Leave Messages set yet... add some if you want a message to be shown if someone leaves.");
            if (leaveMessages.Count > 0) embB.WithTitle("Possible Leave Messages:");

            for (var i = 0; i < leaveMessages.Count; i++)
            {
                embB.AddField($"Message #{i + 1}:", leaveMessages[i]);
            }
            await ReplyAsync("", false, embB.Build());
        }
    }
}
