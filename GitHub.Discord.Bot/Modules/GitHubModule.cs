using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GitHub.Discord.Bot.Data.Model;
using GitHub.Discord.Bot.GitHub.Issues;
using GitHub.Discord.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Emoji = Discord.Emoji;

namespace GitHub.Discord.Bot.Modules
{
    public class GitHubModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<GitHubModule> _logger;

        public GitHubModule(IServiceProvider services)
        {
            _services = services;
            _logger = _services.GetRequiredService<ILoggerFactory>().CreateLogger<GitHubModule>();
        }

        [Command("report")]
        [Alias("r", "issue")]
        [Summary("Reports an issue on GitHub.")]
        public async Task ReportIssueAsync([Remainder] RawIssue rawIssue)
        {
            if (!Bot.BaseConfiguration.ChannelReportingConfiguration.ContainsKey(Context.Message.Channel.Name))
            {
                return;
            }

            if (Context.Message is not IUserMessage userMessage) return;
            try
            {
                List<string> attachmentUrls = userMessage.Attachments.Select(attachment => attachment.Url).ToList();
                IssueReference reference = await _services.GetRequiredService<IGitHubService>().CreateNewIssueAsync(
                    rawIssue,
                    new IssueReference(Context.Guild.Id, userMessage.Channel.Id, userMessage.Id),
                    Context.Guild.Name, Context.Channel.Name,
                    $"{userMessage.Author.Username}#{userMessage.Author.Discriminator}", userMessage.Timestamp,
                    attachmentUrls);
                await userMessage.AddReactionAsync(new Emoji(Bot.BaseConfiguration
                    .ChannelReportingConfiguration[Context.Channel.Name]
                    .IssueCreationReactionEmojiUnicode));
                EmbedBuilder builder = new()
                {
                    Color = Color.Green,
                    Description = Bot.BaseConfiguration.ChannelReportingConfiguration[Context.Channel.Name]
                        .SendIssueLink
                        ? $"{new Emoji(Bot.BaseConfiguration.ChannelReportingConfiguration[Context.Channel.Name].IssueCreationReactionEmojiUnicode)} I've reported your issue [\"{rawIssue.Title}\"]({reference.GitHubIssueHtmlUrl}) on GitHub."
                        : $"{new Emoji(Bot.BaseConfiguration.ChannelReportingConfiguration[Context.Channel.Name].IssueCreationReactionEmojiUnicode)} I've reported your issue \"{rawIssue.Title}\" on GitHub."
                };
                if (Bot.BaseConfiguration.ChannelReportingConfiguration[Context.Channel.Name]
                    .ReplyToIssueAuthorOnReport)
                {
                    await userMessage.ReplyAsync(
                        "", false, builder.Build(),
                        Bot.BaseConfiguration.ChannelReportingConfiguration[Context.Channel.Name]
                            .MentionIssueAuthorOnReport
                            ? null
                            : AllowedMentions.None);
                }
            }
            catch (Exception e)
            {
                EmbedBuilder builder = new()
                {
                    Color = Color.Red,
                    Title = $"{new Emoji("‼️")} An error occurred",
                    Description = "Your report could not be processed due to an internal error."
                };
                _logger.LogError(e, "An error occurred");
                await userMessage.ReplyAsync(
                    "", false, builder.Build(), Bot.BaseConfiguration
                        .ChannelReportingConfiguration[Context.Channel.Name].MentionIssueAuthorOnReport
                        ? null
                        : AllowedMentions.None);
            }
        }
#if DEBUG
        [Command("ts")]
        public async Task SyncAsync()
        {
            try
            {
                await _services.GetRequiredService<IIssueUpdateService>().UpdateReferencedIssues();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Oops!");
            }
        }
#endif
    }
}