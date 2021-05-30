using System.Collections.Generic;

namespace GitHub.Discord.Bot.Cfg
{
    public class ReportingConfiguration
    {
        public string ChannelName { get; set; }
        public string GitHubRepositoryOwner { get; set; }
        public string GitHubRepositoryName { get; set; }
        public string IssuePrefix { get; set; }

        public bool CheckForDuplicates { get; set; }
        public IEnumerable<string> Labels { get; set; }
        public bool ReplyToIssueAuthorOnReport { get; set; }
        public bool ReplyToIssueAuthorOnClose { get; set; }
        public bool MentionIssueAuthorOnReport { get; set; }
        public bool MentionIssueAuthorOnClose { get; set; }
        public bool SendIssueLink { get; set; }
        public bool IncludeDiscordMessageReference { get; set; } //TODO [gruenwaldlu, 2021-05-23-13:09:29+2]: Rename to IncludeDiscordMessageReferenceInGitHubIssueReport
        public string ReportFormat { get; set; }
        
        public IEnumerable<RequiredReportTag> RequiredReportTags { get; set; }
        public string ReportTemplate { get; set; }
        public string IssueCreationReactionEmojiUnicode { get; set; }
        public string IssueClosedReactionEmojiUnicode { get; set; }
    }
}