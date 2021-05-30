using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GitHub.Discord.Bot.Data.Context;
using GitHub.Discord.Bot.GitHub.Issues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;
using Emoji = Discord.Emoji;

namespace GitHub.Discord.Bot.Services
{
    public class IssueUpdateService : IIssueUpdateService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<IssueUpdateService> _logger;

        public IssueUpdateService(IServiceProvider services)
        {
            _services = services;
            _logger = _services.GetRequiredService<ILoggerFactory>().CreateLogger<IssueUpdateService>();
        }
        
        public async Task UpdateReferencedIssues()
        {
            IEnumerable<IssueUpdateHolder> issuesToUpdate = await _services.GetRequiredService<IGitHubService>().FetchIssueUpdatesAsync();
            if (!issuesToUpdate.Any())
            {
                return;
            }

            foreach (IssueUpdateHolder issueUpdateHolder in issuesToUpdate)
            {
                switch (issueUpdateHolder.UpdateAction)
                {
                    case IGitHubService.UpdateAction.IssueClosed:
                        _logger.LogInformation("The issue {@Issue} has been closed", issueUpdateHolder.Issue);
                        await UpdateIssueClosed(issueUpdateHolder);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async Task UpdateIssueClosed(IssueUpdateHolder issueUpdateHolder)
        {
            DiscordSocketClient client = _services.GetRequiredService<DiscordSocketClient>();
            string channel = client.GetGuild(issueUpdateHolder.IssueReference.GuildId)
                .GetTextChannel(issueUpdateHolder.IssueReference.TextChannelId).Name;
            IAsyncEnumerable<IReadOnlyCollection<IMessage>> msg = client.GetGuild(issueUpdateHolder.IssueReference.GuildId).GetTextChannel(issueUpdateHolder.IssueReference.TextChannelId).GetMessagesAsync(issueUpdateHolder.IssueReference.MessageId, Direction.Around, 1, RequestOptions.Default);
            
            await foreach (IReadOnlyCollection<IMessage> m in msg)
            {
                foreach (IMessage message in m)
                {
                    if (message is not IUserMessage userMessage) continue;
                    await userMessage.AddReactionAsync(new Emoji(Bot.BaseConfiguration.ChannelReportingConfiguration[channel]
                        .IssueClosedReactionEmojiUnicode));
                    EmbedBuilder builder = new()
                    {
                        Color = Color.Green,
                        Description = Bot.BaseConfiguration.ChannelReportingConfiguration[channel].SendIssueLink
                            ? $"{new Emoji(Bot.BaseConfiguration.ChannelReportingConfiguration[channel].IssueClosedReactionEmojiUnicode)} Your issue [\"{issueUpdateHolder.Issue.Title}\"]({issueUpdateHolder.Issue.HtmlUrl}) has been closed."
                            : $"{new Emoji(Bot.BaseConfiguration.ChannelReportingConfiguration[channel].IssueClosedReactionEmojiUnicode)} Your issue \"{issueUpdateHolder.Issue.Title}\" has been closed.",
                        
                    };
                    if (issueUpdateHolder.Issue.Labels.Any())
                    {
                        List<string> labels = issueUpdateHolder.Issue.Labels.Select(label => label.Name).ToList();
                        builder.AddField("Labels", string.Join(", ",labels));
                    }
                    await userMessage.ReplyAsync(
                        "", false, builder.Build(),
                        Bot.BaseConfiguration.ChannelReportingConfiguration[channel]
                            .MentionIssueAuthorOnClose
                            ? null
                            : AllowedMentions.None);
                }
            }
            _services.GetRequiredService<BotContext>().IssueReferences.Remove(issueUpdateHolder.IssueReference);
            await _services.GetRequiredService<BotContext>().SaveChangesAsync();
        }
    }
}