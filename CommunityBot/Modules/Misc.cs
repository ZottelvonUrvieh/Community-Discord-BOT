using CommunityBot.Preconditions;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityBot.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        
        public Misc(CommandService service)
        {
            _service = service;
        }

        [Command("help")]
        [Cooldown(15)]
        public async Task Help()
        {

            await Context.Channel.SendMessageAsync("Check your DMs.");

            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();

            var builder = new EmbedBuilder()
            {
                Title = "Help",
                Description = "These are the commands you can use",
                Color = new Color(114, 137, 218)
            };

            foreach (var module in _service.Modules)
            {
                string description = null;
                var descriptionBuilder = new List<string>();
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess && !descriptionBuilder.Contains(cmd.Aliases.First()))
                        descriptionBuilder.Add(cmd.Aliases.First());
                }
                description = String.Join("\n", descriptionBuilder);

                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }
            await dmChannel.SendMessageAsync("", false, builder.Build());
        }

        [Command("help")]
        [Remarks("Shows what a specific command does and what parameters it takes.")]
        [Cooldown(5)]
        public async Task HelpCommand(string command)
        {
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like {command}.");
                return;
            }

            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = $"Here are commands related to {command}"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                                $"Remarks: {cmd.Remarks}";
                    x.IsInline = false;
                });
            }
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("Addition")]
        [Summary("Adds 2 numbers together.")]
        public async Task AddAsync(int num1, int num2)
        {
            await ReplyAsync($"The Answer To That Is: {num1 + num2}");
        }
        
        [Command("Subtract")]
        [Summary("Subtracts 2 numbers.")]
        public async Task SubstractAsync(int num1, int num2)
        {
            await ReplyAsync($"The Answer To That Is: {num1 - num2}");
        }

        [Command("Multiply")]
        [Summary("Multiplys 2 Numbers.")]
        public async Task MultiplyAsync(int num1, int num2)
        {
            await ReplyAsync($"The Answer To That Is {num1 * num2}");
        }

        [Command("Divide")]
        [Summary("Divides 2 Numbers.")]
        public async Task DivideAsync(int num1, int num2)
        {
            await ReplyAsync($"The Answer To That Is: {num1 / num2}");
        }
    }
}
