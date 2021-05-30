using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;

namespace GitHub.Discord.Bot.Data.Model
{
    [Table("GDB_ISSUE_REFERENCE")]
    public class IssueReference
    {
        [Column("ISSUE_REFERENCE_NR"), Required, Key]
        public long IssueReferenceId { get; set; }

        [Column("DSC_GUILD_ID"), Required]
        public ulong GuildId { get; set; }
        
        [Column("DSC_TEXT_CHANNEL_ID"), Required]
        public ulong TextChannelId { get; set; }
        
        [Column("DSC_MESSAGE_ID"), Required]
        public ulong MessageId { get; set; }
        
        [Column("GH_ISSUE_NR"), Required]
        public int GitHubIssueNr { get; set; }
        
        [Column("GH_REPOSITORY_ID"), Required]
        public long GitHubRepositoryId { get; set; }
        
        [Column("EVT_UPDATED"), Required]
        public DateTimeOffset EvtUpdated { get; set; }
        [Column("GH_ISSUE_HTML_URL"), Required]
        public string GitHubIssueHtmlUrl { get; set; }

        public IssueReference(ulong guildId, ulong textChannelId, ulong messageId)
        {
            GuildId = guildId;
            TextChannelId = textChannelId;
            MessageId = messageId;
        }

        public class IssueReferenceValidator : AbstractValidator<IssueReference>
        {
            public IssueReferenceValidator()
            {
                RuleFor(reference => reference.GuildId).NotEqual(0u);
                RuleFor(reference => reference.TextChannelId).NotEqual(0u);
                RuleFor(reference => reference.MessageId).NotEqual(0u);
                RuleFor(reference => reference.GitHubIssueNr).NotEqual(0);
                RuleFor(reference => reference.GitHubRepositoryId).NotEqual(0);
                RuleFor(reference => reference.EvtUpdated).NotNull();
                RuleFor(reference => reference.GitHubIssueHtmlUrl).NotNull().NotEmpty().Must(issueUrl =>
                    Uri.TryCreate(issueUrl, UriKind.Absolute, out Uri uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
                ).WithMessage("The provided value is not a valid http/https url.");
            }
        }
    }
}