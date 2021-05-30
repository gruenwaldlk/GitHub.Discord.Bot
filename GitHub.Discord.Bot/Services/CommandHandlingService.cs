using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GitHub.Discord.Bot.GitHub.Issues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GitHub.Discord.Bot.Services
{
    public class CommandHandlingService : ICommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly ILogger<CommandHandlingService> _logger;

        public CommandHandlingService(IServiceProvider services)
        {
            _services = services;
            _commands = _services.GetRequiredService<CommandService>();
            _discord = _services.GetRequiredService<DiscordSocketClient>();
            _logger = _services.GetRequiredService<ILoggerFactory>().CreateLogger<CommandHandlingService>();
            _commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Fetching command modules...");
            _commands.AddTypeReader(typeof(RawIssue), new RawIssue.RawIssueTypeReader());
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _logger.LogInformation("Command modules loaded");
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (rawMessage is not SocketUserMessage {Source: MessageSource.User} message) return;
            int argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos) ||
                  message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) ||
                message.Author.IsBot || !Bot.BaseConfiguration.ChannelReportingConfiguration.ContainsKey(message.Channel.Name)) return;
            SocketCommandContext context = new(_discord, message);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context,
            IResult result)
        {
            _logger.LogInformation("Command {Command} executed in guild {Guild} in channel {Channel} with result {Result}", context.Message, context.Guild, context.Channel, result);
            if (!command.IsSpecified)
                return;
            if (result.IsSuccess)
                return;
            _logger.LogError("An error occurred when executing {Command}: {@Result}",context.Message, result);
            await context.Channel.SendMessageAsync($"Sorry, this shouldn't happen:\n{result}");
        }
    }
}