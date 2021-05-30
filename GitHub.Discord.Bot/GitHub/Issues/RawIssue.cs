using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using GitHub.Discord.Bot.Cfg;
using GitHub.Discord.Bot.Commons.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GitHub.Discord.Bot.GitHub.Issues
{
    public class RawIssue
    {
        public string Title { get; init; }
        public string Version { get; init; }
        public string GameMode { get; init; }
        public string Faction { get; init; }
        public string Description { get; init; }

        public static RawIssue Parse(string s, string regex, List<RequiredReportTag> requiredReportTags)
        {
            int idx = s.IndexOf("Description:", StringComparison.InvariantCultureIgnoreCase);
            idx += 12;
            string description = s[idx..];
            s = s.Remove(idx);
            Regex r = new(regex, RegexOptions.IgnoreCase);
            Match m = r.Match(s);
            if (!m.Success)
            {
                if (requiredReportTags == null)
                {
                    throw new InvalidReportException("The report is not in the expected format!");
                }
                List<string> missingTags = (from requiredReportTag in requiredReportTags
                    where !requiredReportTag.IsMatch(s)
                    select requiredReportTag.Tag).ToList();
                throw new InvalidReportException("The report is not in the expected format!", missingTags);
            }

            RawIssue rawIssue = new()
            {
                Title = m.Groups["title_content"].Value.Trim(':').Trim(),
                Version = m.Groups["version_content"].Value.Trim(':').Trim(),
                GameMode = m.Groups["game_mode_content"].Value.Trim(':').Trim(),
                Faction = m.Groups["faction_content"].Value.Trim(':').Trim(),
                Description = description.Trim(':').Trim()
            };
            return rawIssue;
        }

        public override string ToString()
        {
            return $"**Version:** {Version}\n" +
                   $"**Game Mode/Galactic Conquest:** {GameMode}\n" +
                   $"**Faction:** {Faction}\n" +
                   "\n----\n\n" +
                   $"**Description**\n{Description}";
        }

        public class RawIssueTypeReader : TypeReader
        {
            public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input,
                IServiceProvider services)
            {
                try
                {
                    RawIssue rawIssue = Parse(input,
                        Bot.BaseConfiguration.ChannelReportingConfiguration[context.Channel.Name].ReportFormat,
                        Bot.BaseConfiguration.ChannelReportingConfiguration[context.Channel.Name].RequiredReportTags
                            .ToList()
                    );
                    return Task.FromResult(TypeReaderResult.FromSuccess(rawIssue));
                }
                catch (InvalidReportException e)
                {
                    if (e.MissingReportContext.Any())
                    {
                        services.GetRequiredService<ILoggerFactory>().CreateLogger<RawIssueTypeReader>()
                            .LogError(e, "An error occurred due to missing information: {MissingContext}",
                                string.Join(", ", e.MissingReportContext));
                        return Task.FromResult(TypeReaderResult.FromError(CommandError.Unsuccessful,
                            $"Your report could not be processed, because it is missing the following information: {string.Join(", ", e.MissingReportContext)}\n" +
                            "Please use the following template to successfully report your issue:\n" +
                            $"```\n{Bot.BaseConfiguration.ChannelReportingConfiguration[context.Channel.Name].ReportTemplate}\n```"));
                    }
                    services.GetRequiredService<ILoggerFactory>().CreateLogger<RawIssueTypeReader>()
                        .LogError(e, "An error occurred");
                    return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                        "Your report could not be processed, because it is not in the required format or missing information\n" +
                        "Please use the following template to successfully report your issue:\n" +
                        $"```\n{Bot.BaseConfiguration.ChannelReportingConfiguration[context.Channel.Name].ReportTemplate}\n```"));
                }
                catch (Exception e)
                {
                    services.GetRequiredService<ILoggerFactory>().CreateLogger<RawIssueTypeReader>()
                        .LogError(e, "An error occurred");
                    return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                        "Sorry, your report could not be processed due to an internal error."));
                }
            }
        }
    }
}