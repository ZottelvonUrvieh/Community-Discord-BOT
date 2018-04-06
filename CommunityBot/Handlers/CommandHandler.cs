using System;
using System.Reflection;
using System.Threading.Tasks;
using CommunityBot.Configuration;
using Discord.Commands;
using Discord.WebSocket;

namespace CommunityBot.Handlers
{
    internal class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _service;
        private IServiceProvider _serviceProvider;
        public CommandHandler(DiscordSocketClient client, CommandService cmdService)
        {
            _client = client;
            _service = cmdService;
        }

        public async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += HandleCommandAsync;
            _client.UserJoined += _client_UserJoined;
            _client.UserLeft += _client_UserLeft;
        }

        private async Task _client_UserJoined(SocketGuildUser user)
        {
            var dmChannel = await user.GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"{user.Mention}, Welcome to **{user.Guild.Name}**. try using ``@Community-Bot#8321 help`` for all the commands!");
        }
        
        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;
            if (msg.Channel == msg.Author.GetOrCreateDMChannelAsync()) return;

            var context = new SocketCommandContext(_client, msg);
            if (context.User.IsBot) return;
            
            int argPos = 0;
            if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var cmdSearchResult = _service.Search(context, argPos);
                if (cmdSearchResult.Commands.Count == 0) return;

                var executionTask = _service.ExecuteAsync(context, argPos, _serviceProvider);

                #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                executionTask.ContinueWith(task =>
                {
                    if (!task.Result.IsSuccess && task.Result.Error != CommandError.UnknownCommand)
                    {
                        string errTemplate = "{0}, Error: {1}.";
                        string errMessage = String.Format(errTemplate, context.User.Mention, task.Result.ErrorReason);
                        context.Channel.SendMessageAsync(errMessage);
                    }
                });
                #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private async Task _client_UserLeft(SocketGuildUser user)
        {
            if (user.Guild.Name == "Discord-BOT-Tutorial")
            {
                var DiscordBotTutorial_General = _client.GetChannel(377879473644765185) as SocketTextChannel;
                await DiscordBotTutorial_General.SendMessageAsync($"{user.Username} ({user.Id}) left **{user.Guild.Name}**!");
            }
        }
    }
}