using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using GitHub.Discord.Bot.Commons.Exceptions;
using GitHub.Discord.Bot.Data.Context;
using GitHub.Discord.Bot.Data.Model;
using GitHub.Discord.Bot.GitHub.Issues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;

namespace GitHub.Discord.Bot.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<GitHubService> _logger;

        public GitHubService(IServiceProvider services)
        {
            _services = services;
            _logger = _services.GetRequiredService<ILoggerFactory>().CreateLogger<GitHubService>();
        }

        public async Task<IssueReference> CreateNewIssueAsync(RawIssue rawIssue, IssueReference reference,
            string guildName, string channelName,
            string userName, DateTimeOffset timestamp, IList<string> attachmentUrls)
        {
            GitHubClient client = new(new ProductHeaderValue(Bot.BaseConfiguration.ProductName))
            {
                Credentials = new Credentials(Bot.BaseConfiguration.GitHubOAuthToken)
            };
            Repository repositoryContext = await client.Repository.Get(
                Bot.BaseConfiguration.ChannelReportingConfiguration[channelName].GitHubRepositoryOwner,
                Bot.BaseConfiguration.ChannelReportingConfiguration[channelName].GitHubRepositoryName);
            if (repositoryContext == null)
            {
                throw new InvalidConfigurationException(
                    $"The following repository cannot be resolved: " +
                    $"{Bot.BaseConfiguration.ChannelReportingConfiguration[channelName].GitHubRepositoryOwner}" +
                    $"/{Bot.BaseConfiguration.ChannelReportingConfiguration[channelName].GitHubRepositoryName}");
            }

            StringBuilder bodyBuilder = new();
            bodyBuilder.Append(rawIssue);
            if (attachmentUrls.Any())
            {
                bodyBuilder.AppendLine("\n\n----\n\n**Attachments:**");
                foreach (string attachmentUrl in attachmentUrls)
                {
                    if (!string.IsNullOrWhiteSpace(attachmentUrl))
                    {
                        bodyBuilder.AppendLine(IsImageAttachment(attachmentUrl)
                            ? $"![Image Attachment]({attachmentUrl})"
                            : $"- {attachmentUrl}");
                    }
                }
            }

            if (Bot.BaseConfiguration.ChannelReportingConfiguration[channelName]
                .IncludeDiscordMessageReference)
            {
                bodyBuilder.AppendLine(
                    $"\n\n----\n\n**Source:** {timestamp:F} - **{guildName}** in **#{channelName}** by **{userName}**");
            }

            NewIssue createIssue =
                new(string.IsNullOrWhiteSpace(Bot.BaseConfiguration.ChannelReportingConfiguration[channelName]
                        .IssuePrefix)
                        ? rawIssue.Title
                        : $"{Bot.BaseConfiguration.ChannelReportingConfiguration[channelName].IssuePrefix}: {rawIssue.Title}")
                    {Body = bodyBuilder.ToString()};
            foreach (string label in Bot.BaseConfiguration.ChannelReportingConfiguration[channelName].Labels)
            {
                createIssue.Labels.Add(label);
            }

            Issue issue = await client.Issue.Create(
                Bot.BaseConfiguration.ChannelReportingConfiguration[channelName].GitHubRepositoryOwner,
                Bot.BaseConfiguration.ChannelReportingConfiguration[channelName].GitHubRepositoryName,
                createIssue);
            if (issue == null)
            {
                throw new IssueCouldNotBeCreatedException();
            }

            try
            {
                reference.GitHubIssueNr = issue.Number;
                reference.GitHubRepositoryId = repositoryContext.Id;
                reference.EvtUpdated = issue.CreatedAt;
                reference.GitHubIssueHtmlUrl = issue.HtmlUrl;
                await new IssueReference.IssueReferenceValidator().ValidateAndThrowAsync(reference);
                _services.GetRequiredService<BotContext>().Add(reference);
                await _services.GetRequiredService<BotContext>().SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "An error occurred while persisting the issue in the internal database. " +
                    "This does not affect the issue report, but we can no longer update the issue in discord");
            }

            _logger.LogInformation("Created new issue {@Issue}", issue);
            return reference;
        }

        public async Task<IEnumerable<IssueUpdateHolder>> FetchIssueUpdatesAsync()
        {
            List<IssueReference> currentReferences =
                _services.GetRequiredService<BotContext>().IssueReferences.ToList();
            List<IssueUpdateHolder> referencesToUpdate = new();
            GitHubClient client = new(new ProductHeaderValue(Bot.BaseConfiguration.ProductName))
            {
                Credentials = new Credentials(Bot.BaseConfiguration.GitHubOAuthToken)
            };
            foreach (IssueReference reference in currentReferences)
            {
                Issue issue = await client.Issue.Get(reference.GitHubRepositoryId, reference.GitHubIssueNr);
                if (issue == null) continue;
                if (!(issue.UpdatedAt > reference.EvtUpdated)) continue;
                _logger.LogInformation("Issue updated: {@GitHubIssue}, {@IssueReference}", issue, reference);
                
                if (issue.ClosedAt != null)
                {
                    referencesToUpdate.Add(new IssueUpdateHolder
                    {
                        Issue = issue,
                        IssueReference = reference,
                        UpdateAction = IGitHubService.UpdateAction.IssueClosed
                    });
                }
            }

            return referencesToUpdate;
        }

        private static bool IsImageAttachment(string attachmentUrl)
        {
            return Bot.BaseConfiguration.EmbedImageFormats.Any(imageFormat =>
                attachmentUrl.EndsWith(imageFormat, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}