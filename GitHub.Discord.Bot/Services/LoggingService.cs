using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GitHub.Discord.Bot.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(IServiceProvider services)
        {
            CommandService commands = services.GetRequiredService<CommandService>();
            DiscordSocketClient discord = services.GetRequiredService<DiscordSocketClient>();
            _logger = services.GetService<ILoggerFactory>().CreateLogger<LoggingService>();
            discord.Log += OnLogEventRaised;
            commands.Log += OnLogEventRaised;
        }

        public Task OnLogEventRaised(LogMessage message)
        {
#if DEBUG
            _logger.LogInformation("Log event raised: {@LogMessage}", message);
#endif
            switch (message.Exception)
            {
                case CommandException cmdException:
                    switch (message.Severity)
                    {
                        case LogSeverity.Critical:
                            _logger.LogCritical("{@CommandException}", cmdException);
                            break;
                        case LogSeverity.Error:
                            _logger.LogError("{@CommandException}", cmdException);
                            break;
                        case LogSeverity.Warning:
                            _logger.LogWarning("{@CommandException}", cmdException);
                            break;
                        case LogSeverity.Info:
                            _logger.LogInformation("{@CommandException}", cmdException);
                            break;
                        default:
                            _logger.LogDebug("{@CommandException}", cmdException);
                            break;
                    }

                    break;
                default:
                    switch (message.Severity)
                    {
                        case LogSeverity.Critical:
                            _logger.LogCritical("{Message}", message.Message);
                            break;
                        case LogSeverity.Error:
                            _logger.LogError("{Message}", message.Message);
                            break;
                        case LogSeverity.Warning:
                            _logger.LogWarning("{Message}", message.Message);
                            break;
                        case LogSeverity.Info:
                            _logger.LogInformation("{Message}", message.Message);
                            break;
                        default:
                            _logger.LogDebug("{Message}", message.Message);
                            break;
                    }

                    break;
            }

            return Task.CompletedTask;
        }
    }
}