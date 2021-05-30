using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GitHub.Discord.Bot.Modules
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<InfoModule> _logger;

        public InfoModule(IServiceProvider services)
        {
            _services = services;
            _logger = _services.GetRequiredService<ILoggerFactory>().CreateLogger<InfoModule>();
        }

#if DEBUG
        [Command("ping")]
        [Alias("pong", "hello", "p")]
        [Summary("A classic debug command.")]
        public Task PingAsync()
        {
            _logger.LogInformation("{CommandFunction} executed", nameof(PingAsync));
            return ReplyAsync("Hello there!");
        }
#endif

        [Command("help")]
        [Alias("h")]
        [Summary("Generates this help.")]
        public async Task HelpAsync()
        {
            List<CommandInfo> commands =
                _services.GetRequiredService<CommandService>().Commands.ToList();
            EmbedBuilder embedBuilder = new();
            foreach (CommandInfo command in commands)
            {
                string embedFieldText = command.Summary;
                embedBuilder.AddField(command.Name, embedFieldText);
            }

            await ReplyAsync("Here is a list of all available commands:", false,
                embedBuilder.Build());
        }
    }
}