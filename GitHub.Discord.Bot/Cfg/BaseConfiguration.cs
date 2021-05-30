using System.Collections.Generic;

namespace GitHub.Discord.Bot.Cfg
{
    public class BaseConfiguration
    {
        public string ProductName { get; set; }
        public string DiscordToken { get; set; }
        public string SentryToken { get; set; }
        public string GitHubOAuthToken { get; set; }
        public int GitHubSyncInterval { get; set; }
        public string DbConnectionString { get; set; }
        
        public IEnumerable<string> EmbedImageFormats { get; set; }
        public ChannelReportingConfiguration ChannelReportingConfiguration { get; set; }
    }
}