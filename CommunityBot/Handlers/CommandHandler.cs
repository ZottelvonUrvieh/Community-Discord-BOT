using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using CommunityBot.Features.GlobalAccounts;
using CommunityBot.Providers;

namespace CommunityBot.Handlers
{
    public class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _service;

        public CommandHandler(DiscordSocketClient client, CommandService commandService)
        {
            _client = client;
            _service = commandService;
        }

        public async Task InitializeAsync()
        {
            await _service.AddModulesAsync(Assembly.GetEntryAssembly());
            Global.Client = _client;
        }
        
        public async Task HandleCommandAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return;
            if (msg.Channel is SocketDMChannel) return;
            
            var context = new SocketCommandContext(_client, msg);
            if (context.User.IsBot) return;

            await RoleByPhraseProvider.EvaluateMessage(
                context.Guild, 
                context.Message.Content,
                (SocketGuildUser) context.User
            );

            var argPos = 0;
            if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos) || CheckPrefix(ref argPos, context))
            {
                var cmdSearchResult = _service.Search(context, argPos);
                if (cmdSearchResult.Commands.Count == 0) return;

                var executionTask = _service.ExecuteAsync(context, argPos);

                #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                executionTask.ContinueWith(task =>
                {
                    if (task.Result.IsSuccess || task.Result.Error == CommandError.UnknownCommand) return;
                    const string errTemplate = "{0}, Error: {1}.";
                    var errMessage = string.Format(errTemplate, context.User.Mention, task.Result.ErrorReason);
                    context.Channel.SendMessageAsync(errMessage);
                });
                #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private static bool CheckPrefix(ref int argPos, SocketCommandContext context)
        {
            var prefixes = GlobalGuildAccounts.GetGuildAccount(context.Guild.Id).Prefixes;
            var tmpArgPos = 0;
            var success = prefixes.Any(pre =>
            {
                if (!context.Message.Content.StartsWith(pre)) return false;
                tmpArgPos = pre.Length;
                return true;
            });
            argPos = tmpArgPos;
            return success;
        }
    }
}