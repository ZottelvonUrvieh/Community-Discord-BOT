using CommunityBot.Features.GlobalAccounts;
using CommunityBot.Features.PublicLists;
using CommunityBot.Helpers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CommunityBot.Modules
{
    [Group("List")]
    public class Lists : ModuleBase<SocketCommandContext>
    {
        [Command("new")]
        [RequireContext(ContextType.Guild)]
        public async Task NewList()
        {
            var serverAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild);
            await ReplyAsync("What do you want to name the list?");
            var respondMsg = await Context.Channel.AwaitMessage(msg => msg.Author == Context.User);
            if (respondMsg == null) return;
            var result = PublicLists.NewList(serverAcc.PublicLists, respondMsg.Content);
            if (result == Results.Success)
            {
                GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);
                await ReplyAsync($"List {respondMsg.Content} successfully added!");
                return;
            }
            await ReplyAsync($"Could not create list \"{respondMsg.Content}\" - a list with this name already exists!");
        }

        [Command("add")]
        [RequireContext(ContextType.Guild)]
        public async Task AddItemToList([Remainder] string listName = "")
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild);
            IUserMessage botDisplayMsg = null;
            SocketMessage userRespondMsg = null;
            if (string.IsNullOrWhiteSpace(listName))
            {
                botDisplayMsg = await ReplyAsync($"These are all available lists: {string.Join(", ", guildAcc.PublicLists.Keys)}\n" +
                $"Type the name of the one you want to add an item to.");
                userRespondMsg = await Context.Channel.AwaitMessage(msg => msg.Author == Context.User);
                if (userRespondMsg == null) return;
                listName = userRespondMsg.Content;
            }
            
            if (guildAcc.PublicLists.ContainsKey(listName) == false)
            {
                await ReplyAsync($"Could not find list \"{listName}\"...!");
                return;
            }

            var list = guildAcc.PublicLists[listName];

            if (botDisplayMsg == null)
                botDisplayMsg = await ReplyAsync("What should the title of the list entry be?");
            else
                await botDisplayMsg.ModifyAsync(msg => msg.Content = "What should the title of the list entry be?");

            userRespondMsg = await Context.Channel.AwaitMessage(msg => msg.Author == Context.User);
            if (userRespondMsg == null) return;
            var title = userRespondMsg.Content;

            await botDisplayMsg.ModifyAsync(msg => msg.Content = "What should the description of the list entry be?");
            userRespondMsg = await Context.Channel.AwaitMessage(msg => msg.Author == Context.User);
            if (userRespondMsg == null) return;

            list.Add(title, userRespondMsg.Content);
            GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);

            await botDisplayMsg.ModifyAsync(msg => msg.Content = ":white_check_mark: Item successfully added!");
        }
        
        [Command("show")]
        [RequireContext(ContextType.Guild)]
        public async Task ShowList([Remainder] string listName = "")
        {
            var serverAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild);

            IUserMessage botDisplayMsg = null;
            SocketMessage userRespondMsg = null;
            if (string.IsNullOrWhiteSpace(listName))
            {
                botDisplayMsg = await ReplyAsync($"These are all available lists: {string.Join(", ", serverAcc.PublicLists.Keys)}\n" +
                $"Type the name of the one you want to show.");
                userRespondMsg = await Context.Channel.AwaitMessage(msg => msg.Author == Context.User);
                if (userRespondMsg == null) return;
                listName = userRespondMsg.Content;
            }            

            var success = serverAcc.PublicLists.TryGetValue(listName, out var list);

            if (success == false)
            {
                await ReplyAsync("List not found");
                return;
            }

            var embB = new EmbedBuilder();
            var listPaged = PublicLists.PageList(list, 1, 9);
            foreach (var entry in listPaged)
            {
                embB.AddField(entry.Key, string.IsNullOrEmpty(entry.Value) ? Constants.InvisibleString : entry.Value);
            }
            if (userRespondMsg == null)
                await ReplyAsync("", false, embB.Build());
            else
                await botDisplayMsg.ModifyAsync(msg =>
                {
                    msg.Content = "";
                    msg.Embed = embB.Build();
                });
        }
    }
}
