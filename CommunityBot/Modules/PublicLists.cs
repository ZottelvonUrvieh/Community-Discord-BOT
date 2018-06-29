using CommunityBot.Features.GlobalAccounts;
using CommunityBot.Helpers;
using CommunityBot.Features.PublicLists;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace CommunityBot.Modules
{
    [Group("List")]
    [RequireContext(ContextType.Guild)]
    public class Lists : ModuleBase<SocketCommandContext>
    {
        [Command("new")]
        public async Task NewList([Remainder] string listName = "")
        {
            var serverAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild);
            if (listName == "")
            {
                await ReplyAsync("What do you want to name the list?");
                var respondMsg = await Context.Channel.AwaitMessage(msg => msg.Author == Context.User);
                if (respondMsg == null) return;
                listName = respondMsg.Content;
            }
            var result = PublicLists.NewList(serverAcc.PublicLists, listName);
            if (result == Results.Success)
            {
                GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);
                await ReplyAsync($"List {listName} successfully added!");
                return;
            }
            await ReplyAsync($"Could not create list \"{listName}\" - a list with this name already exists!");
        }

        [Command("add")]
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

        [Command("remove")]
        public async Task RemoveItemFromList([Remainder] string listName = "")
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild);
            IUserMessage botDisplayMsg = null;
            SocketMessage userRespondMsg = null;
            if (string.IsNullOrWhiteSpace(listName))
            {
                botDisplayMsg = await ReplyAsync($"These are all available lists: {string.Join(", ", guildAcc.PublicLists.Keys)}\n" +
                $"Type the name of the one you want to remove an item from.");
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

            await ShowList(listName);

            botDisplayMsg = await ReplyAsync("Which item you want to remove? (Type the title)");

            userRespondMsg = await Context.Channel.AwaitMessage(msg => msg.Author == Context.User);
            if (userRespondMsg == null) return;
            var title = userRespondMsg.Content;

            list.Remove(title);
            GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);

            await botDisplayMsg.ModifyAsync(msg => msg.Content = ":white_check_mark: Item successfully removed!");
        }

        [Command("show")]
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

            var embB = new EmbedBuilder()
                .WithTitle(listName);
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
