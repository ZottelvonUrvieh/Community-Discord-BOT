using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using CommunityBot.Configuration;
using CommunityBot.Handlers;
using CommunityBot.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using CommunityBot.Features.GlobalAccounts;
using CommunityBot.Features.Audio;

namespace CommunityBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandHandler _handler;

        static void Main(string[] args)
        => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            _client = CreateClient();

            var serviceProvider = ConfigureServices();

            _client.Log += Logger.Log;
            _client.Ready += Timers.StartTimer;
            _client.ReactionAdded += OnReactionAdded;
            _client.MessageReceived += MessageRewardHandler.HandleMessageRewards;
            // Subscribe to other events here.

            await serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync(serviceProvider);
            await AttemptLogin();
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!reaction.User.Value.IsBot)
            {
                var msgList = Global.MessagesIdToTrack ?? new Dictionary<ulong, string>();
                if (msgList.ContainsKey(reaction.MessageId))
                {
                    if (reaction.Emote.Name == "➕")
                    {
                        var item = msgList.FirstOrDefault(k => k.Key == reaction.MessageId);
                        var embed = BlogHandler.SubscribeToBlog(reaction.User.Value.Id, item.Value);
                    }
                }
            }

            return Task.CompletedTask;
        }

        private async Task AttemptLogin()
        {
            try
            {
                await _client.LoginAsync(TokenType.Bot, BotSettings.config.Token);
            }
            catch (Exception e)
            {
                Console.WriteLine("The BOT Token is most likely incorrect.");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        private DiscordSocketClient CreateClient()
        {
            return new DiscordSocketClient(
                new DiscordSocketConfig()
                {
                    LogLevel = LogSeverity.Verbose
                });
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<AudioService>()
                .BuildServiceProvider();
        }
    }
}
