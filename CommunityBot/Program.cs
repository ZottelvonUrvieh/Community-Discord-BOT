using Discord.WebSocket;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using CommunityBot.Configuration;
using CommunityBot.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;

namespace CommunityBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private IServiceProvider _serviceProvider;

        static void Main(string[] args)
        => new Program().StartAsync(args).GetAwaiter().GetResult();

        public async Task StartAsync(string[] args)
        {      
            var discordSocketConfig = HandleCommandlineArguments(args);
            if (discordSocketConfig == null) return;

            _client = new DiscordSocketClient(discordSocketConfig);

            _serviceProvider = ConfigureServices();
            
            _serviceProvider.GetRequiredService<DiscordEventHandler>().InitDiscordEvents();
            await _serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync();

            while (!await AttemptLogin()){}

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private DiscordSocketConfig HandleCommandlineArguments(string[] args)
        {
            if (args.Length > 0) args = args[0].Split(" ");

            // Help argument handling -help / -h / -info / -i
            if (args.Any(arg => new string[]{"-help", "-h", "-info", "-i"}.Contains(arg)))
            {
                Console.WriteLine(   
                    "Possible arguments you can provide are:\n" +
                    "-help | -h | -info -i  : shows this help\n" +
                    "-hl                    : run in headless mode (no output to console)\n" +
                    "-vb                    : run with verbose discord logging\n" +
                    "-token=<token>         : run with specific token instead of the saved one in bot configs\n" +
                    "-cs=<number>           : message cache size per channel (defaults to 0)"
                );
                return null;
            }

            // Headless argument handling -hl
            if (args.Contains("-hl")) Global.Headless = true;

            // Verbose argument handling -vb
            var logLevel = LogSeverity.Info;
            if (args.Contains("-vb"))
                logLevel = LogSeverity.Verbose;
            
            // Cachesize argument handling -cs=<cacheSize>
            var chacheSize = 0;
            if (args.Any(arg => arg.StartsWith("-cs=")))
            {
                var numberString = args.FirstOrDefault(arg => arg.StartsWith("-cs=")).Replace("-cs=", "");
                int.TryParse(numberString, out chacheSize);
            }

             // Token argument handling -token=YOUR.TOKEN.HERE
            var tokenString = args.FirstOrDefault(arg => arg.StartsWith("-token="));
            if (tokenString is null == false) 
            {
                BotSettings.config.Token = tokenString.Replace("-token=", "");
            }

            return new DiscordSocketConfig()
            {
                LogLevel = logLevel,
                MessageCacheSize = chacheSize,
                AlwaysDownloadUsers = true
            };
        }

        private async Task<bool> AttemptLogin()
        {
            try
            {
                await _client.LoginAsync(TokenType.Bot, BotSettings.config.Token);
                return true;
            }
            catch (HttpRequestException e)
            {
                if (e.InnerException == null)
                {
                    Console.WriteLine($"An HTTP Request exception occurred.\nMessage:\n{e.Message}");
                }
                else
                {
                    Global.WriteColoredLine($"An HTTP request ran into a problem:\n{e.InnerException.Message}",
                        ConsoleColor.Red);
                }

                var shouldTryAgain = GetTryAgainRequested();
                if (!shouldTryAgain) Environment.Exit(0);
                return false;
            }
            catch (Exception)
            {
                Console.WriteLine("An exception occurred. Your token might not be configured, or it might be wrong.");

                var shouldTryAgain = GetTryAgainRequested();
                if (!shouldTryAgain) Environment.Exit(0);
                BotSettings.LoadConfig();
                return false;
            }
        }
        
        private static bool GetTryAgainRequested()
        {
            if (Global.Headless) return false;

            Console.WriteLine("\nDo you want to try again? (y/n)");
            Global.WriteColoredLine("(not trying again closes the application)\n", ConsoleColor.Yellow);

            return Console.ReadKey().Key == ConsoleKey.Y;
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<DiscordEventHandler>()
                .BuildServiceProvider();
        }
    }
}
