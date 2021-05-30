using GitHub.Discord.Bot.Data.Model;
using GitHub.Discord.Bot.Services;
using Octokit;

namespace GitHub.Discord.Bot.GitHub.Issues
{
    public class IssueUpdateHolder
    {
        public Issue Issue { get; init; }
        public IssueReference IssueReference { get; init; }
        public IGitHubService.UpdateAction UpdateAction { get; init; }
    }
}