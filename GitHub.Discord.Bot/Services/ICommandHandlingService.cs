using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace GitHub.Discord.Bot.Services
{
    public interface ICommandHandlingService
    {
        Task InitializeAsync();
        Task MessageReceivedAsync(SocketMessage rawMessage);

        Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context,
            IResult result);
    }
}