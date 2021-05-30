using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Discord.Bot.Data.Model;
using GitHub.Discord.Bot.GitHub.Issues;

namespace GitHub.Discord.Bot.Services
{
    public interface IGitHubService
    {
        public enum UpdateAction
        {
            IssueClosed
        }

        public Task<IssueReference> CreateNewIssueAsync(RawIssue rawIssue, IssueReference reference, string guildName,
            string channelName, string userName, DateTimeOffset timestamp, IList<string> attachmentUrls);

        public Task<IEnumerable<IssueUpdateHolder>> FetchIssueUpdatesAsync();
    }
}