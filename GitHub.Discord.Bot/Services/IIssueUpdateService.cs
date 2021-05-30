using System.Threading.Tasks;

namespace GitHub.Discord.Bot.Services
{
    public interface IIssueUpdateService
    {
        public Task UpdateReferencedIssues();
    }
}