using System.Threading.Tasks;
using Discord;

namespace GitHub.Discord.Bot.Services
{
    public interface ILoggingService
    {
        Task OnLogEventRaised(LogMessage message);
    }
}